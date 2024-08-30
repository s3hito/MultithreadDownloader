using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Globalization;
using System.Threading;
using static MultithreadDownloader.ProxyManager;
using System.Configuration;
using static MultithreadDownloader.DownloadThread;
using System.ComponentModel;

namespace MultithreadDownloader 
{

    public class DownloadController : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;


        private string _filename="testfile";
        private string _url;
        private string Path;
        private string PathToTempFolder;
        private HttpWebRequest request;
        private WebResponse responce;
        private int _tnumber;
        private int _timeoutms=2000;
        private string _status;
        private long _byteslength; 
        private long _sectionlenth;
        private long LastPiece;
        private long _totalprogress;
        private double _progresspercent;
        private double _downloadspeed;
        private ConsoleDrawer Drawer;
        private bool Locked=false;
        private string DownloadBaseInfo;
        private bool DownloadFinished = false;


        private List<DownloadThread> _threadlist = new List<DownloadThread>();
        private List<DownloadThread> OldThreadList = new List<DownloadThread>();
        private List<Task> tasks = new List<Task>();

        private FileManager FMan;
        private ProxyManager ProxyDistributor;
        private List<string> ProxyList;

        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };


        public string Filename { get { return _filename; } set {_filename=value; OnPropertyChanged("Filename"); } }
        public string Size { get { return SizeSuffix(_byteslength); } }
        public double ProgressPercentage { get { return _progresspercent; } set { _progresspercent = value; OnPropertyChanged("ProgressPercentage"); } } //
        public string URL { get { return _url; } }
        public int ThreadNumber { get { return _tnumber; } }
        public int TimeOutMs { get { return _timeoutms; } set { _timeoutms = value; } }
        public string Status { get { return _status; }  set { _status = value; OnPropertyChanged("Status"); } }
        public long BytesLength { get { return _byteslength; } }
        public long SectionLength { get { return _sectionlenth; } }
        public long TotalProgress {  get { return _totalprogress; } }
        public List<DownloadThread> ThreadList { get { return _threadlist; } }






        public DownloadController( string url, int _tnum, FileManager fileman, KeyValueConfigurationCollection config, bool useconsole=true)
        {
            Status = "Idle";
            _url = url;
            _tnumber = _tnum;
            Path = config["Path"].Value;
            ProxyList=fileman.FetchProxyFile();
            ProxyDistributor = new ProxyManager(
                (ProxyDistributionStates)Enum.Parse(typeof(ProxyDistributionStates), config["ProxyRule"].Value),
                ProxyList,
                (OutOfProxyBehaviourStates)Enum.Parse(typeof(OutOfProxyBehaviourStates), config["OutOfProxyRule"].Value));


            request = (HttpWebRequest)WebRequest.Create(URL);
            responce = request.GetResponse();
            _byteslength = responce.ContentLength;
            _sectionlenth = _byteslength / _tnumber;

            TryGetName();
            FMan = fileman;
            FMan.SetValues(_filename, Path, _tnumber);

            FMan.CreateDirectory();
            PathToTempFolder = Path + "\\" + _filename + ".temp";

            if (useconsole)
            {
                Drawer = new ConsoleDrawer(DownloadBaseInfo, this);

            }

        }

        void OnPropertyChanged(string PropName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropName));
        }

        public async Task PrintData()
        {


            SplitIntoSections();
             Start();


        }

        private void TryGetName()
        {
            if (responce.Headers["Content-Disposition"]==null) 
            { 
                _filename = URL.Split("/").Last();
            }
            else _filename = responce.Headers["Content-Disposition"].Replace("attachment; filename=", String.Empty).Replace("\"", String.Empty);
        }
        public void SplitIntoSections()
        {
            LastPiece = BytesLength % _tnumber;
            for (int i = 0; i < _tnumber; i++)
            {
                ThreadList.Add(new DownloadThread(URL, i * _sectionlenth, ((i + 1) * _sectionlenth) - 1, $"{_filename}.temp{i}", PathToTempFolder, ProxyDistributor));
            }
            ThreadList[_tnumber - 1].End = ThreadList[_tnumber - 1].End + LastPiece ; //Maybe add "+1" here

        }
        public async Task Start()
        {
            Status = "Downloading";
            foreach (DownloadThread thread in ThreadList)
            {
                tasks.Add(thread.StartThreadAsync());
            }

            await ServicesLauncher();
            Status = "Finished";
        }

        public async Task ServicesLauncher()
        {
            Task.Run(UpdateProgress);
            if (Drawer != null)
            {
                Task.Run(Drawer.Start);

            }
            Task.Run(DownloadWatcher);
            await Task.Run(TaskWatcher);


            DownloadFinished = true;
            
            if(Drawer != null)
            {
                Drawer.Stop();

            }
            FMan.CombineTempFiles();
            FMan.RemoveDirectory();
        }

        public async Task TaskWatcher()
        {
            bool flag=false;
            while (!DownloadFinished)
            {

                foreach (DownloadThread download in ThreadList)
                {

                    if (download.DownloadStatus==DownloadStates.Finished)
                    {
                        flag = true;
                    }
                    else
                    {
                        flag=false;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
                Thread.Sleep(100);
            }


        }

        public async Task DownloadWatcher()
        {
            OldThreadList = ThreadList.Select(x=>x.Copy()).ToList();
            Thread.Sleep(TimeOutMs);
            while (!DownloadFinished)
            {
                for (int i=0;i<_tnumber;i++)
                {
                    if (ThreadList[i].ProgressAbsolute == OldThreadList[i].ProgressAbsolute && ThreadList[i].DownloadStatus!=DownloadStates.Finished)
                    {
                        ThreadList[i].Suspended = true;
                        ThreadList[i].CloseFileStream();
                        ThreadList[i] = new DownloadThread(URL, ThreadList[i].ProgressAbsolute, ThreadList[i].End, $"{_filename}.temp{i}", PathToTempFolder,ProxyDistributor, ThreadList[i].Proxy , ThreadList[i].Accumulated, ThreadList[i].ReconnectCount);
                        ThreadList[i].CanClearLine = true;
                        tasks[i] = ThreadList[i].StartThreadAsync();
                    }
                }
                OldThreadList = ThreadList.Select(x => x.Copy()).ToList();
                Thread.Sleep(TimeOutMs);
            }
        }

        private async Task UpdateProgress()
        {
            while (!DownloadFinished)
            {
                _totalprogress = 0;
                
                foreach (DownloadThread download in ThreadList)
                {
                    _totalprogress += download.Accumulated;
                }
                ProgressPercentage = (((double)TotalProgress / (double)BytesLength) * 100);
                Thread.Sleep(10);
            }
           
        }

        static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }


    }
}

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

namespace MultithreadDownloader
{

    public class DownloadController
    {
        
        private string Filename="testfile";
        private string URL;
        private string Path;
        private string PathToTempFolder;
        HttpWebRequest request;
        WebResponse responce;
        private int TNumber;
        public int TimeOutMs=2000;
        public long BytesLength; //set to private later
        private long SectionLength;
        private long LastPiece;
        public long TotalProgress;
        ConsoleDrawer Drawer;
        public bool Locked=false;
        private string DownloadBaseInfo;
        private bool DownloadFinished = false;


        public List<DownloadThread> ThreadList = new List<DownloadThread>();
        private List<DownloadThread> OldThreadList = new List<DownloadThread>();
        List<Task> tasks = new List<Task>();

        FileManager FMan;
        ProxyManager ProxyDistributor;
        private List<string> ProxyList;

        

        public DownloadController( string _url, int _tnum, FileManager fileman, KeyValueConfigurationCollection config)
        {
            
            URL = _url;
            TNumber = _tnum;
            Path = config["Path"].Value;
            ProxyList=fileman.FetchProxyFile();
            ProxyDistributor = new ProxyManager(
                (ProxyDistributionStates)Enum.Parse(typeof(ProxyDistributionStates), config["ProxyRule"].Value),
                ProxyList,
                (OutOfProxyBehaviourStates)Enum.Parse(typeof(OutOfProxyBehaviourStates), config["OutOfProxyRule"].Value));


            request = (HttpWebRequest)WebRequest.Create(URL);
            responce = request.GetResponse();
            BytesLength = responce.ContentLength;
            SectionLength = BytesLength / TNumber;

            TryGetName();
            FMan = fileman;
            FMan.SetValues(Filename, Path, TNumber);

            FMan.CreateDirectory();
            PathToTempFolder = Path + "\\" + Filename + ".temp";

            DownloadBaseInfo = 
                $"{URL} \n" +
                $"Length: {BytesLength} bytes ~= {BytesLength / 1024 / 1024} Mb " +
                $"Chunk size: {SectionLength} \n" +
                $"Number of threads: {TNumber}";
            Drawer = new ConsoleDrawer(DownloadBaseInfo, this);

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
                Filename = URL.Split("/").Last();
            }
            else Filename = responce.Headers["Content-Disposition"].Replace("attachment; filename=", String.Empty).Replace("\"", String.Empty);
        }
        public void SplitIntoSections()
        {
            LastPiece = BytesLength % TNumber;
            for (int i = 0; i < TNumber; i++)
            {
                ThreadList.Add(new DownloadThread(URL, i * SectionLength, ((i + 1) * SectionLength) - 1, $"{Filename}.temp{i}", PathToTempFolder, ProxyDistributor));
            }
            ThreadList[TNumber - 1].End = ThreadList[TNumber - 1].End + LastPiece ; //Maybe add "+1" here

        }
        public async Task Start()
        {
            foreach (DownloadThread thread in ThreadList)
            {
                tasks.Add(thread.StartThreadAsync());
            }

            await ServicesLauncher();
            Console.WriteLine("Done");
        }

        public async Task ServicesLauncher()
        {

            Task.Run(Drawer.Start);
            Task.Run(DownloadWatcher);
            await Task.Run(TaskWatcher);

            DownloadFinished = true;
            
            Drawer.Stop();
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
                for (int i=0;i<TNumber;i++)
                {
                    if (ThreadList[i].ProgressAbsolute == OldThreadList[i].ProgressAbsolute && ThreadList[i].DownloadStatus!=DownloadStates.Finished)
                    {
                        ThreadList[i].Suspended = true;
                        ThreadList[i].CloseFileStream();
                        ThreadList[i] = new DownloadThread(URL, ThreadList[i].ProgressAbsolute, ThreadList[i].End, $"{Filename}.temp{i}", PathToTempFolder,ProxyDistributor, ThreadList[i].Proxy , ThreadList[i].Accumulated, ThreadList[i].ReconnectCount);
                        ThreadList[i].CanClearLine = true;
                        tasks[i] = ThreadList[i].StartThreadAsync();

                    }
                }
                OldThreadList = ThreadList.Select(x => x.Copy()).ToList();
                Thread.Sleep(TimeOutMs);
            }
        }
    }
}

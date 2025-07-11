﻿using System;
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
using System.Collections.ObjectModel;

namespace MultithreadDownloader 
{

    public class DownloadController : ObservableObject
    {



        private string _filename="testfile";
        private string _url;
        private string Path;
        private string PathToTempFolder;
        private HttpWebRequest request;
        private WebResponse responce;
        private int _tnumber;
        private int _timeoutms=3000;
        private DownloadStatuses _status;
        private long _byteslength; 
        private long _sectionlenth;
        private long LastPiece;
        private long _totalprogress;
        private double _progresspercent;
        private double _downloadspeed;
        private bool Locked=false;
        private bool DownloadFinished = false;
        public bool canClearLine=false;
        private long progressFromPaused;
        private KeyValueConfigurationCollection config;
        private ProxyConfiguration proxyConfiguration;
        public int TestVal=50;



        

        private ObservableCollection<DownloadThread> _threadlist = new ObservableCollection<DownloadThread>();
        private List<DownloadThread> OldThreadList = new List<DownloadThread>();
        private List<Task> tasks = new List<Task>();

        private FileManager FMan;
        private ProxyManager ProxyDistributor;
        private List<string> ProxyList;


        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };


        public string Filename { get { return _filename; } set {_filename=value; OnPropertyChanged(); } }
        public string Size { get { return SizeSuffix(_byteslength); } }
        public double ProgressPercentage { get { return _progresspercent; } set { 
                _progresspercent = value; OnPropertyChanged(); } } 
        public string URL { get { return _url; } }
        public int ThreadNumber { get { return _tnumber; } }
        public int TimeOutMs { get { return _timeoutms; } set { _timeoutms = value; } }
        public DownloadStatuses Status { get { return _status; }  set { _status = value; OnPropertyChanged(); } }
        public long BytesLength { get { return _byteslength; } }
        public long SectionLength { get { return _sectionlenth; } set { _sectionlenth = value; OnPropertyChanged(); } }
        public long TotalProgress {  get { return _totalprogress; } 
            set { _totalprogress = value; OnPropertyChanged(); } }
        public ObservableCollection<DownloadThread> ThreadList { get { return _threadlist; } }






        public DownloadController( string url, int _tnum, FileManager fileman, ProxyConfiguration proxconfig)
        {
   
            
            Status = DownloadStatuses.Idle;
            FMan = fileman;

            //Load config

            config = FMan.LoadConfiguration();


            _url = url;
            _tnumber = _tnum;
            proxyConfiguration = proxconfig;
            ProxyList = fileman.FetchProxyFile(config["ProxyFileName"].Value);
            ProxyDistributor = new ProxyManager(proxyConfiguration, ProxyList);

            Task.Run(SendInitialRequest);




        }

        public DownloadController(DownloadState downloadState, FileManager fileman)
        {
            FMan = fileman;
            config = FMan.LoadConfiguration();
            Path = config["Path"].Value;
            ProxyList = FMan.FetchProxyFile();



            CreateDownloadFromState(downloadState);
            PathToTempFolder = Path + "\\" + _filename + ".temp";
            FMan.SetValues(_filename, Path, _tnumber);


        }

        public async Task SendInitialRequest()
        {
            request = (HttpWebRequest)WebRequest.Create(URL);
            responce = request.GetResponse();
            _byteslength = responce.ContentLength;
            OnPropertyChanged("Size");
            SectionLength = _byteslength / _tnumber;

            TryGetName();

            Path = config["Path"].Value;
            PathToTempFolder = Path + "\\" + _filename + ".temp";
            FMan.SetValues(_filename, Path, _tnumber);
            FMan.CreateDirectory();


        }

        public async Task StartDownloadAsync()
        {
            //Launches main process for controller 

            SplitIntoSections();
            StartAllThreadsAsync();


        }


        public void SplitIntoSections()
        {
            LastPiece = BytesLength % _tnumber;
            for (int i = 0; i < _tnumber; i++)
            {
                ThreadList.Add(new DownloadThread(URL, i * _sectionlenth, ((i + 1) * _sectionlenth) - 1, $"{_filename}.temp{i}", PathToTempFolder, ProxyDistributor, seqnum:i));
            }
            ThreadList[_tnumber - 1].End = ThreadList[_tnumber - 1].End + LastPiece ; //Maybe add "+1" here

        }
        public async Task StartAllThreadsAsync()
        {
            Status = DownloadStatuses.Downloading;
            foreach (DownloadThread thread in ThreadList)
            {
                if (thread.Status!=DownloadStatuses.Finished) tasks.Add(thread.StartThreadAsync());
            }

            await ServicesLauncher();
        }

        public async Task ServicesLauncher()
        {
            Task.Run(UpdateProgress);


            Task.Run(DownloadWatcher);
            await Task.Run(TaskWatcher);


         
            if (Status != DownloadStatuses.Paused && Status != DownloadStatuses.Cancelled) //Check if download is paused so that the fman doesn't delete the temp files
            {
                CaptureDownloadState();
                CloseAllThreads();
                FMan.CombineTempFiles();
                FMan.RemoveTempDirectory();
                Status = DownloadStatuses.Finished;
            }
            canClearLine=true;

        }

        public async Task TaskWatcher()
        {
            bool flag=false;
            while (Status==DownloadStatuses.Downloading)
            {

                foreach (DownloadThread download in ThreadList)
                {
                    if (download.Status==DownloadStatuses.Finished.Status) flag = true;
                    else
                    {
                        flag=false;
                        break;
                    }
                }

                if (flag) Status=DownloadStatuses.Finished; // if looped through all threads and their status is "Finished", set the status of the entire download to "Finished"
                
                Thread.Sleep(100);
            }


        }

        public async Task DownloadWatcher()
        {
            DownloadThread _newThread; 
            OldThreadList = ThreadList.Select(x=>x.Copy()).ToList();
            Thread.Sleep(TimeOutMs);
            while (Status==DownloadStatuses.Downloading)
            {
                for (int i=0;i<_tnumber;i++)
                {
                    if (ThreadList[i].ProgressAbsolute == OldThreadList[i].ProgressAbsolute && ThreadList[i].Status!=DownloadStatuses.Finished && _status!=DownloadStatuses.Paused)
                    {
                        if (ThreadList[i].Suspended != true)//check if the thread was already closed by pause button
                        {
                            ThreadList[i].Suspended = true;
                            ThreadList[i].CloseAllStreams();
                        }
                        _newThread = new DownloadThread(
                            URL,
                            ThreadList[i].ProgressAbsolute,
                            ThreadList[i].End, $"{_filename}.temp{i}",
                            PathToTempFolder, ProxyDistributor,
                            ThreadList[i].Proxy,
                            ThreadList[i].Accumulated,
                            ThreadList[i].ReconnectCount);
                        ThreadList[i] = _newThread;
                        ThreadList[i].canClearLine = true;
                        tasks[i] = ThreadList[i].StartThreadAsync();

                    }
                }
                OldThreadList = ThreadList.Select(x => x.Copy()).ToList();
                Thread.Sleep(TimeOutMs);
            }
        }

        private void RemoveAllThreads()
        {
            _threadlist = new ObservableCollection<DownloadThread>();
        }

        public void Pause()
        {
            Status = DownloadStatuses.Paused;
            CloseAllThreads();
            CaptureDownloadState();
        }

        public void Continue()
        {
            if (Status != DownloadStatuses.Finished)
            {
                Status = DownloadStatuses.Downloading;
                if (ThreadList.Count==0)SplitIntoSections(); //if threads are not already created, create them
                canClearLine = true;
                StartAllThreadsAsync();
            }
            

        }

        public void Cancel()
        {
            if (Status == DownloadStatuses.Finished)
            {
                FMan.RemoveDownloadedFile();
                CloseAllThreads();
            }
            else
            {
                Status = DownloadStatuses.Cancelled;
                CloseAllThreads();
                FMan.RemoveTempDirectory();
            }
            canClearLine = true;
            RemoveAllThreads();

        }

        private void CaptureDownloadState()
        {
            DownloadState state = new DownloadState
            {
                URL = _url,
                Filname = _filename,
                TotalSize = BytesLength,
                PathToTempFolder = PathToTempFolder,
                ThreadNumber = _tnumber,
                ChucnkSize = _sectionlenth,
                TotalProgress = _totalprogress,
                ProxyConfiguration = proxyConfiguration,
                DownloadStatus = this.Status,
                ThreadStates = ThreadList.Select(t => new ThreadState
                {
                    Start = t.Start,
                    End = t.End,
                    ProgressAbsolute = t.ProgressAbsolute,
                    Accumulated = t.Accumulated,
                    Filename = t.Filename

                }).ToList()
            };

            FMan.SaveDownloadStateToFile(state);
        }

        private async Task UpdateProgress()
        {
            while (Status == DownloadStatuses.Downloading)
            {
                Thread.Sleep(10);
                TotalProgress = 0;
                TotalProgress+=progressFromPaused;
                foreach (DownloadThread download in ThreadList)
                {
                    TotalProgress += download.Accumulated;
                }
                ProgressPercentage = (((double)TotalProgress / (double)BytesLength) * 100);
            }
           
        }

        private void CloseAllThreads()
        {
            try
            {
                for (int i = 0; i < _tnumber; i++)
                {

                    ThreadList[i].Suspended = true;
                    ThreadList[i].CloseAllStreams();

                }
            }
            catch (Exception ex)
            {
                // Log the exception so you can see what's happening
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Thread terminated with exception: {ex.Message}");
            }
        }
        private void TryGetName()
        {
            if (responce.Headers["Content-Disposition"] == null)
            {
                Filename = URL.Split("/").Last();
            }
            else _filename = responce.Headers["Content-Disposition"].Replace("attachment; filename=", String.Empty).Replace("\"", String.Empty);
        }

        public void CreateDownloadFromState(DownloadState state)
        {
            try
            {
                DownloadThread _newThread;
                _url = state.URL;
                Filename = state.Filname;
                PathToTempFolder = state.PathToTempFolder;
                _tnumber = state.ThreadNumber;
                _byteslength = state.TotalSize;
                _sectionlenth = state.ChucnkSize;
                proxyConfiguration = state.ProxyConfiguration;
                Status=state.DownloadStatus;
                ProxyDistributor = new ProxyManager(proxyConfiguration, ProxyList);
                _threadlist = new ObservableCollection<DownloadThread>();
                tasks = new List<Task>();
                progressFromPaused = state.TotalProgress;

                for (int i = 0; i < _tnumber; i++)
                {
                    ThreadState threadState = state.ThreadStates[i];
                    _newThread = new DownloadThread(
                        _url,
                        threadState.ProgressAbsolute,
                        threadState.End, threadState.Filename,
                        PathToTempFolder, ProxyDistributor);
                    if ((threadState.End - threadState.Start) == threadState.Accumulated - 1) _newThread.Status = DownloadStatuses.Finished;
                    ThreadList.Add(_newThread);//Adds a newly created thread to a thread list
                                               //progressFromPaused+=threadState.Accumulated;//we add accumulated length from previous processes for better progress displaying in the UpdateProgress()

                }

                //TotalProgress += progressFromPaused;
                ProgressPercentage = (((double)TotalProgress / (double)_byteslength) * 100);// this line is for displaying in the downloadsmanager before the download is continued

            }

            catch (Exception ex)
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Thread terminated with exception: {ex.Message}");
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

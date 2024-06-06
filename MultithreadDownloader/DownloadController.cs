using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Globalization;
using System.Threading;

namespace MultithreadDownloader
{

    internal class DownloadController
    {
        
        private string Filename;
        private string URL;
        HttpWebRequest request;
        WebResponse responce;
        private int TNumber;
        public int TimeOutMs=3000;
        private long BytesLength;
        private long SectionLength;
        private long LastPiece;
        private bool CanLaunchConsoleUpdate=true;
        private string DownloadBaseInfo;
        private bool DownloadFinished = false;

        private List<DownloadThread> ThreadList = new List<DownloadThread>();
        private List<DownloadThread> OldThreadList = new List<DownloadThread>();
        List<Task> tasks = new List<Task>();


        private bool UseProxy;
        private string ProxyAddress;
        private int ProxyPort;

        public DownloadController(string _filename, string _url, int _tnum, string path = "",bool _useproxy=false, string _proxyaddress=null)
        {
            Filename = _filename;
            URL = _url;
            TNumber = _tnum;
            UseProxy=_useproxy;
            if (_proxyaddress!=null)
            {
                ProxyAddress = _proxyaddress.Split(":")[0];
                ProxyPort = int.Parse(_proxyaddress.Split(":")[1]);
            }
            
            

            request = (HttpWebRequest)WebRequest.Create(URL);
            responce = request.GetResponse();
            BytesLength = responce.ContentLength;
            DownloadBaseInfo = $"{URL} \n" + $"Length: {BytesLength} bytes ~= {BytesLength / 1024 / 1024} Mb \n" +
                $"Number of threads: {TNumber}";
        }

        public void PrintData()
        {
            SplitIntoSections();
            
            Start();


        }
        public void SplitIntoSections()
        {
            SectionLength = BytesLength / TNumber;
            LastPiece = BytesLength % TNumber;
            for (int i = 0; i < TNumber; i++)
            {
                ThreadList.Add(new DownloadThread(URL, i * SectionLength, ((i + 1) * SectionLength) - 1, $"{Filename}_temp_{i}"));
                ThreadList[i].Size = ThreadList[i].End-ThreadList[i].Start;
            }
            ThreadList[TNumber - 1].End = ThreadList[TNumber - 1].End + LastPiece + 1;

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


        
        public void CombineTempFiles()
        {

            using (FileStream OutFile = new FileStream($"{Filename}", FileMode.Append, FileAccess.Write))
            {
                for (int i = 0; i < TNumber; i++)
                {
                    int bytesread = 0;
                    byte[] buffer = new byte[1024];
                    using (FileStream InputFile = new FileStream($"{Filename}_temp_{i}", FileMode.Open, FileAccess.Read))
                    {
                        do
                        {
                            bytesread = InputFile.Read(buffer, 0, buffer.Length);
                            OutFile.Write(buffer, 0, bytesread);
                        }
                        while (bytesread > 0);
                    }

                }
            }
        }

        public void DeleteTempFiles()
        {
            string[] TempFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*temp*");
            foreach (string TempFile in TempFiles)
            {
                File.Delete(TempFile);
            }
        }
        public async Task ConsoleUpdater()
        {
            while (!DownloadFinished) 
            {
                UpdateDownloadDetails();
                Thread.Sleep(10);
            }
        }
        public void UpdateDownloadDetails()
        {
           
            if (CanLaunchConsoleUpdate)
            {
                CanLaunchConsoleUpdate = false;
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(DownloadBaseInfo);
                foreach (DownloadThread download in ThreadList)
                {
                    
                    if (download.CanClearLine)//If finished downloading clear the entire line
                    {
                        ClearLine();
                        download.CanClearLine = false;
                    }
                    
                    Console.WriteLine($"{download.ThreadName}: {download.ProgressAbsolute-download.Start} bytes {download.ProgressRelative.ToString("N2")}% {download.Status}");
                }

                CanLaunchConsoleUpdate = true;
            }
            
            
        }

        public void ClearLine()
        {
            int CurrentLineCursor = Console.CursorTop;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, CurrentLineCursor);
        }

        public async Task ServicesLauncher()
        {
            Task.Run(ConsoleUpdater);
            Task.Run(ThreadWatcher);

            await Task.WhenAll(tasks);
            DownloadFinished = true;
            CombineTempFiles();
            DeleteTempFiles();
        }

        public async Task ThreadWatcher()
        {
            OldThreadList = ThreadList.Select(x=>x.Copy()).ToList();
            Thread.Sleep(TimeOutMs);
            while (!DownloadFinished)
            {
                for (int i=0;i<TNumber;i++)
                {
                    if (ThreadList[i].ProgressAbsolute == OldThreadList[i].ProgressAbsolute && ThreadList[i].Status=="Downloading")
                    {
                        ThreadList[i].InitiateReconnectSequence();
                       
                        tasks[i] = ThreadList[i].StartThreadAsync();
                    }
                }
                OldThreadList = ThreadList.Select(x => x.Copy()).ToList();
                Thread.Sleep(TimeOutMs);
            }
        }
    }
}

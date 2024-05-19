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
        private long BytesLength;
        private long SectionLength;
        private long LastPiece;
        private bool CanLaunchConsoleUpdate=true;
        private string DownloadBaseInfo;
        private List<DownloadThread> ThreadList = new List<DownloadThread>();
        private bool DownloadFinished=false;
        public DownloadController(string _filename, string _url, int _tnum, string path = "")
        {
            Filename = _filename;
            URL = _url;
            TNumber = _tnum;

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
                ThreadList.Add(new DownloadThread(i * SectionLength, ((i + 1) * SectionLength) - 1, $"{Filename}_temp_{i}"));
            }
            ThreadList[TNumber - 1].End = ThreadList[TNumber - 1].End + LastPiece + 1;

        }
        public async Task Start()
        {
            
            List<Task> tasks = new List<Task>();
            
            foreach (DownloadThread thread in ThreadList)
            {
                tasks.Add(StartThreadAsync(thread));
            }
            Task.Run(ConsoleUpdater);
            await Task.WhenAll(tasks);
            DownloadFinished=true;
            CombineTempFiles();
            DeleteTempFiles();
            Console.WriteLine("Done");
        }



        public async Task StartThreadAsync(DownloadThread thread)
        {
            HttpWebRequest ThreadRequest = (HttpWebRequest)WebRequest.Create(URL);
            ThreadRequest.AddRange(thread.Start, thread.End);
            WebResponse ThreadResponce = await ThreadRequest.GetResponseAsync();
            thread.Status = "Downloading";
            thread.ProgressAbsolute = thread.Start;
            using (Stream ThreadRespStream = ThreadResponce.GetResponseStream())
            {
                using (FileStream fs = new FileStream(thread.ThreadName, FileMode.Create))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    do
                    {
                        bytesRead = ThreadRespStream.Read(buffer, 0, buffer.Length);
                        fs.Write(buffer, 0, bytesRead);
                        fs.Flush();
                        thread.ProgressAbsolute += bytesRead;
                       
                        thread.ProgressRelative = CalcProgress(thread);
                        
                    }
                    while (bytesRead > 0);
                    fs.Close();
                }
            }
            thread.Status = "Done";
            
            
        }

        private float CalcProgress(DownloadThread thread)
        {
            long normprog = thread.ProgressAbsolute - thread.Start;
            long normgoal = thread.End - thread.Start;
            double relprog = (double)normprog / (double)normgoal;
            float res = (float)(Math.Round(relprog*100,2));
            
            return res;
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
                Thread.Sleep(1);
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
                    
                    if (download.Status == "Done" && download.ConsoleFlag)//If finished downloading clear the entire line
                    {
                        int CurrentLineCursor = Console.CursorTop;
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, CurrentLineCursor);
                    }
                    
                    Console.WriteLine($"{download.ThreadName}: {download.ProgressRelative.ToString("N2")} {download.Status}");
                }

                CanLaunchConsoleUpdate = true;
            }
            
            
        }

    }
}

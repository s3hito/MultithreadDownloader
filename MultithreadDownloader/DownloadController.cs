using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Globalization;

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
        private List<DownloadThread> ThreadList = new List<DownloadThread>();
        public DownloadController(string _filename, string _url, int _tnum, string path = "")
        {
            Filename = _filename;
            URL = _url;
            TNumber = _tnum;

            request = (HttpWebRequest)WebRequest.Create(URL);
            responce = request.GetResponse();
            BytesLength = responce.ContentLength;

        }

        public void PrintData()
        {

            SplitIntoSections();
            Console.WriteLine(URL);
            Console.WriteLine($"Length: {BytesLength} bytes ~= {BytesLength / 1024 / 1024} Mb");
            Console.WriteLine($"Number of threads: {TNumber}");
            Console.WriteLine("Downloading");

            Start();
            Console.WriteLine("Downloaded");


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
            await Task.WhenAll(tasks);
            Console.WriteLine("Continuing");
        }

        public async Task StartThreadAsync(DownloadThread thread)
        {
            HttpWebRequest ThreadRequest = (HttpWebRequest)WebRequest.Create(URL);
            ThreadRequest.AddRange(thread.Start, thread.End);
            WebResponse ThreadResponce = (WebResponse)ThreadRequest.GetResponse();

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
                    }
                    while (bytesRead > 0);
                    fs.Close();
                }
            }
            Console.WriteLine("Thread completed");
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

    }
}

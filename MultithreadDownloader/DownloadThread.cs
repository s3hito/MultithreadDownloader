using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloader
{
    public class DownloadThread
    {
        public long Start;
        public long End;
        public string ThreadName;
        public string Status;
        public long ProgressAbsolute;
        public float ProgressRelative;
        public bool ConsoleFlag;
        public long Size;
        public string ProxyAddress;
        public int ProxyPort;
        public bool UseProxy;
        public string URL;

        public DownloadThread(string url, long start, long end, string threadname,bool useproxy=false, string proxyAdress=null, int proxyPort=0)
        {
            URL = url;
            Start = start;
            End = end;
            ThreadName = threadname;
            Status = "Idle";
            ConsoleFlag = true;
            UseProxy = useproxy;
            ProxyAddress = proxyAdress;
            ProxyPort = proxyPort;

            
            

        }

        public async Task StartThreadAsync()
        {
            HttpWebRequest ThreadRequest = (HttpWebRequest)WebRequest.Create(URL);

            
            if (UseProxy)
            {
                ThreadRequest.Proxy = new WebProxy(ProxyAddress, ProxyPort);
            }

            ThreadRequest.AddRange(Start, End);
            Status = "Connecting";
            WebResponse ThreadResponce = await ThreadRequest.GetResponseAsync();
            Status = "Downloading";
            ProgressAbsolute = Start;
            using (Stream ThreadRespStream = ThreadResponce.GetResponseStream())
            {
                using (FileStream fs = new FileStream(ThreadName, FileMode.OpenOrCreate))
                {
                   
                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    do
                    {
                   
                        bytesRead =await ThreadRespStream.ReadAsync(buffer, 0, buffer.Length,cts.Token);
                        fs.Write(buffer, 0, bytesRead);
                        fs.Flush();
                        ProgressAbsolute += bytesRead;
                        
                        ProgressRelative = CalcProgress();

                    }
                    while (bytesRead > 0);
                    fs.Close();
                }
            }
            Status = "Done";
        }
        private float CalcProgress()
        {
            long normprog = ProgressAbsolute - Start;
            long normgoal = End - Start;
            double relprog = (double)normprog / (double)normgoal;
            float res = (float)(Math.Round(relprog * 100, 2));

            return res;
        }

        public void InitiateReconnectSequence()
        {
            Thread.Sleep(100000);

        }
    }
}

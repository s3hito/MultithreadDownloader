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
        public bool CanClearLine;
        public long Size;
        public string ProxyAddress;
        public int ProxyPort;
        public bool UseProxy;
        public string URL;
        FileStream fs;
        Stream ThreadRespStream;

        public DownloadThread(string url, long start, long end, string threadname,bool useproxy=false, string proxyAdress=null, int proxyPort=0)
        {
            URL = url;
            Start = start;
            End = end;
            ThreadName = threadname;
            Status = "Idle";
            CanClearLine = false;
            UseProxy = useproxy;
            ProxyAddress = proxyAdress;
            ProxyPort = proxyPort;
        }

        public DownloadThread Copy() 
        {
            return (DownloadThread)this.MemberwiseClone();
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
            Thread.Sleep(3000);
            WebResponse ThreadResponse = null;
            Task<WebResponse> getResponceTask = ThreadRequest.GetResponseAsync(); //Need to add timeout for this guy as well. When reconnnecting it may not always get responce
            if (getResponceTask == await Task.WhenAny(getResponceTask, Task.Delay(3000)))//Хуита не робит
            {
                ThreadResponse = await getResponceTask;
                Status = "Downloading";
                ProgressAbsolute = Start;
            }
            else
            {
                Status = "Timed out. Reconnecting...";
                
            }

           

            ThreadRespStream = ThreadResponse.GetResponseStream();
            fs = new FileStream(ThreadName, FileMode.OpenOrCreate);

            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            do
            {
                   
                bytesRead =await ThreadRespStream.ReadAsync(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, bytesRead);
                fs.Flush();
                ProgressAbsolute += bytesRead;
                        
                ProgressRelative = CalcProgress();
                    

            }
            while (bytesRead > 0);
            fs.Close();
                
            Status = "Done";
            CanClearLine = true;
            
           
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
            Status = "Reconnecting...";
            ThreadRespStream.Close();

            CanClearLine = true;
            fs.Flush();
            fs.Close();
            Start = ProgressAbsolute;

        }
    }
}

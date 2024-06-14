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
        WebResponse ThreadResponse;
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

            using (FileStream fs = new FileStream(ThreadName, FileMode.Create)) { }
        }

        public DownloadThread Copy() 
        {
            return (DownloadThread)this.MemberwiseClone();
        }



        public async Task StartThreadAsync()
        {
            HttpWebRequest ThreadRequest = (HttpWebRequest)WebRequest.Create(URL);
            fs = null;
            fs = new FileStream(ThreadName, FileMode.Append);


            if (UseProxy)
            {
                ThreadRequest.Proxy = new WebProxy(ProxyAddress, ProxyPort);
            }

            ThreadRequest.AddRange(Start, End);
            Status = "Connecting";
            ThreadResponse = await ThreadRequest.GetResponseAsync(); //Need to add timeout for this guy as well. When reconnnecting it may not always get responce
            if (fs == null)
            {
                CloseAllStreams();
                return;
            } 
            Status = "Downloading";
            ProgressAbsolute = Start;
          

            ThreadRespStream = ThreadResponse.GetResponseStream();
            if (fs == null)
            {
                CloseAllStreams();
                return;
            }
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            do
            {

                bytesRead = await ThreadRespStream.ReadAsync(buffer, 0, buffer.Length);
                if (fs == null)
                {
                    CloseAllStreams();
                    return;
                }
                fs.Write(buffer, 0, bytesRead);
                fs.Flush();
                ProgressAbsolute += bytesRead;

                ProgressRelative = CalcProgress();


            }
            while (bytesRead > 0 && ProgressAbsolute < End); //Sometimes for large files it'll keep sending data over the end 
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

        public void CloseAllStreams()
        {
            if (ThreadRespStream != null) ThreadRespStream.Close();

            if (ThreadResponse != null) ThreadResponse.Close();

            if (fs != null)
            {
                fs.Flush();
                fs.Close();
            }
        }

        public void InitiateReconnectSequence()
        {

            Status = "Disconnected";
           
            CloseAllStreams();


            CanClearLine = true;
            
            
            Start = ProgressAbsolute;
             
            Status = "Disconnected123";


        }
    }
}

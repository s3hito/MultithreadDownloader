using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public string Path;
        public string PathToFile;
        public string Filename;
        public string ThreadName;
        public long ProgressAbsolute;
        public float ProgressRelative;
        public bool CanClearLine;
        public long Size;
        public string Proxy;
        private ProxyManager ProxyDistRef;
        public string ProxyIpAddress;
        public int ProxyPort;
        public int ReconnectCount;
        public int MaxReconnect;
        private bool ChangeProxyOnMaxReconnect;
        public string URL;
        public int InstanceNumber;
        public bool Suspended = false;
        public long Accumulated;
        FileStream fs;
        WebResponse ThreadResponse;
        Stream ThreadRespStream;
        HttpWebRequest ThreadRequest;
        public DownloadStates DownloadStatus;
        public enum DownloadStates
        {
            [Description("Idle")]
            Idle=0,
            [Description("Connecting")]
            Connecting = 1,
            [Description("Downloading")]
            Downloading = 2,
            [Description("Finished")]
            Finished = 3,
            [Description("Disconnected")]
            Disconnected = 4,
            [Description("Reconnecting")]
            Reconnecting = 5

        }
        public DownloadThread(string url, long start, long end, string filename,string path, ProxyManager proxman,string prox="", long acum=0, int reccount=-1, int maxrec=3)
        {

            URL = url;
            Start = start;
            End = end;
            Filename = filename;
            DownloadStatus = DownloadStates.Idle;
            ProxyDistRef= proxman;
            CanClearLine = false;
            Accumulated = acum;
            ReconnectCount= reccount;
            MaxReconnect = maxrec;
            ThreadRequest = (HttpWebRequest)WebRequest.Create(URL);
            ProxyDistRef = proxman;
            Proxy = prox;
            lock (ProxyDistRef)
            {
                Proxy = ProxyDistRef.GetProxy(this);

            }
            if (Proxy != "")
            {
                ProxyIpAddress = Proxy.Split(":")[0];
                ProxyPort = Convert.ToInt32(Proxy.Split(":")[1]);
                ThreadRequest.Proxy = new WebProxy(ProxyIpAddress, ProxyPort);
            }


            Path = path;
            PathToFile = Path + "\\" + Filename;
            ThreadName = Filename.Split(".").Last();
            using (FileStream fs = new FileStream(PathToFile, FileMode.OpenOrCreate)) { } //Gotta add a feature so that if it's a new download
                                                                                          //it creates a file (so that if there's a file from previous download)
                                                                                          //and if it's a new thread for reconnection it opens a file
        }

        public DownloadThread Copy() 
        {
            return (DownloadThread)this.MemberwiseClone();
        }



        public async Task StartThreadAsync()
        {
            Size = End - Start;

            fs = null;
            fs = new FileStream(PathToFile, FileMode.Append);
            ProgressAbsolute = Start;

            //ThreadRequest.AddRange(Start, ControllerRef.BytesLength);
            ThreadRequest.AddRange(Start, End);
            DownloadStatus = DownloadStates.Connecting;
            ThreadResponse = await ThreadRequest.GetResponseAsync();
            if (Suspended)
            {
                CloseAllStreams();
                return;
            }
            DownloadStatus=DownloadStates.Downloading;
          

            ThreadRespStream = ThreadResponse.GetResponseStream();

            if (Suspended)
            {
                CloseAllStreams();
                return;
            }

            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            do
            {

                bytesRead = await ThreadRespStream.ReadAsync(buffer, 0, buffer.Length);//throws an exception if timed out and ThreadRespStream is null
                if (Suspended)
                {
                    CloseAllStreams();
                    return;
                }
                
                if (ProgressAbsolute + bytesRead - 1 > End)//Check for overdownload
                {
                    bytesRead = Convert.ToInt32(End - ProgressAbsolute + 1);
                }
                
                ProgressAbsolute += bytesRead;

                fs.Write(buffer, 0, bytesRead);


                fs.Flush();
                Accumulated += bytesRead;
                ProgressRelative = CalcProgress();

                
            }
            while (bytesRead > 0 && ProgressAbsolute <= End); //Sometimes for large files it'll keep sending data over the end.
                                   //Add this later && ProgressAbsolute < End

            fs.Close();
            DownloadStatus=DownloadStates.Finished;
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
        
        public void CloseAllStreams()//never executes with fs.CanWrite
        {
            if (ThreadRespStream != null) ThreadRespStream.Close();

            if (ThreadResponse != null) ThreadResponse.Close();

        }

        
        public void CloseFileStream()
        {
            DownloadStatus= DownloadStates.Disconnected ;
            fs.Flush();
            fs.Close();
            fs = null;
            ReconnectCount++;
            
        }
    }
}

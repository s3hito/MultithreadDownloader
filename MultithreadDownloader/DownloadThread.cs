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
    public class DownloadThread : ObservableObject
    {
        public long Start;
        public long End;
        private int _seqnumber;
        public string Path;
        public string PathToFile;
        public string Filename;
        public string ThreadName;
        public long ProgressAbsolute;
        public float ProgressRelative;
        public bool canClearLine;
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
        public long _accumulated;
        FileStream fs;
        WebResponse ThreadResponse;
        Stream ThreadRespStream;
        HttpWebRequest ThreadRequest;
        private DownloadStatuses _status;

        public int SequenceNumber { get { return _seqnumber; }  set { _seqnumber = value; OnPropertyChanged(); } }
        public long Accumulated { get { return _accumulated; } set { _accumulated = value; OnPropertyChanged(); } }
        public DownloadStatuses Status { get { return _status; } set { _status = value; OnPropertyChanged(); } }

       
        public DownloadThread(string url, long start, long end, string filename,string path, ProxyManager proxman,string prox="", long acum=0, int reccount=-1, int maxrec=3, int seqnum=0)
        {

            URL = url;
            Start = start;
            End = end;
            Filename = filename;
            Status = DownloadStatuses.Idle;
            ProxyDistRef= proxman;
            canClearLine = false;
            Accumulated = acum;
            ReconnectCount= reccount;
            MaxReconnect = maxrec;
            _seqnumber = seqnum;
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
            if ((start-end-1)!=0) using (FileStream fs = new FileStream(PathToFile, FileMode.OpenOrCreate)) { } //if download has not completed, open or create the file.
                                                                                                                //if the download has completed, do nothing
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

            ThreadRequest.AddRange(Start, End);
            Status = DownloadStatuses.Connecting;
            ThreadResponse = await ThreadRequest.GetResponseAsync();
            if (Suspended) return;

            Status = DownloadStatuses.Downloading;
          

            ThreadRespStream = ThreadResponse.GetResponseStream();

            if (Suspended) return;


            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            do
            {

                bytesRead = await ThreadRespStream.ReadAsync(buffer, 0, buffer.Length);
                if (Suspended) return;

                
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
            fs = null;
            Status=DownloadStatuses.Finished;
            canClearLine = true;
            
           
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
            try
            {
                if (ThreadRespStream != null) { ThreadRespStream.Close(); ThreadRespStream = null; }

                if (ThreadResponse != null) { ThreadResponse.Close(); ThreadResponse = null; }
                if (fs != null)
                {
                    Status = DownloadStatuses.Disconnected;
                    fs.Flush();
                    fs.Close();
                    fs = null;
                    ReconnectCount++;
                }
             

            }
            
            catch (Exception ex)
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"thread terminated with exception: {ex.Message}");
            }
        }

    }
}

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
        public string Path;
        public string PathToFile;
        public string Filename;
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
        public int InstanceNumber;
        public bool Suspended = false;
        FileStream fs;
        WebResponse ThreadResponse;
        Stream ThreadRespStream;

        public DownloadThread(string url, long start, long end, string filename,string path, bool useproxy=false, string proxyAdress=null, int proxyPort=0)
        {
            URL = url;
            Start = start;
            End = end;
            Filename = filename;
            Status = "Idle";
            CanClearLine = false;
            UseProxy = useproxy;
            ProxyAddress = proxyAdress;
            ProxyPort = proxyPort;
            Path= path;
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

            HttpWebRequest ThreadRequest = (HttpWebRequest)WebRequest.Create(URL);
            fs = null;
            fs = new FileStream(PathToFile, FileMode.Append);
            ProgressAbsolute = Start;


            if (UseProxy)
            {
                ThreadRequest.Proxy = new WebProxy(ProxyAddress, ProxyPort);
            }

            //ThreadRequest.AddRange(Start, ControllerRef.BytesLength);
            ThreadRequest.AddRange(Start, End);
            Status = "Connecting";
            ThreadResponse = await ThreadRequest.GetResponseAsync();
            if (Suspended)
            {
                CloseAllStreams();
                return;
            }

            Status = "Downloading";
          

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
                ProgressRelative = CalcProgress();

                
            }
            while (bytesRead > 0 && ProgressAbsolute <= End); //Sometimes for large files it'll keep sending data over the end.
                                   //Add this later && ProgressAbsolute < End

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
        
        public void CloseAllStreams()//never executes with fs.CanWrite
        {
            if (ThreadRespStream != null) ThreadRespStream.Close();

            if (ThreadResponse != null) ThreadResponse.Close();

        }

        
        public void CloseFileStream()
        {
               
                Status = "Disconnected";
                fs.Flush();
                fs.Close();
                fs = null;
                CanClearLine = true;
            
        }
    }
}

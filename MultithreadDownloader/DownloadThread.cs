using System;
using System.Collections.Generic;
using System.Linq;
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
        public DownloadThread(long start, long end, string threadname)
        {
            Start = start;
            End = end;
            ThreadName = threadname;
            Status = "Idle";
            ConsoleFlag = true;
        }
    }
}

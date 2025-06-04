using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloader
{
    [Serializable]
    public class DownloadState
    {
        public string URL { get; set; }
        public string Filname { get; set; }
        public int ThreadNumber { get; set; }
        public List<ThreadState> ThreadStates { get; set; }
        public string PathToTempFolder;
        public long TotalSize { get; set; }
        public long ChucnkSize {  get; set; }
        public long TotalProgress { get; set; }
        public DownloadStatuses DownloadStatus { get; set; }
        public ProxyConfiguration ProxyConfiguration { get; set; }
    }

}

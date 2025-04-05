using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloader
{
    public class DownloadStatuses
    {
        private DownloadStatuses(string status) { Status = status; }
        public string Status { get; private set; }


        public static DownloadStatuses Idle { get { return new DownloadStatuses("Idle"); } }
        public static DownloadStatuses Connecting { get { return new DownloadStatuses("Connecting"); } }
        public static DownloadStatuses Downloading { get { return new DownloadStatuses("Downloading"); } }
        public static DownloadStatuses Finished { get { return new DownloadStatuses("Finished"); } }
        public static DownloadStatuses Disconnected { get { return new DownloadStatuses("Disconnected"); } }
        public static DownloadStatuses Reconnecting { get { return new DownloadStatuses("Reconnecting"); } }

        public override string ToString()
        {
            return Status;
        }
        public static implicit operator string(DownloadStatuses status) { return status.Status; }
    }
}

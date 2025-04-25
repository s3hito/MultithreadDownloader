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


        private static readonly DownloadStatuses _idle = new DownloadStatuses("Idle");
        private static readonly DownloadStatuses _connecting = new DownloadStatuses("Connecting");
        private static readonly DownloadStatuses _downloading = new DownloadStatuses("Downloading");
        private static readonly DownloadStatuses _finished = new DownloadStatuses("Finished");
        private static readonly DownloadStatuses _disconnected = new DownloadStatuses("Disconnected");
        private static readonly DownloadStatuses _reconnecting = new DownloadStatuses("Reconnecting");
        private static readonly DownloadStatuses _paused = new DownloadStatuses("Paused");
        private static readonly DownloadStatuses _cancelled = new DownloadStatuses("Cancelled");

        public static DownloadStatuses Idle => _idle;
        public static DownloadStatuses Connecting => _connecting;
        public static DownloadStatuses Downloading => _downloading;
        public static DownloadStatuses Finished => _finished;
        public static DownloadStatuses Disconnected => _disconnected;
        public static DownloadStatuses Reconnecting => _reconnecting;
        public static DownloadStatuses Paused => _paused;
        public static DownloadStatuses Cancelled => _cancelled;


        public override string ToString() => Status;

        public static implicit operator string(DownloadStatuses status) => status.Status;
    }
}

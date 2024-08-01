using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloader
{
    internal class ConsoleDrawer
    {
        private int DelayMs;
        private bool Switch;
        private string PrefixString;
        private long TotalProgress;
        private long TotalSize;
        private double ProgressPercent;
        private string ProgressBar;
        private long PGChunkSize;
        private DownloadController ControllerRef;
        private List<DownloadThread> DownloadList;  

        
        public ConsoleDrawer(string prefixstring, DownloadController contref, int delayms=10) 
        {
            PrefixString = prefixstring; 

            ControllerRef = contref;
            DelayMs = delayms;
            TotalSize = contref.BytesLength;
        }

        public async Task Start()
        {
            Switch = true;
            Print();
        }
        public void Stop() 
        { 
            Thread.Sleep(100);
            Switch= false;
        }
        private void Print() 
        {
            while (Switch)
            {
                Update();
                Thread.Sleep(DelayMs);
            }

        }
        private void Update()
        {
            Console.SetCursorPosition(0,0);
            Console.WriteLine(PrefixString);
            ProgressPercent= ((double)TotalProgress / (double)TotalSize) *100;
            MakeProgressBar(20);
            Console.WriteLine($"Progress:{ProgressBar} {ProgressPercent.ToString("N2")}%");
            TotalProgress = 0;
            DownloadList = ControllerRef.ThreadList.Select(x => x.Copy()).ToList();
            int i = 0;
            foreach (DownloadThread download in DownloadList)
            {
                if (download.CanClearLine)
                {
                    ControllerRef.ThreadList[i].CanClearLine = false;

                    ClearLine();
                }
                Console.WriteLine($"{download.ThreadName}: {download.ProgressAbsolute - download.Start}/{download.Size} bytes {download.DownloadStatus.GetDescription()} Proxy: {download.Proxy} Reconnections: {download.ReconnectCount}"); //Start:{{download.Start}} End:{{download.End
                TotalProgress += download.Accumulated;
              
                i++;
            }
            
        }
        private void MakeProgressBar(int length)
        {
            PGChunkSize = TotalSize / length;

            ProgressBar = "[";

            for (int i = 0; i < TotalProgress / PGChunkSize; i++)
            {
                ProgressBar += "=";
            }
            if (TotalProgress % PGChunkSize > PGChunkSize / 2)
            {
                ProgressBar += "-";
            }
            else if (TotalProgress / TotalSize < 1)
            {
                ProgressBar += " ";
            }
           

            for (int i = 0; i < length - 1 - (TotalProgress / PGChunkSize); i++)
            {
                ProgressBar += " ";
            }

                ProgressBar += "]";
        }

        private void ClearLine()
        {
            int CurrentLineCursor = Console.CursorTop;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, CurrentLineCursor);
        }
    }
}

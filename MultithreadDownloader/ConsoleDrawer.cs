using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MultithreadDownloader
{
    internal class ConsoleDrawer
    {
        private int DelayMs;
        private bool Switch;
        private string PrefixString;
        private long TotalSize;
        private string ProgressBar;
        private long PGChunkSize;
        private DownloadController ControllerRef;
        private List<DownloadThread> DownloadList;  

        
        public ConsoleDrawer(string prefixstring, DownloadController contref, int delayms=10) 
        {
           

            ControllerRef = contref;
            DelayMs = delayms;
            TotalSize = contref.BytesLength;

            PrefixString = $"{contref.URL} \n" +
               $"Size: {contref.Size}\n" +
               $"Chunk size: {contref.SectionLength} \n" +
               $"Number of threads: {contref.ThreadNumber}";
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
            Console.WriteLine("Done");


        }
        private void Update()
        {
            Console.SetCursorPosition(0,0);
            Console.WriteLine(PrefixString);
            MakeProgressBar(20);
            Console.WriteLine($"Progress:{ProgressBar} {ControllerRef.ProgressPercentage.ToString("N2")}%");
            
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
              
                i++;
            }
            
        }
        private void MakeProgressBar(int length)
        {
            PGChunkSize = TotalSize / length;

            ProgressBar = "[";

            for (int i = 0; i < ControllerRef.TotalProgress / PGChunkSize; i++)
            {
                ProgressBar += "=";
            }
            if (ControllerRef.TotalProgress % PGChunkSize > PGChunkSize / 2)
            {
                ProgressBar += "-";
            }
            else if (ControllerRef.TotalProgress / TotalSize < 1)
            {
                ProgressBar += " ";
            }
           

            for (int i = 0; i < length - 1 - (ControllerRef.TotalProgress / PGChunkSize); i++)
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

using System;
using System.Collections.Generic;
using System.Linq;
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
        private DownloadController ControllerRef;
        private List<DownloadThread> DownloadList;  
        
        public ConsoleDrawer(string prefixstring, DownloadController contref, int delayms=10) 
        {
            PrefixString = prefixstring;
            ControllerRef = contref;
            DelayMs = delayms;

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

            DownloadList = ControllerRef.ThreadList.Select(x => x.Copy()).ToList();
            int i = 0;
            foreach (DownloadThread download in DownloadList)
            {
                if (download.CanClearLine)
                {
                    ControllerRef.ThreadList[i].CanClearLine = false;
                    ClearLine();
                }
                Console.WriteLine($"{download.ThreadName}: {download.ProgressAbsolute - download.Start}/{download.Size} bytes {download.ProgressRelative.ToString("N2")}% {download.DownloadStatus.GetDescription()} Start:{download.Start} End:{download.End}");
                i++;
            }
            
            
            
           
            
        }
        private void ClearLine()
        {
            int CurrentLineCursor = Console.CursorTop;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, CurrentLineCursor);
        }
    }
}

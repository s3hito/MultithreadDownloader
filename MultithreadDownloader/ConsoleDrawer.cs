using System;
using System.Collections.Generic;
using System.Linq;
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
        public ConsoleDrawer(string prefixstring, List<DownloadThread> downlist, int delayms=10) 
        {
            PrefixString = prefixstring;
            DownloadList = downlist;
            DelayMs = delayms;

        }

        public async Task Start()
        {
            Switch = true;
            Print();
        }
        public void Stop() 
        { 
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
            bool ackquireqlock=false;
            Monitor.TryEnter(DownloadList,ref ackquireqlock);
            if (ackquireqlock)
            {

            
            foreach (DownloadThread download in DownloadList)//Holy crap it freezes right here I saw it in a debugger.
                                                                         //Why tf it does that????? I got no clue for some reason it
                                                                         //doesn't go through the list it just stays here forever.
                                                                         //I don't fucking know why this is happening. pizdec.... 
            {
                if (download.CanClearLine)
                {
                    download.CanClearLine= false;
                    ClearLine();
                }
                Console.WriteLine($"{download.ThreadName}: {download.ProgressAbsolute - download.Start} bytes {download.ProgressRelative.ToString("N2")}% {download.Status} Start:{download.Start} End:{download.End}");

            }
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

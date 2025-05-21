using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloader
{
    public delegate void GenerateMenu();

    public class DownloadManagerDrawer
    {
        private DownloadsManager dlManRef;
        private GenerateMenu currentMenuGenerator;
        private List<DownloadThread> threadlist;
        private DownloadController controller;

        private ConsoleKeyInfo key;

        int i;
        private int delayms;
        private int menuIndex;
        private long PGChunkSize;

        private bool exitFlag=false;
        private bool clearFlag=false;
        private bool isInDetailsMenu=false;

        private string prefixstring;
        private string progressBar;

        public DownloadManagerDrawer(DownloadsManager dlman, int delayms=10)
        {
            dlManRef = dlman;
            this.delayms = delayms;

            currentMenuGenerator = MainMenuGenerator;
        }


        public void StartDrawer()
        {
            UpdatePrefixString();
            Task.Run(CheckInput);
            while (!exitFlag)
            {
                UpdateConsole();
                Task.Delay(delayms);
            }

        }

        private async Task UpdateConsole()
        {

            if (clearFlag) Console.Clear(); clearFlag = false;
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(prefixstring);
            /*
            if (download.CanClearLine)
            {
                ControllerRef.ThreadList[i].CanClearLine = false; //Add this later
                ClearLine();
            }
            */
            currentMenuGenerator();


        }

        private void MainMenuGenerator()
        {

            for (i=0; i < dlManRef.downloadControllers.Count;i++)
            {
                if (i == menuIndex) { Console.BackgroundColor = ConsoleColor.Gray; Console.ForegroundColor = ConsoleColor.Black; }

                Console.Write($"{dlManRef.downloadControllers[i].Filename}");// <-- add progressbar here
                Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($": {dlManRef.downloadControllers[i].ProgressPercentage.ToString("N2")}% {MakeProgressBar(20, dlManRef.downloadControllers[i])}");

            }
        }

        private void DownloadDetailGenerator()
        {
            
            threadlist = controller.ThreadList.Select(x => x.Copy()).ToList();

            foreach (DownloadThread thread in controller.ThreadList)
            {
                Console.WriteLine($"{thread.ThreadName}: {thread.ProgressAbsolute - thread.Start}/{thread.Size} bytes {thread.Status.ToString()} Proxy: {thread.Proxy} Reconnections: {thread.ReconnectCount}");
            }
        }




        private void UpdatePrefixString()
        {
            if (isInDetailsMenu)
            {
                prefixstring= $"{controller.URL} \n" +
                       $"Size: {controller.Size}\n" +
                       $"Chunk size: {controller.SectionLength} \n" +
                       $"Number of threads: {controller.ThreadNumber}\n" +
                       $"----------------------------"; ;
            }
            else prefixstring = $"Use UP/DN to move, ENTER to select, ESC - exit, P - toggle pause, X - delete\n" +
                    $"Total downloads: {dlManRef.downloadControllers.Count}";
        }

        private async Task CheckInput()
        {
            while (!exitFlag)
            {
                key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (menuIndex > 0 && !isInDetailsMenu) menuIndex--;
                        else menuIndex = dlManRef.downloadControllers.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        if (menuIndex < dlManRef.downloadControllers.Count - 1 && !isInDetailsMenu) menuIndex++;
                        else menuIndex = 0;
                        break;
                    case ConsoleKey.Enter:
                        isInDetailsMenu = true;
                        clearFlag = true;
                        currentMenuGenerator = DownloadDetailGenerator;
                        controller = dlManRef.downloadControllers[menuIndex];
                        UpdatePrefixString();
                        break;
                    //activate a switch to the menu for displaying download details
                    case ConsoleKey.Escape:
                        isInDetailsMenu = false;
                        clearFlag = true;
                        UpdatePrefixString();
                        currentMenuGenerator = MainMenuGenerator;
                        break;
                    case ConsoleKey.P:
                        controller= dlManRef.downloadControllers[menuIndex];
                        dlManRef.ToggleDownload(controller);
                        clearFlag = true;
                        break; 


                }
            }
        }
        private string MakeProgressBar(int length, DownloadController controllerRef)
        {
            PGChunkSize = controllerRef.BytesLength / length;

            progressBar = "[";

            for (int i = 0; i < controllerRef.TotalProgress / PGChunkSize; i++)
            {
                progressBar += "=";
            }
            if (controllerRef.TotalProgress % PGChunkSize > PGChunkSize / 2)
            {
                progressBar += "-";
            }
            else if (controllerRef.TotalProgress / controllerRef.BytesLength < 1)
            {
                progressBar += " ";
            }


            for (int i = 0; i < length - 1 - (controllerRef.TotalProgress / PGChunkSize); i++)
            {
                progressBar += " ";
            }

            progressBar += "]";

            return progressBar;
        }
        private void ClearLine()
        {
            int CurrentLineCursor = Console.CursorTop;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, CurrentLineCursor);
        }
    }
}

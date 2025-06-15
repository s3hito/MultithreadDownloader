using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MultithreadDownloader
{
    public delegate void GenerateMenu();

    public class DownloadManagerDrawer
    {
        private DownloadsManager dlManRef;
        private GenerateMenu currentMenuGenerator;
        private List<DownloadThread> threadlist;
        private DownloadController controller;
        private int controllerToDeleteIdx;

        private ConsoleKeyInfo key;

        private int threadnumber;
        int i;
        private int delayms;
        private int menuIndex;
        private long PGChunkSize;

        private bool exitFlag=false;
        private bool clearFlag=false;
        private bool isInDetailsMenu=false;
        private bool deleteFlag = false;
        private bool pauseFlag = false;

        private string link;
        private string prefixstring;
        private string progressBar;

        public DownloadManagerDrawer(DownloadsManager dlman, int delayms=20)
        {
            dlManRef = dlman;
            this.delayms = delayms;

            currentMenuGenerator = MainMenuGenerator;
        }


        public async Task StartDrawer()
        {
            UpdatePrefixString();
            Task.Run(CheckInput);
            while (!exitFlag)
            {
                if (pauseFlag) { await Task.Delay(delayms); continue; }
                UpdateConsole();
                await Task.Delay(delayms);
            }

        }

        private async Task UpdateConsole()
        {

            if (clearFlag) { Console.Clear(); clearFlag = false; }
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(prefixstring);

            if (dlManRef.downloadControllers.Count!=0) currentMenuGenerator();

            if (deleteFlag) { DeleteController(); UpdatePrefixString(); }//delete controller safely


        }

        private void MainMenuGenerator()
        {

            for (i=0; i < dlManRef.downloadControllers.Count;i++)
            {
                if (dlManRef.downloadControllers[i].canClearLine == true) { dlManRef.downloadControllers[i].canClearLine = false; ClearLine(); }
                    
                if (i == menuIndex) { Console.BackgroundColor = ConsoleColor.Gray; Console.ForegroundColor = ConsoleColor.Black; }

                Console.Write($"{dlManRef.downloadControllers[i].Filename}");
                Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($": {dlManRef.downloadControllers[i].ProgressPercentage.ToString("N2")}% {MakeProgressBar(20, dlManRef.downloadControllers[i])}");

            }
        }

        private void DownloadDetailGenerator()
        {
            Console.WriteLine($"Progress: {controller.ProgressPercentage.ToString("N2")}% {progressBar}");
            for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write('-');
            Console.WriteLine("");
            threadlist = controller.ThreadList.Select(x => x.Copy()).ToList();

            foreach (DownloadThread thread in controller.ThreadList)
            {
                if (thread.canClearLine) { thread.canClearLine = false; ClearLine(); }

                Console.WriteLine($"{thread.ThreadName}: {thread.ProgressAbsolute - thread.Start}/{thread.Size} bytes {thread.Status.ToString()} Proxy: {thread.Proxy} Reconnections: {thread.ReconnectCount}");
            }
        }

        private void AddDownloadGenerator()
        {
            Console.Clear();
            Console.WriteLine("Enter a link: ");
            link = Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Enter thread number: ");
            threadnumber= Convert.ToInt32(Console.ReadLine());
            dlManRef.AddDownloadFromLink(link, threadnumber, new FileManager(), new ProxyConfiguration { DistributionPolicy = ProxyManager.ProxyDistributionStates.MultipleCycle, OutOfProxyBehaviour = ProxyManager.OutOfProxyBehaviourStates.StartOver });
            pauseFlag = false;
        }




        private void UpdatePrefixString()
        {
            if (isInDetailsMenu)
            {
                prefixstring = $"{controller.URL} \n" +
                       $"Size: {controller.Size}\n" +
                       $"Chunk size: {controller.SectionLength} \n" +
                       $"Number of threads: {controller.ThreadNumber}";
            }
            else prefixstring = $"Use UP/DN to move, ENTER to select, ESC - exit, P - toggle pause, X - delete, A - add\n" +
                    $"Total downloads: {dlManRef.downloadControllers.Count}";
        }

        private void DeleteController()
        {
            dlManRef.DeleteDownload(controllerToDeleteIdx);
            deleteFlag = false;
            clearFlag = true;
        }

        private async Task CheckInput()
        {
            try
            {
                while (!exitFlag)
                {
                
                    key = Console.ReadKey();
                    if (dlManRef.downloadControllers.Count == 0 && key.Key!=ConsoleKey.A) continue;
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
                            controller = dlManRef.downloadControllers[menuIndex];
                            dlManRef.ToggleDownload(controller);
                            clearFlag = true;
                            break;
                        case ConsoleKey.X:

                            controllerToDeleteIdx= menuIndex;
                            isInDetailsMenu = false;
                            deleteFlag = true;
                            break;

                        case ConsoleKey.A:
                            pauseFlag = true;
                            AddDownloadGenerator();
                            break;

                    }
                   

                }
            }
            
            catch (Exception ex)
            {
                // Log the exception so you can see what's happening
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"CheckInput thread terminated with exception: {ex.Message}");
                // Optionally restart the input checking
                // await Task.Delay(1000);
                // _ = Task.Run(CheckInput); // Restart the input task
            }


        }
        private string MakeProgressBar(int length, DownloadController controllerRef)
        {
            if (controllerRef == null) return "";
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

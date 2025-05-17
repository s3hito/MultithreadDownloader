using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Configuration;
using static MultithreadDownloader.ProxyManager;


namespace MultithreadDownloader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //string link = "https://files.testfile.org/Video%20MP4%2FRoad%20-%20testfile.org.mp4";
            string link = "https://www.spiggle-theis.com/images/videos/BET_.mp4";


            //DownloadController dnl = new DownloadController("testfile.mp4", link, 1, _useproxy: true, _proxyaddress: "117.250.3.58:8080");// with proxy
            FileManager FMan = new FileManager();
            KeyValueConfigurationCollection Config = FMan.LoadConfiguration();
            DownloadsManager dMan=new DownloadsManager();
            bool createNewDownload=true;
            if (createNewDownload)
            {
                DownloadController dnl = new DownloadController(link, 2, FMan, Config);
                dnl.StartDownloadAsync();
                Console.ReadLine();
                dnl.Pause();
                Console.ReadLine();
                List<DownloadState> downloads = dMan.LoadSavedDownloads();

                foreach (DownloadState download in downloads)
                {
                    DownloadController controller = new DownloadController(download, FMan, Config);
                    controller.Continue();
                    Console.ReadLine();

                }

            }

           
          

            

        }



    }
}

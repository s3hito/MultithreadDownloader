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
            string link2 = "https://r3---sn-oxuctoxu-n8vl.gvt1.com/edgedl/android/studio/install/2024.3.1.15/android-studio-2024.3.1.15-windows.exe?cms_redirect=yes&met=1745513094,&mh=Ma&mip=176.192.200.200&mm=28&mn=sn-oxuctoxu-n8vl&ms=nvh&mt=1745512578&mv=m&mvi=3&pl=19&rmhost=r6---sn-oxuctoxu-n8vl.gvt1.com&rms=nvh,nvh&shardbypass=sd";


            //DownloadController dnl = new DownloadController("testfile.mp4", link, 1, _useproxy: true, _proxyaddress: "117.250.3.58:8080");// with proxy
            FileManager FMan = new FileManager();
            KeyValueConfigurationCollection Config = FMan.LoadConfiguration();
            DownloadsManager dMan=new DownloadsManager();



            List<DownloadState> downloads = dMan.LoadSavedDownloads();
            DownloadManagerDrawer dmanDrawer = new DownloadManagerDrawer(dMan);
            dMan.AddDownloadFromLink(link, 2, FMan, Config);
            await dmanDrawer.StartDrawer();
            

        }



    }
}

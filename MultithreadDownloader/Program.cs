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
            string link = "https://swupdate.openvpn.net/downloads/connect/openvpn-connect-3.4.4.3412_signed.msi";
            //string link = "https://sample.mp4-download.com/DUBAI,%20United%20Arab%20Emirates%20In%208K%20ULTRA%20HD%20HDR%2060%20FPS..mp4";
            //string link = "https://files.testfile.org/ZIPC/60MB-Corrupt-Testfile.Org.zip";
            //string link = "https://files.testfile.org/Video%20MP4%2FSand%20-%20testfile.org.mp4";
            //string link = "https://files.testfile.org/Video%20MP4%2FRoad%20-%20testfile.org.mp4";
            //string link = "https://sample.mp4-download.com/8k2.mp4";
            //DownloadController dnl = new DownloadController("testfile.mp4", link, 1, _useproxy: true, _proxyaddress: "117.250.3.58:8080");// with proxy
            FileManager FMan = new FileManager();
            KeyValueConfigurationCollection Config = FMan.LoadConfiguration();
            DownloadController dnl = new DownloadController(link, 10, FMan, Config);
            await dnl.PrintData();


            Console.ReadLine();
            Console.ReadLine();

        }



    }
}

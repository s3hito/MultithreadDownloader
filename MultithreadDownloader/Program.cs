using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;


namespace MultithreadDownloader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //string link = "https://files.testfile.org/Video%20MP4%2FRoad%20-%20testfile.org.mp4";
            //string link = "https://swupdate.openvpn.net/downloads/connect/openvpn-connect-3.4.4.3412_signed.msi";
            string link = "https://sample.mp4-download.com/DUBAI,%20United%20Arab%20Emirates%20In%208K%20ULTRA%20HD%20HDR%2060%20FPS..mp4";
            //string link = "https://ash-speed.hetzner.com/100MB.bin";
            //DownloadController dnl = new DownloadController("testfile.mp4", link, 1, _useproxy: true, _proxyaddress: "117.250.3.58:8080");// with proxy
            DownloadController dnl = new DownloadController("testfile.mp4", link, 4);// without proxy
            dnl.PrintData();


            Console.ReadLine();
        }



    }
}

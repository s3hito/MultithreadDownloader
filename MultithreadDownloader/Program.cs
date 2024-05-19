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
            string link = "https://sample.mp4-download.com/DUBAI,%20United%20Arab%20Emirates%20In%208K%20ULTRA%20HD%20HDR%2060%20FPS..mp4";
            DownloadController dnl = new DownloadController("testfile.mp4", link, 8);
            dnl.PrintData();


            Console.ReadLine();
        }



    }
}

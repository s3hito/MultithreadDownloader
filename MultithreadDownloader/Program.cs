using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

///eer
namespace MultithreadDownloader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string link = "https://sample-videos.com/video321/mp4/720/big_buck_bunny_720p_10mb.mp4";
            DownloadController dnl = new DownloadController("testfile.mp4", link, 4);
            dnl.PrintData();
            ///dnl.Start();
            ///dnl.CombineTempFiles(); 
            ///dnl.DeleteTempFiles();
            Console.ReadLine();
        }



    }
}

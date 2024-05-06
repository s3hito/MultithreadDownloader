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
            string gg;
            string link = "https://file-examples.com/storage/fe4996602366316ffa06467/2017/04/file_example_MP4_1280_10MG.mp4";
            DownloadController dnl = new DownloadController("testfile.mp4", link, 4);
            dnl.PrintData();
            ///dnl.Start();
            ///dnl.CombineTempFiles(); 
            ///dnl.DeleteTempFiles();
            Console.ReadLine();
        }



    }
}

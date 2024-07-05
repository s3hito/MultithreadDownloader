using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloader
{
    
    internal class FileManager
    {
        private string Filename;
        private string Path;
        private int FileCount;
        public FileManager(string fname, string path, int filecount ) 
        {
            Filename = fname;
            Path = path;
            FileCount = filecount;
        }

        public void CreateFolder()
        {
            Directory.CreateDirectory( Path+"\\"+Filename+".temp");
        }

        public void RemoveDirectory() 
        { 
            Directory.Delete(Path + "\\" + Filename + ".temp", true );
        }

        public void CombineTempFiles()
        {

            using (FileStream OutFile = new FileStream($"{Path+"\\"+Filename}", FileMode.Create, FileAccess.Write))
            {
                for (int i = 0; i < FileCount; i++)
                {
                    int bytesread = 0;
                    byte[] buffer = new byte[1024];
                    using (FileStream InputFile = new FileStream($"{Path}\\{Filename}.temp\\{Filename}.temp{i}", FileMode.Open, FileAccess.Read))
                    {
                        do
                        {
                            bytesread = InputFile.Read(buffer, 0, buffer.Length);
                            OutFile.Write(buffer, 0, bytesread);
                        }
                        while (bytesread > 0);
                    }

                }
            }
        }

        public void DeleteTempFiles()
        {
            string[] TempFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*temp*");
            foreach (string TempFile in TempFiles)
            {
                File.Delete(TempFile);
            }
        }
    }
}

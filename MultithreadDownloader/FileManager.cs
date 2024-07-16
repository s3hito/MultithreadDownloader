using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;

namespace MultithreadDownloader
{
    
    public class FileManager
    {
        private string Filename;
        private string PathToTempFolder;
        private string PathtToMainFolder;
        private string PathToMainFile;
        private int FileCount;
        
        public void SetValues(string fname, string path, int filecount)
        {
            Filename = fname;
            PathToTempFolder = path + "\\" + fname + ".temp";
            PathToMainFile = path + "\\" + fname;

            PathtToMainFolder = path;
            FileCount = filecount;
        }

        public void CreateDirectory()
        {
            Directory.CreateDirectory(PathToTempFolder);
        }

        public void RemoveDirectory() 
        { 
            Directory.Delete(PathToTempFolder, true );
        }

        public List<string> FetchProxyFile()
        {
            return ;
        }

        public void DumpProxyList()
        {

        }

        public void CombineTempFiles()
        {

            using (FileStream OutFile = new FileStream(PathToMainFile, FileMode.Create, FileAccess.Write))
            {
                for (int i = 0; i < FileCount; i++)
                {
                    int bytesread = 0;
                    byte[] buffer = new byte[1024];
                    using (FileStream InputFile = new FileStream($"{PathToTempFolder}\\{Filename}.temp{i}", FileMode.Open, FileAccess.Read))
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

        public KeyValueConfigurationCollection LoadConfiguration()
        {
            Configuration ConfManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection ConfCollection = ConfManager.AppSettings.Settings;
            return ConfCollection;
        }

    }
}

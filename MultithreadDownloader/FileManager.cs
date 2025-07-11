﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;


namespace MultithreadDownloader
{
    
    public class FileManager
    {
        private string Filename;
        private string PathToTempFolder;
        private string PathtToMainFolder;
        private string PathToMainFile;
        private string SavedDownloadsPath;
        private string SavedDownloadsFolder= "SavedDownloads";
        private int FileCount;
        public Action CreateDirectory;
        
        public void SetValues(string fname, string path, int filecount)
        {
            Filename = fname;
            PathToTempFolder = path + "\\" + fname + ".temp";
            PathToMainFile = path + "\\" + fname;
            SavedDownloadsPath = Path.Combine(path, SavedDownloadsFolder);

            PathtToMainFolder = path;
            FileCount = filecount;


            CreateDirectory = () => Directory.CreateDirectory(PathToTempFolder);
        }


        
        

        public List<string> FetchProxyFile(string proxyfilename="proxylist.txt")
        {
            using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + $"\\{proxyfilename}", FileMode.OpenOrCreate)){}
            List<string> lines =File.ReadLines(proxyfilename).ToList();
            
            return lines;
        }

        public void DumpProxyList(List<string> proxylist)
        {
            using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\proxylist.txt", FileMode.OpenOrCreate)) { }
            File.WriteAllLines("proxylist.txt", proxylist);
        }

        public void CombineTempFiles()
        {
            try
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

            catch (Exception ex)
            {
                throw new Exception("Exception: ", ex);
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

        public void RemoveTempDirectory()
        {
            Directory.Delete(PathToTempFolder, true);
        }

        public void RemoveDownloadedFile()
        {
            File.Delete(PathToMainFile);
        }


        public void SaveDownloadStateToFile(DownloadState state)
        {
            try
            {
                string stateFilePath = Path.Combine(SavedDownloadsPath, $"{Filename}.state.json");
                string jsonState = JsonConvert.SerializeObject(state);
                File.WriteAllText(stateFilePath, jsonState);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Thread terminated with exception: {ex.Message}");
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

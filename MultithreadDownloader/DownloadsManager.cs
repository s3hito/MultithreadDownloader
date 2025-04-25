using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloader
{
    public class DownloadsManager
    {
        private string SavedDownloadsPath;
        private string SavedDownloadsFolder = "SavedDownloads";
        List<DownloadState> downloads;
        public DownloadsManager() 
        {
            SavedDownloadsPath = Path.Combine(Directory.GetCurrentDirectory(), SavedDownloadsFolder);
            if (!Directory.Exists(SavedDownloadsFolder)) Directory.CreateDirectory(SavedDownloadsPath);


        }

        public List<DownloadState> LoadSavedDownloads()
        {
            List<DownloadState> downloads = new List<DownloadState>();

            string[] stateFiles = Directory.GetFiles(SavedDownloadsFolder, "*.state.json");


            foreach (string stateFile in stateFiles)
            {
                string jsonState = File.ReadAllText(stateFile);
                DownloadState state = JsonConvert.DeserializeObject<DownloadState>(jsonState);
                downloads.Add(state);
            }
            return downloads;
        }

    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloader
{
    public class DownloadsManager
    {
        private string SavedDownloadsPath;
        private string SavedDownloadsFolder = "SavedDownloads";

        private DownloadState downloadState;
        private List<DownloadState> downloadsStates;
        public List<DownloadController> downloadControllers;
        DownloadManagerDrawer ManagerDrawer;
        
        public DownloadsManager() 
        {
            SavedDownloadsPath = Path.Combine(Directory.GetCurrentDirectory(), SavedDownloadsFolder);
            if (!Directory.Exists(SavedDownloadsFolder)) Directory.CreateDirectory(SavedDownloadsPath);
            ManagerDrawer = new DownloadManagerDrawer(this);
            downloadControllers = new List<DownloadController>();
        }

        public void AddDownloadFromLink(string link, int tnum, FileManager fman, KeyValueConfigurationCollection config)
        {
            DownloadController controller = new DownloadController(link, tnum, fman, config);
            AddDownloadController(controller);
        }



        public void ToggleDownload(DownloadController controller)
        {
            if (controller.Status == DownloadStatuses.Paused || controller.Status == DownloadStatuses.Idle) 
            {
                downloadState = GetNeededDownload(controller);
                if (downloadState != null) controller.CreateDownloadFromState(downloadState); // if download already exists, tell controller where to start the download from

                controller.Continue();
            } 
            else if (controller.Status == DownloadStatuses.Downloading) controller.Pause();
        }

        public void AddDownloadController(DownloadController controller)
        {
            downloadControllers.Add(controller);
        }

        private DownloadState GetNeededDownload(DownloadController controller)
        {
            LoadSavedDownloads();
            downloadState= downloadsStates.Where(download => download.Filname == controller.Filename).FirstOrDefault();
            //select from all loaded downloads needed one
            return downloadState;
        }
        public List<DownloadState> LoadSavedDownloads()
        {
            downloadsStates = new List<DownloadState>();

            string[] stateFiles = Directory.GetFiles(SavedDownloadsFolder, "*.state.json");


            foreach (string stateFile in stateFiles)
            {
                string jsonState = File.ReadAllText(stateFile);
                DownloadState state = JsonConvert.DeserializeObject<DownloadState>(jsonState);
                downloadsStates.Add(state);
            }

            return downloadsStates;
        }

        public void DeleteDownload(int controllerIdx)
        {
            downloadControllers[controllerIdx].Cancel();
            DeleteStateFile(downloadControllers[controllerIdx]);
            downloadControllers.RemoveAt(controllerIdx);

        }

        public void DeleteStateFile(DownloadController controller)
        {
            string[] stateFiles = Directory.GetFiles(SavedDownloadsFolder, "*.state.json");


            foreach (string stateFile in stateFiles)
            {
                string jsonState = File.ReadAllText(stateFile);
                DownloadState state = JsonConvert.DeserializeObject<DownloadState>(jsonState);
                if (state.Filname == controller.Filename)
                {
                    File.Delete(Path.Combine(Directory.GetCurrentDirectory(), stateFile));
                }
            }
        }
    }
}

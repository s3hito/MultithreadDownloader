using MultithreadDownloader;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WFPUI.Commands;
using WFPUI.Views;
using System.Windows;
using System.IO;


namespace WFPUI.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private int _threascount;
        private string _proxytext;
        private string _downloadsPath;
        private string _temppath;
        private string _proxyfilename;
        private FileManager _filemanager;
        private KeyValueConfigurationCollection config;

        public SettingsViewModel()
        {
            _filemanager = new FileManager();
            config = _filemanager.LoadConfiguration();
            _proxyfilename = config["ProxyFileName"].Value;
            ProxyText = string.Join('\n', _filemanager.FetchProxyFile(_proxyfilename));
            PathToTempFiles = config["TempPath"].Value;
            PathToDownloadedFiles = config["Path"]?.Value ?? "";
            ConnectionNumer = Convert.ToInt32(config["DeaultThreads"]?.Value??"1");
        }

        public string ProxyText { get { return _proxytext; } set { _proxytext = value; OnPropertyChanged(); } }
        public string PathToTempFiles { get { return _temppath; } set { _temppath = value; OnPropertyChanged(); } }
        public string PathToDownloadedFiles { get  { return _downloadsPath; } set {_downloadsPath = value; OnPropertyChanged(); } }
        public int ConnectionNumer { get { return _threascount; } set { _threascount = value; OnPropertyChanged(); } }
        public ICommand ConfirmCommand => new RelayCommand(param => ConfirtSettings(param));
        public ICommand CancelCommnad => new RelayCommand(param => CancelSettings(param));

        private void ConfirtSettings(object windowObj)
        {
            SaveConfig();
            Window settingsWindow = (Window)windowObj;
            settingsWindow.Close();

            
        }
        private void CancelSettings(object windowObj) 
        {
            Window settingsWindow = (Window)windowObj;
            settingsWindow.Close();
        }

        private void SaveConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("Path");
            config.AppSettings.Settings.Remove("TempPath");
            config.AppSettings.Settings.Remove("DeaultThreads");
            config.AppSettings.Settings.Add("Path", Path.GetFullPath(PathToDownloadedFiles));
            config.AppSettings.Settings.Add("TempPath", Path.GetFullPath(PathToTempFiles));
            config.AppSettings.Settings.Add("DeaultThreads", ConnectionNumer.ToString());
            config.Save(ConfigurationSaveMode.Modified);
        }
    }

}

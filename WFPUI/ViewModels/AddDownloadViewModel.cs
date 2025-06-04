using MultithreadDownloader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WFPUI.Commands;
using WFPUI.Views;

namespace WFPUI.ViewModels
{
    public class AddDownloadViewModel : ObservableObject
    {
        private string _url;
        private int _threadNumber;
        private DownloadsManager _downloadsManager;
        private ProxyDistributionOption _selectedProxyDistribution;
        private OutOfProxyBehaviourOption _selectedOutOfProxyBehaviour;
        public AddDownloadViewModel()
        {

        }
        public AddDownloadViewModel(DownloadsManager dlman)
        {
            _url = "";
            _threadNumber = 1;
            _downloadsManager = dlman;
            InitializeDefaults();



        }

        public string Url { get => _url; set { _url = value; }}
        public int ThreadNumber { get => _threadNumber; set { _threadNumber = value; } }
        public ObservableCollection<ProxyDistributionOption> ProxyDistributionOptions { get; private set; }
        public ObservableCollection<OutOfProxyBehaviourOption> OutOfProxyBehaviourOptions { get; private set; }
        public ProxyDistributionOption SelectedProxyOption 
        { 
            get => _selectedProxyDistribution;
            set {
                _selectedProxyDistribution = value;
                OnPropertyChanged(nameof(IsOutOfProxyEnabled));

            }
        }
        public OutOfProxyBehaviourOption SelectedOutOfProxyBehaviour
        {
            get => _selectedOutOfProxyBehaviour;
            set => _selectedOutOfProxyBehaviour = value;
            //OnPropertyChange();

        }
        public bool IsOutOfProxyEnabled 
        
        { get 
            {
                return _selectedProxyDistribution != null &&
                       _selectedProxyDistribution.Value != ProxyManager.ProxyDistributionStates.NoProxy;
            } 
        }

         


        public ICommand AddCommand => new RelayCommand(parameter => AddDownload(parameter));
        public ICommand CancelCommand => new RelayCommand(window => CloseWindow(window));





        private void AddDownload(object windowObj)
        {
            ProxyConfiguration proxConf = new ProxyConfiguration { DistributionPolicy = _selectedProxyDistribution.Value, OutOfProxyBehaviour = _selectedOutOfProxyBehaviour.Value };
            _downloadsManager.AddDownloadFromLink(_url, _threadNumber, new FileManager(), proxConf);
            Window window = (Window)windowObj;
            window.Close();

        }

        private void CloseWindow(object windowObj)
        {
            Window window = (Window)windowObj;
            window.Close();
        }

        private void InitializeDefaults()
        {
            ProxyDistributionOptions = new ObservableCollection<ProxyDistributionOption>
            {
                new ProxyDistributionOption("No Proxy", ProxyManager.ProxyDistributionStates.NoProxy),
                new ProxyDistributionOption("Single Proxy", ProxyManager.ProxyDistributionStates.Single),
                new ProxyDistributionOption("Single Proxy with Switching", ProxyManager.ProxyDistributionStates.SingleSwithching),
                new ProxyDistributionOption("Multiple Proxies", ProxyManager.ProxyDistributionStates.Multiple),
                new ProxyDistributionOption("Multiple Proxies (Cycle)", ProxyManager.ProxyDistributionStates.MultipleCycle)
            };

            OutOfProxyBehaviourOptions = new ObservableCollection<OutOfProxyBehaviourOption>
            {
                new OutOfProxyBehaviourOption("Don't Use Proxy", ProxyManager.OutOfProxyBehaviourStates.DontUseProxy),
                new OutOfProxyBehaviourOption("Use Last Used Proxy", ProxyManager.OutOfProxyBehaviourStates.UseLastUsedProxy),
                new OutOfProxyBehaviourOption("Start Over", ProxyManager.OutOfProxyBehaviourStates.StartOver)
            };
            _selectedProxyDistribution = ProxyDistributionOptions.FirstOrDefault();
            _selectedOutOfProxyBehaviour = OutOfProxyBehaviourOptions.FirstOrDefault();

        }
    }

}

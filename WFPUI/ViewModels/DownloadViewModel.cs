using MultithreadDownloader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Resources;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WFPUI.Commands;

namespace WFPUI.ViewModels
{
    public class DownloadViewModel : ObservableObject
    {
        private DownloadController _downloadController;
        private DownloadsManager downloadsManager;

        public DownloadViewModel()
        {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                // Only execute this code at runtime, not design time

            }

            else
            {
                // Create mock data for design time
                _downloadController = null;
            }

        }

        public DownloadViewModel(DownloadController controller)
        {
            _downloadController = controller;
            _downloadController.PropertyChanged += DownloadController_PropertyChanged;

        }



        public DownloadController ActiveDownload => _downloadController;
        public ObservableCollection<DownloadThread> ThreadList => _downloadController?.ThreadList;
        public double TotalProgress => _downloadController?.TotalProgress ?? 0;
        public string URL => _downloadController?.URL ?? string.Empty;
        public string Status => _downloadController?.Status ?? "Unknown";
        public string Size => _downloadController?.Size ?? "0 B";
        public long SectionLength => _downloadController?.SectionLength ?? 0;
        public double ProgressPercentage => _downloadController?.ProgressPercentage ?? 0;





        public ICommand PauseCommand => new RelayCommand(_ => PauseDownload());
        public ICommand CancelCommand => new RelayCommand(_ => CancelDownload());

        private void PauseDownload()
        {



        }

        private void CancelDownload()
        {
            _downloadController.Cancel();
        }
        private void StartDownload()
        {
            _downloadController.StartDownloadAsync();

        }

        private void DownloadController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DownloadController.TotalProgress):
                    OnPropertyChanged(nameof(TotalProgress));
                    break;
                case nameof(DownloadController.URL):
                    OnPropertyChanged($"{nameof(DownloadController.URL)}");
                    break;
                case nameof(DownloadController.Status):
                    OnPropertyChanged(nameof(Status));
                    break;
                case nameof(DownloadController.Size):
                    OnPropertyChanged(nameof(Size));
                    break;
                case nameof(DownloadController.SectionLength):
                    OnPropertyChanged(nameof(SectionLength));
                    break;
                case nameof(DownloadController.ProgressPercentage):
                    OnPropertyChanged(nameof(ProgressPercentage));
                    break;
                case nameof(DownloadController.ThreadList):
                    OnPropertyChanged(nameof(ThreadList));
                    break;
            }


        }

        public void Dispose()
        {
            _downloadController.PropertyChanged-=DownloadController_PropertyChanged;
        }
    }
}

using MultithreadDownloader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WFPUI.Commands;
using WFPUI.Views;

namespace WFPUI.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private DownloadsManager downloadsManager;
        private DownloadController _selectedController;
        private ObservableCollection<DownloadController> _downloads;

        public MainWindowViewModel()
        {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                downloadsManager = new DownloadsManager();
                //downloadsManager.LoadSavedDownloads();
                //downloadsManager.CreateDownloadsFromStates();
                _downloads = downloadsManager.downloadControllers;
                downloadsManager.PropertyChanged += DownloadsManaager_PropertyChanged;

                string link = "https://www.spiggle-theis.com/images/videos/BET_.mp4";
                //downloadsManager.AddDownloadFromLink(link, 1, new FileManager());
            }


        }

        public ObservableCollection<DownloadController> DownloadsList => _downloads;

        private void DownloadsManaager_PropertyChanged(object sender, PropertyChangedEventArgs e) //kinda useless for now 
        {
            switch (e.PropertyName)
            {
                case nameof(DownloadsManager.downloadControllers):
                    OnPropertyChanged(nameof(DownloadsList));
                    break;
            }

        }

        public DownloadController SelectedController
        {
            get => _selectedController;
            set
            {

                _selectedController = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }


        public ICommand ToggleCommand => new RelayCommand(_ => ToggleDownload(), _ => SelectedController != null);
        public ICommand CancelCommand => new RelayCommand(_ => CancelDownload(), _ => SelectedController != null);
        public ICommand DoubleClickCommand => new RelayCommand(parameter => Item_DoubleClick(parameter));

        public ICommand AddDownloadCommand => new RelayCommand(_ =>  AddDownload());
        private void ToggleDownload()
        {

            downloadsManager.ToggleDownload(SelectedController);

        }

        private void CancelDownload()
        {
            downloadsManager.DeleteDownload(SelectedController);// Add functionality to check if the download is completed and in that case delete the completed file
        }


        public void Item_DoubleClick(object parameter)
        {
            DownloadDetailsWindow detailsWin = new DownloadDetailsWindow( downloadsManager,(DownloadController)parameter);
            detailsWin.Show();
        }

        private void AddDownload()
        {
            AddDownloadWindow addWin = new AddDownloadWindow(downloadsManager);
            addWin.Show();
        }


       

    }
}

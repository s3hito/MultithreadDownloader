using MultithreadDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WFPUI.ViewModels;

namespace WFPUI.Views
{
    /// <summary>
    /// Interaction logic for AddDownload.xaml
    /// </summary>
    public partial class AddDownloadWindow : Window
    {
        public AddDownloadWindow(DownloadsManager dlman)
        {
            InitializeComponent();
            AddDownloadViewModel viewModel = new AddDownloadViewModel(dlman); 
            this.DataContext = viewModel;
        }
    }
}

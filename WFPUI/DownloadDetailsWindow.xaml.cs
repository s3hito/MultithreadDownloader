using MultithreadDownloader;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
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

namespace WFPUI
{
    /// <summary>
    /// Interaction logic for DownloadDetailsWindow.xaml
    /// </summary>
    public partial class DownloadDetailsWindow : Window
    {
        DownloadController dnl;
        private List<DownloadThread> ThreadList;
        public DownloadDetailsWindow(DownloadController controller)
        {
            InitializeComponent();
            DownloadViewModel viewModel = new DownloadViewModel(controller);
            this.DataContext = viewModel;

            Closing += (s, e) => viewModel.Dispose();
        }


        private void lvThreadList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView _Listview = sender as ListView;
            GridView _Gridview = _Listview.View as GridView;
            double _ActualWidth = _Listview.ActualWidth - SystemParameters.VerticalScrollBarWidth;
            _Gridview.Columns.Last().Width = _ActualWidth;
        }


    }
   
    
}
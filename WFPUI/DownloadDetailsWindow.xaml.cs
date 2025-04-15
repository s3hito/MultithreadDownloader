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

namespace WFPUI
{
    /// <summary>
    /// Interaction logic for DownloadDetailsWindow.xaml
    /// </summary>
    public partial class DownloadDetailsWindow : Window
    {
        DownloadController dnl;
        private List<DownloadThread> ThreadList;
        public DownloadDetailsWindow()
        {
            InitializeComponent();
            /*
            FileManager FMan = new FileManager();
            string link = "https://www.spiggle-theis.com/images/videos/BET_.mp4";
            KeyValueConfigurationCollection Config = FMan.LoadConfiguration();
            dnl = new DownloadController(link, 10, FMan, Config, false);
            AddDownload();
            */
        }

        public void AddDownload()
        {

           

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
﻿using MultithreadDownloader;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace WFPUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// net7.0
    public partial class MainWindow : Window
    {
        //AddDow addwin = new AddWindow();
        DownloadController dnl;

        public MainWindow()
        {

            /*
            FileManager FMan = new FileManager();
            string link = "https://www.spiggle-theis.com/images/videos/BET_.mp4";
            KeyValueConfigurationCollection Config = FMan.LoadConfiguration();
            dnl = new DownloadController(link, 10, FMan, Config, false);
            AddDownload();
            */
            InitializeComponent();
            

        }



        public void AddDownload()
        {


            dnl.StartDownloadAsync();
            lvDownloadInfo.Items.Add(dnl);
        }

        private void lvDownloadInfo_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
            ListView _Listview = sender as ListView;
            GridView _Gridview = _Listview.View as GridView;
            double _ActualWidth = _Listview.ActualWidth - SystemParameters.VerticalScrollBarWidth;
            double _EvenWidth = _ActualWidth / (_Gridview.Columns.Count);
            
            for (Int32 i = 0; i< _Gridview.Columns.Count-1; i++)
            {
                _Gridview.Columns[i].Width = _EvenWidth;
                
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            btnAdd.Background = Brushes.Black;
            //addwin.Show();
           

        }

        private void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {



        }

        private void btnAdd_MouseEnter(object sender, MouseEventArgs e)
        {
            btnAdd.Background = new SolidColorBrush(Color.FromArgb(255, 161, 161, 161));
        }

        private void btnAdd_MouseLeave(object sender, MouseEventArgs e)
        {
            btnAdd.Background = Brushes.White;
        }

    }
}

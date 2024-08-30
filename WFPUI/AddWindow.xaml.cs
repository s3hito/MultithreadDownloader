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

namespace WFPUI
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        MainWindow mainWin;
        public AddWindow()
        {

            InitializeComponent();
            cmbProxyRule.Items.Add("No proxy");
            cmbProxyRule.Items.Add("Single");
            cmbProxyRule.Items.Add("Single switching");
            cmbProxyRule.Items.Add("Multiple");
            cmbProxyRule.Items.Add("Multiple cycle");

            cmbOutOfProxy.Items.Add("Use last used proxy");
            cmbOutOfProxy.Items.Add("Start over");
            cmbOutOfProxy.Items.Add("Don't use proxy");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mainWin = (MainWindow)Application.Current.MainWindow;

            mainWin.AddDownload();
            this.Hide();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}

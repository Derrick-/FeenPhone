using FeenPhone.WPFApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeenPhone.WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for AboutWPF.xaml
    /// </summary>
    public partial class AboutWPF : UserControl
    {
        public VersionInfoModel VersionInfo { get; private set; }
        public AboutWPF()
        {
            InitializeComponent();

            DataContext = this;

            VersionInfo = new VersionInfoModel();
        }

        private void AboutClose_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Uri uri = e.Uri;
            try { Process.Start(new ProcessStartInfo(uri.AbsoluteUri)); }
            catch { }
            e.Handled = true;
        }
    }
}

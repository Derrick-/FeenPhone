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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeenPhone.WPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LoadSettings();
            this.Closed += new EventHandler(Window_Closed);
            Settings.SaveSettings += Settings_SaveSettings;
        }

        private void LoadSettings()
        {
            Properties.Settings settings = Settings.Container;
        }

        private void Settings_SaveSettings(object sender, EventArgs e)
        {
            Properties.Settings settings = Settings.Container;

        }
        
        void Window_Closed(object sender, EventArgs e)
        {
            Settings.InvokeSaveSettings(this);
            Settings.Container.Save();

            AudioIn.Dispose();
            AudioOut.Dispose();
        }

    }
}

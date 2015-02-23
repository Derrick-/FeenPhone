using FeenPhone.WPFApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace FeenPhone.WPFApp
{
    /// <summary>
    /// Interaction logic for AddressBookWindow.xaml
    /// </summary>
    public partial class AddressBookWindow : Window
    {
        private static ObservableCollection<AddressBookEntry> _Entries = new ObservableCollection<AddressBookEntry>();
        public static ObservableCollection<AddressBookEntry> Entries
        {
            get { return _Entries; }
        }

        static  AddressBookWindow()
        {
            LoadSettings();
            Settings.AppClosing += Settings_SaveSettings;
        }

        public AddressBookEntry Selected { get; set; }

        public static DependencyProperty ServerIsValidProperty = DependencyProperty.Register("ServerIsValid", typeof(bool?), typeof(AddressBookWindow), new PropertyMetadata(null));
        public bool? ServerIsValid
        {
            get { return (bool?)this.GetValue(ServerIsValidProperty); }
            set { this.SetValue(ServerIsValidProperty, value); }
        }

        public AddressBookWindow()
        {
            InitializeComponent();

            listView.ItemsSource = Entries;

            Selected = null;

        }

        private static void Settings_SaveSettings(object sender, EventArgs e)
        {
            var settings = Settings.Container;
            
            var items = new System.Collections.Specialized.StringCollection();
            foreach (var item in Entries)
                items.Add(string.Join(";", item.Name, item.Address));
            settings.AddressBook = items;
        }

        private static void LoadSettings()
        {
            var settings = Settings.Container;

            Entries.Clear();

            var items = settings.AddressBook;
            if (items != null)
            {
                foreach (var item in items)
                {
                    var parts = item.Split(new char[] { ';' }, StringSplitOptions.None);
                    if (parts.Length >= 2)
                    {
                        var name = string.Join(";", parts.Take(parts.Length - 1));
                        var address = parts.Last();
                        try
                        {
                            var entry = new AddressBookEntry(address, name);
                            Entries.Add(entry);
                        }
                        catch (ArgumentException) { }
                    }
                }
            }
            else
                Entries.Add(new AddressBookEntry() { Name = "Localhost", Address = "127.0.0.1:5150" });
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - 35;
            var col1 = 0.50;
            var col2 = 0.50;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
                var entry = listView.SelectedItem as AddressBookEntry;
                if (entry != null && Entries.Contains(entry))
                    Entries.Remove(entry);
        }

        private void MenuItem_Edit_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("Edit Not Implmented");
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
                Selected = item.Tag as AddressBookEntry;
            else
                Selected = null;
            Close();
        }

        private void Server_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateServerBox();
        }

        private bool ValidateServerBox()
        {
            string server = txtServer.Text;
            if (string.IsNullOrWhiteSpace(server))
            {
                ServerIsValid = null;
                return false;
            }

            if (AddressBookEntry.IsValidServerEntry(server))
            {
                ServerIsValid = true;
                return true;
            }

            ServerIsValid = false;
            return false;
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateServerBox())
                Entries.Add(new AddressBookEntry(txtServer.Text, txtName.Text));
        }


    }
}

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

        static AddressBookWindow()
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

        public AddressBookWindow(string currentServer, string currentPort)
            : this()
        {
            if (!string.IsNullOrWhiteSpace(currentServer) && !string.IsNullOrWhiteSpace(currentPort))
            {
                string toCheck = string.Format("{0}:{1}", currentServer, currentPort);
                if (AddressBookEntry.IsValidServerEntry(toCheck))
                {
                    var existing = FindMatchingEntry(toCheck);
                    if (existing != null)
                    {
                        txtName.Text = existing.Name;
                        txtServer.Text = existing.Address;
                    }
                    else
                        txtServer.Text = toCheck;

                    txtName.Focus();
                }
            }
        }

        private static void Settings_SaveSettings(object sender, EventArgs e)
        {
            // Format: Name;[password@]address[:port]

            var settings = Settings.Container;

            var items = new System.Collections.Specialized.StringCollection();
            foreach (var item in Entries)
            {
                string address = string.IsNullOrWhiteSpace(item.Password) ? item.Address : item.Password + "@" + item.Address;
                items.Add(string.Join(";", item.Name, address));
            }
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
                        string password = null;
                        if(address.Contains("@"))
                        {
                            var addrParts = address.Split('@');
                            password = addrParts[0];
                            address = addrParts[1];
                        }
                        try
                        {
                            var entry = new AddressBookEntry(address, name, password);
                            Entries.Add(entry);
                        }
                        catch (ArgumentException) { }
                    }
                }
            }
            else
                Entries.Add(new AddressBookEntry(IpAndPortString: "127.0.0.1:5150", name: "Localhost"));
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - 35;
            var col1 = 0.35;
            var col2 = 0.45;
            var col3 = 0.20;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            var entry = listView.SelectedItem as AddressBookEntry;
            if (entry != null && Entries.Contains(entry))
                Entries.Remove(entry);
        }

        private void MenuItem_Edit_Click(object sender, RoutedEventArgs e)
        {
            var entry = listView.SelectedItem as AddressBookEntry;
            if (entry != null && Entries.Contains(entry))
            {
                txtName.Text = entry.Name;
                txtServer.Text = entry.Address;
                txtName.Focus();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtName.SelectAll();
                }));
            }
        }

        private void MenuItem_SetPass_Click(object sender, RoutedEventArgs e)
        {
            var entry = listView.SelectedItem as AddressBookEntry;
            if (entry != null && Entries.Contains(entry))
            {
                string serverName = string.IsNullOrWhiteSpace(entry.Name) ? string.IsNullOrWhiteSpace(entry.Address) ? "[null]" : entry.Address.ToString() : entry.Name;
                string message = string.Format("Enter new pass for {0}", serverName);
                string pass = entry.Password;
                if (LoginPassWindow.Prompt(ref pass, message, "Apply"))
                {
                    entry.Password = string.IsNullOrWhiteSpace(pass) ? null : pass;
                    listView.Items.Refresh();
                }
            }
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

        private void txtServer_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))
                txtServer.Text = string.Empty;
            else
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtServer.SelectAll();
                }));
        }

        private void txtServer_LostFocus(object sender, RoutedEventArgs e)
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
            AddOrUpdateFromTextBoxes();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddOrUpdateFromTextBoxes();
        }

        private void AddOrUpdateFromTextBoxes()
        {
            if (ValidateServerBox())
            {
                string name = txtName.Text.Trim();
                string host = txtServer.Text.Trim();

                var entry = FindMatchingEntry(txtServer.Text);
                if (entry == null)
                {
                    entry = new AddressBookEntry(host, name);
                    Entries.Add(entry);
                }
                else
                    entry.Name = name;

                listView.Items.Refresh();
                listView.ScrollIntoView(entry);
                listView.SelectedItem = entry;
                txtName.Text = txtServer.Text = string.Empty;
            }
        }

        private void txtServer_TextChanged(object sender, TextChangedEventArgs e)
        {
            var existing = FindMatchingEntry(txtServer.Text);
            if (existing != null)
                btnAdd.Content = "Update";
            else
                btnAdd.Content = "Add";
        }

        private static AddressBookEntry FindMatchingEntry(string host)
        {
            var existing = Entries.Where(m => m.Address.Trim().Equals(host.Trim(), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            return existing;
        }

    }
}

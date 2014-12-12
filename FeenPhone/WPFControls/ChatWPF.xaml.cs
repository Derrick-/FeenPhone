using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeenPhone.WPFControls
{
    /// <summary>
    /// Interaction logic for ChatWPF.xaml
    /// </summary>
    public partial class ChatWPF : UserControl
    {
        public ChatWPF()
        {
            InitializeComponent();

            FeenPhone.Client.EventSink.OnChat += OnChat;
        }

        private void OnChat(object sender, Client.OnChatEventArgs e)
        {
            string from = e.User == null ? "Unknown" : e.User.Nickname;
            Dispatcher.BeginInvoke(new Action<string>(AddToLog), string.Format("{0}: {1}", from, e.Text));
        }

        void AddToLog(string text)
        {
            var maxVertOffset = LogScroller.ExtentHeight - LogScroller.ViewportHeight;
            bool wasAtBottom = LogScroller.VerticalOffset >= maxVertOffset;
            if (wasAtBottom)
                LogScroller.ScrollToBottom();
            if (log.Text != string.Empty)
                log.Text += '\n';
            log.Text += text;
        }

        private void ChatEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendText();
                e.Handled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendText();
        }

        void SendText()
        {
            if (!string.IsNullOrWhiteSpace(ChatEntry.Text))
            {
                string text = ChatEntry.Text.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (NetworkWPF.Client != null)
                        NetworkWPF.Client.SendChat(text);
                }

                ChatEntry.Text = string.Empty;
            }
        }

    }
}

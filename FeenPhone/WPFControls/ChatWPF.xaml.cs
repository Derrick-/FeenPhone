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
            Dispatcher.BeginInvoke(new Action<string>(AddToLog), string.Format("{0}: {1}", e.User.Nickname, e.Text));
        }

        void AddToLog(string text)
        {
            var maxVertOffset = LogScroller.ExtentHeight - LogScroller.ViewportHeight;
            bool wasAtBottom = LogScroller.VerticalOffset >= maxVertOffset;
            if (wasAtBottom)
                LogScroller.ScrollToBottom();
            log.Text += text;
        }

    }
}

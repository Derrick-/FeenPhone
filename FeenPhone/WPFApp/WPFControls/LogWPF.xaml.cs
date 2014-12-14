using System;
using System.Collections.Generic;
using System.IO;
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

namespace FeenPhone.WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for LogWPF.xaml
    /// </summary>
    public partial class LogWPF : UserControl
    {
        public LogWPF()
        {
            InitializeComponent();

            var console = new ConsoleWriter();
            console.OnText += console_OnText;
            Console.SetOut(console);

        }
        void console_OnText(object sender, ConsoleWriter.OnTextEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action<string>(AddToLog), e.Text);
        }

        void AddToLog(string text)
        {
            var maxVertOffset=LogScroller.ExtentHeight - LogScroller.ViewportHeight;
            bool wasAtBottom = LogScroller.VerticalOffset>=maxVertOffset;
            log.Text += text;
            if (wasAtBottom)
                LogScroller.ScrollToBottom();
        }

        class ConsoleWriter : TextWriter
        {

            public class OnTextEventArgs : EventArgs
            {
                public string Text { get; set; }
                public OnTextEventArgs(string text)
                {
                    Text = text;
                }
            }

            public event EventHandler<OnTextEventArgs> OnText;

            public ConsoleWriter()
            {
            }

            public override Encoding Encoding
            {
                get { return Console.OutputEncoding; }
            }

            public override void Write(char[] buffer, int index, int count)
            {
                if (OnText != null)
                {
                    string text = new String(buffer, index, count);
                    OnText(this, new OnTextEventArgs(text));
                }
            }

        }


    }
}

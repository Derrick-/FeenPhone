using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for AudioOutWPF.xaml
    /// </summary>
    public partial class AudioOutWPF : UserControl
    {
        [ImportMany(typeof(Audio.Codecs.INetworkChatCodec))]
        public IEnumerable<Audio.Codecs.INetworkChatCodec> Codecs { get; set; }

        public static DependencyProperty OutputFormatProperty = DependencyProperty.Register("OutputFormat", typeof(string), typeof(AudioOutWPF), new PropertyMetadata(null));
        public string OutputFormat
        {
            get { return (string)this.GetValue(OutputFormatProperty); }
            set
            {
                Dispatcher.BeginInvoke(new Action<DependencyProperty, object>(SetValue), OutputFormatProperty, value);
            }
        }

        public AudioOutWPF()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                new CompositionContainer(new AssemblyCatalog(Assembly.GetExecutingAssembly())).ComposeParts(this);

            InitializeComponent();
            DataContext = this;

            LoadSettings();
            Settings.SaveSettings += Settings_SaveSettings;

            FeenPhone.Client.EventSource.OnAudioData += EventSource_OnAudioData;
        }

        private void LoadSettings()
        {
            var settings = Settings.Container;
        }

        private void Settings_SaveSettings(object sender, EventArgs e)
        {
            var settings = Settings.Container;
        }

        void EventSource_OnAudioData(object sender, Client.AudioDataEventArgs e)
        {
            ReceivedAudio(e.Codec, e.Data);
        }

        private IWavePlayer waveOut;
        private BufferedWaveProvider waveProvider;
        static readonly TimeSpan MaxBufferedDuration = TimeSpan.FromMilliseconds(100);
        private void ReceivedAudio(Audio.Codecs.CodecID codecid, byte[] encoded)
        {

            Audio.Codecs.INetworkChatCodec remoteCodec = Codecs.SingleOrDefault(m => m.CodecID == codecid);

            if (waveOut != null && waveProvider.WaveFormat != remoteCodec.RecordFormat)
            {
                waveOut.Stop();
                waveOut = null;
            }

            if (waveOut == null)
            {
                waveOut = new DirectSoundOut(50);
                waveProvider = new BufferedWaveProvider(remoteCodec.RecordFormat);
                waveOut.Init(waveProvider);
                waveOut.Play();

                OutputFormat = remoteCodec.RecordFormat.ToString();
            }
            if (waveProvider.BufferedDuration <= MaxBufferedDuration)
            {
                byte[] decoded = remoteCodec.Decode(encoded, encoded.Length);
                waveProvider.AddSamples(decoded, 0, decoded.Length);
            }
            else
                Console.WriteLine("Skipping audio data to reduce latency ({0}ms)", waveProvider.BufferedDuration.TotalMilliseconds);
        }


    }
}

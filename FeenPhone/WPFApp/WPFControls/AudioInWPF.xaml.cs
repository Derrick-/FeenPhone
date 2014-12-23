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
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.ComponentModel;
using FeenPhone.Audio;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.ComponentModel.Composition;
using NAudio.CoreAudioApi;
using FeenPhone.WPFApp.Models;
using NAudio.Wave.SampleProviders;
using NAudio.Wave.Compression;
using System.Diagnostics;

namespace FeenPhone.WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for AudioWPF.xaml
    /// </summary>
    public partial class AudioInWPF : UserControl, INotifyPropertyChanged, IDisposable
    {
        static ObservableCollection<InputDeviceModel> InputList = new ObservableCollection<InputDeviceModel>();

        [ImportMany(typeof(Audio.Codecs.INetworkChatCodec))]
        public IEnumerable<Audio.Codecs.INetworkChatCodec> Codecs { get; set; }

        public AudioInWPF()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                new CompositionContainer(new AssemblyCatalog(Assembly.GetExecutingAssembly())).ComposeParts(this);

            InitializeComponent();
            DataContext = this;
            InitializeInputDevices();
            PopulateCodecsCombo(Codecs);

            LoadSettings();
            Settings.AppClosing += Settings_SaveSettings;

            AudioEvents.OnAudioDeviceAdded += AudioEvents_OnAudioDeviceAdded;
            AudioEvents.OnAudioDeviceRemoved += AudioEvents_OnAudioDeviceRemoved;
        }

        private void AudioEvents_OnAudioDeviceAdded(object sender, AudioEvents.MMDeviceAddedRemovedArgs e)
        {
            Console.WriteLine("Added audio device: " + e.deviceId);
        }

        private void AudioEvents_OnAudioDeviceRemoved(object sender, AudioEvents.MMDeviceAddedRemovedArgs e)
        {
            Console.WriteLine("Removed audio device: " + e.deviceId);
        }

        private void LoadSettings()
        {
            var settings = Settings.Container;

            string strCodec = settings.Codec;
            string strInputDevice = settings.InputDevice;

            if (!string.IsNullOrWhiteSpace(strCodec))
            {
                var selectCodecItem = comboBoxCodecs.Items.OfType<CodecComboItem>().Where(m => m.Text == strCodec).FirstOrDefault();
                if (selectCodecItem != null)
                {
                    comboBoxCodecs.SelectedItem = selectCodecItem;
                    this.codec = selectCodecItem.Codec;
                }
            }

            if (!string.IsNullOrWhiteSpace(strInputDevice))
            {
                var selectInputDevice = InputList.Where(m => m.ToString() == strInputDevice).FirstOrDefault();
                if (selectInputDevice != null)
                    SelectedInputSource = selectInputDevice;
            }
            else
            {
                SelectedInputSource = InputList.FirstOrDefault();
                if (comboInputGroups.Items.Count > 0)
                    comboInputGroups.SelectedIndex = 0;
            }

            BufferTargetMs = settings.InputLatency;
        }

        private void Settings_SaveSettings(object sender, EventArgs e)
        {
            var settings = Settings.Container;

            var selectedCodec = comboBoxCodecs.SelectedItem as CodecComboItem;
            if (selectedCodec != null)
                settings.Codec = selectedCodec.Text;
            else
                settings.Codec = null;

            var selectedMic = SelectedInputSource;
            if (selectedMic != null)
                settings.InputDevice = selectedMic.ToString();
            else
                settings.InputDevice = null;

            settings.InputLatency = BufferTargetMs;
        }

        private void InitializeInputDevices()
        {
            InputList.Clear();
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var capabilities = WaveIn.GetCapabilities(n);
                InputList.Add(new InputDeviceModel(n, capabilities));
            }

            var devices = MMDevices.deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            foreach (var device in devices)
            {
                InputList.Add(new InputDeviceModel(device));
            }

        }

        private void PopulateCodecsCombo(IEnumerable<Audio.Codecs.INetworkChatCodec> codecs)
        {
            if (codecs != null)
            {
                var sorted = from codec in codecs
                             where codec.IsAvailable
                             orderby codec.BitsPerSecond ascending
                             select codec;

                foreach (var codec in sorted)
                {
                    this.comboBoxCodecs.Items.Add(new CodecComboItem() { Text = codec.Name(), Codec = codec });
                }
            }
            else
                comboBoxCodecs.Items.Add("Codec List");
            this.comboBoxCodecs.SelectedIndex = 0;
        }


        private void UpdateMinBufferDurationForDevice(InputDeviceModel model)
        {
            int min = 50;
            if (model != null)
            {
                switch (model.Provider)
                {
                    case DeviceModel.DeviceProvider.Wasapi:
                        {
                            var mmdevice = model.MMDevice;
                            min = mmdevice.MinBufferDurationMs;
                            break;
                        }
                }
                BufferTargetMs = Math.Max(model.LastLatency.HasValue ? model.LastLatency.Value : 0, min);
            }
            else
                BufferTargetMs = min;

            BufferMinMs = min;
        }

        class CodecComboItem
        {
            public string Text { get; set; }
            public Audio.Codecs.INetworkChatCodec Codec { get; set; }
            public override string ToString()
            {
                return Text;
            }
        }

        public static DependencyProperty IsRecordingProperty = DependencyProperty.Register("IsRecording", typeof(bool?), typeof(AudioInWPF), new PropertyMetadata(false, OnIsRecordingChanged));
        public static DependencyProperty InputSourceListProperty = DependencyProperty.Register("InputSourceList", typeof(ObservableCollection<InputDeviceModel>), typeof(AudioInWPF), new PropertyMetadata(InputList));
        public static DependencyProperty SelectedInputSourceProperty = DependencyProperty.Register("SelectedInputSource", typeof(InputDeviceModel), typeof(AudioInWPF), new PropertyMetadata(null, OnSelectedInputSourceChanged));
        public static DependencyProperty SelectedInputSourceIndexProperty = DependencyProperty.Register("SelectedInputSourceIndex", typeof(int?), typeof(AudioInWPF), new PropertyMetadata(null));
        public static DependencyProperty ControlsEnabledProperty = DependencyProperty.Register("ControlsEnabled", typeof(bool), typeof(AudioInWPF), new PropertyMetadata(true));

        public static DependencyProperty BufferTargetMsProperty = DependencyProperty.Register("BufferTargetMs", typeof(int), typeof(AudioInWPF), new PropertyMetadata(50));
        public static DependencyProperty BufferMinMsProperty = DependencyProperty.Register("BufferMinMs", typeof(int), typeof(AudioInWPF), new PropertyMetadata(50));
        public static DependencyProperty BufferMaxMsProperty = DependencyProperty.Register("BufferMaxMs", typeof(int), typeof(AudioInWPF), new PropertyMetadata(100));

        public int BufferTargetMs
        {
            get { return (int)this.GetValue(BufferTargetMsProperty); }
            set { this.SetValue(BufferTargetMsProperty, value); }
        }
        public int BufferMinMs
        {
            get { return (int)this.GetValue(BufferMinMsProperty); }
            set { this.SetValue(BufferMinMsProperty, value); }
        }

        public int BufferMaxMs
        {
            get { return (int)this.GetValue(BufferMaxMsProperty); }
            set { this.SetValue(BufferMaxMsProperty, value); }
        }

        public bool ControlsEnabled
        {
            get { return (bool)this.GetValue(ControlsEnabledProperty); }
            set { this.SetValue(ControlsEnabledProperty, value); }
        }

        public bool? IsRecording
        {
            get { return (bool?)this.GetValue(IsRecordingProperty); }
            set
            {
                this.SetValue(IsRecordingProperty, value);
                OnPropertyChanged("IsRecording");
            }
        }

        public InputDeviceModel SelectedInputSource
        {
            get { return (InputDeviceModel)this.GetValue(SelectedInputSourceProperty); }
            set { this.SetValue(SelectedInputSourceProperty, value); }
        }

        public int? SelectedInputSourceIndex
        {
            get { return (int?)this.GetValue(SelectedInputSourceIndexProperty); }
            set { this.SetValue(SelectedInputSourceIndexProperty, value); }
        }

        private static void OnSelectedInputSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AudioInWPF target = d as AudioInWPF;
            if (target != null)
            {
                var oldModel = e.OldValue as InputDeviceModel;
                if (oldModel != null)
                {
                    oldModel.LastLatency = target.BufferTargetMs;
                }

                var newModel = e.NewValue as InputDeviceModel;
                target.UpdateMinBufferDurationForDevice(newModel);
            }
        }

        private static void OnIsRecordingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AudioInWPF target = d as AudioInWPF;
            if (target != null)
            {
                target.SetRecording((bool?)e.NewValue == true);
            }
        }

        private void SetRecording(bool enabled)
        {
            if (enabled)
                StartRecording();
            else
                StopRecording();
        }

        private IWaveIn waveIn;

        private Audio.Codecs.INetworkChatCodec codec;
        public Audio.Codecs.INetworkChatCodec Codec { get { return codec; } }
        private void StartRecording(bool shouldTryUseExclusive = true)
        {
            if (SelectedInputSource != null && SelectedInputSourceIndex.HasValue && SelectedInputSourceIndex.Value >= 0)
            {
                this.codec = ((CodecComboItem)comboBoxCodecs.SelectedItem).Codec;

                bool canUseExclusive = false;

                if (SelectedInputSource.Provider == DeviceModel.DeviceProvider.Wasapi)
                {
                    var mmdevice = SelectedInputSource.MMDevice;

                    WaveFormat deviceFormat = mmdevice.AudioClient.MixFormat;

                    if (mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Exclusive, codec.RecordFormat))
                    {
                        canUseExclusive = true;
                        deviceFormat = codec.RecordFormat;
                    }
                    else if (mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Shared, codec.RecordFormat))
                    {
                        canUseExclusive = false;
                        deviceFormat = codec.RecordFormat;
                    }
                    else if (deviceFormat.BitsPerSample != 16 || deviceFormat.Encoding != WaveFormatEncoding.Pcm)
                    {
                        WaveFormat altFormat = new WaveFormat(deviceFormat.SampleRate, 16, deviceFormat.Channels);

                        if (mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Exclusive, altFormat))
                            canUseExclusive = true;
                        else if (mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Shared, altFormat))
                            canUseExclusive = false;
                        else
                            throw new Exception("Device does not support 16bit PCM, or device is in use");

                        deviceFormat = altFormat;

                        Console.WriteLine("Initializing Wasapi\n  Device: {0}\n  Format: {1}\n  Mode: {2}\n  Resampling: {3}",
                            mmdevice.FriendlyName,
                            deviceFormat,
                            canUseExclusive ? "Exclusive" : "Shared",
                            deviceFormat.Equals(codec.RecordFormat) ? "NO" : "YES");

                    }
                    else
                    {
                        canUseExclusive = mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Exclusive, deviceFormat);
                    }

                    AudioClientShareMode shareMode;
                    if (canUseExclusive && shouldTryUseExclusive)
                        shareMode = AudioClientShareMode.Exclusive;
                    else
                        shareMode = AudioClientShareMode.Shared;

                    Guid audioSessionGuid = Guid.NewGuid();
                    try
                    {
                        mmdevice.AudioClient.Reset();
                    }
                    catch { }

                    BufferTargetMs = Math.Max(BufferTargetMs, mmdevice.MinBufferDurationMs);
                    var w = new WasapiCapture(mmdevice, BufferTargetMs);

                    waveIn = w;
                    waveIn.WaveFormat = deviceFormat;
                    w.ShareMode = shareMode;
                }
                else
                {
                    var w = new WaveIn();
                    w.BufferMilliseconds = BufferTargetMs;
                    w.DeviceNumber = SelectedInputSource.WavDeviceNumber;
                    waveIn = w;

                    waveIn.WaveFormat = codec.RecordFormat;
                    canUseExclusive = false;

                }

                waveIn.DataAvailable += waveIn_DataAvailable;
                try
                {
                    waveIn.StartRecording();
                    ControlsEnabled = false;
                }
                catch (NAudio.MmException ex)
                {
                    Console.WriteLine("Audio Error: Couldn't open recording device\n{0}", ex.Message);
                    waveIn = null;
                    IsRecording = false;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("Couldn't start recording: {0}", ex.Message);
                    IsRecording = false;
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Couldn't start recording: {0}", ex);
                    IsRecording = false;
                    return;
                }
            }
            else
                IsRecording = false;
        }

        private void StopRecording()
        {
            if (waveIn != null)
            {
                waveIn.DataAvailable -= waveIn_DataAvailable;
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
            }
            ControlsEnabled = true;
        }

        private void waveIn_DataAvailable(object sender, WaveInEventArgs waveInArgs)
        {
            Dispatcher.BeginInvoke(new Action<WaveInEventArgs>((args) =>
                {
                    if (IsRecording == true && !isDisposed)
                    {
                        byte[] toEncode = args.Buffer;

                        int length = args.BytesRecorded;
                        if (length > 0)
                        {
                            if (waveIn.WaveFormat != codec.RecordFormat)
                            {
                                toEncode = InputResampler.Resample(toEncode, args.BytesRecorded, waveIn.WaveFormat, codec.RecordFormat, out length);
                            }
                            if (toEncode == null)
                            {
                                Console.WriteLine("Encode Error: Disabling input. Please choose another record format and report this bug..");
                                StopRecording();
                            }
                            else
                            {
                                byte[] encoded = codec.Encode(toEncode, length);

                                if (NetworkWPF.Client != null)
                                    NetworkWPF.Client.SendAudio(codec.CodecID, encoded, encoded.Length);
                            }
                        }
                    }
                }), waveInArgs);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                Dispatcher.BeginInvoke(PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
            }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            StopRecording();
            foreach (var codec in Codecs)
                codec.Dispose();
            isDisposed = true;
        }

        private void comboInputs_Loaded(object sender, RoutedEventArgs e)
        {
            if (comboInputGroups.Items != null)
            {
                foreach (CollectionViewGroup item in comboInputGroups.Items)
                {
                    if (item.Items.Cast<InputDeviceModel>().Any(m => m == SelectedInputSource))
                    {
                        comboInputGroups.SelectedItem = item;
                        break;
                    }
                }
            }
        }
    }
}

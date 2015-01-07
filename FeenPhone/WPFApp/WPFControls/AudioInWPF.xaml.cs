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
using FeenPhone.Client;

namespace FeenPhone.WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for AudioWPF.xaml
    /// </summary>
    public partial class AudioInWPF : UserControl, INotifyPropertyChanged, IDisposable
    {
        static ObservableCollection<InputDeviceModel> InputList = new ObservableCollection<InputDeviceModel>();

        public AudioVisualizationSource VisSource { get; private set; }

        [ImportMany(typeof(Audio.Codecs.INetworkChatCodec))]
        public IEnumerable<Audio.Codecs.INetworkChatCodec> Codecs { get; set; }
        private IOrderedEnumerable<Audio.Codecs.INetworkChatCodec> CodecsAvailableSorted()
        {
            var sorted = from codec in Codecs
                         where codec.IsAvailable
                         orderby codec.BitsPerSecond ascending
                         orderby codec.SortOrder ascending
                         select codec;
            return sorted;
        }

        private readonly FeenPhone.Audio.SampleAggregator aggregator;

        public AudioInWPF()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                new CompositionContainer(new AssemblyCatalog(Assembly.GetExecutingAssembly())).ComposeParts(this);

            InitializeComponent();
            DataContext = this;
            InitializeInputDevices();
            PopulateCodecsCombo();

            LoadSettings();
            Settings.AppClosing += Settings_SaveSettings;

            AudioEvents.OnAudioDeviceAdded += AudioEvents_OnAudioDeviceAdded;
            AudioEvents.OnAudioDeviceRemoved += AudioEvents_OnAudioDeviceRemoved;

            EventSource.OnLoginStatus += EventSource_OnLoginStatus;
            EventSource.OnUserConnected += EventSource_InvokeOnUserConnected;

            aggregator = new FeenPhone.Audio.SampleAggregator() { PerformFFT = false };
            aggregator.NotificationCount = 882;
            aggregator.PerformFFT = true;

            VisSource = new AudioVisualizationSource(aggregator);
        }

        private bool isFirstConnect = true;
        private void EventSource_OnLoginStatus(object sender, LoginStatusEventArgs e)
        {
            bool loginAccepted = e.isLoggedIn;
            if (loginAccepted)
            {
                DispatchOnConnected();
            }
        }

        private void EventSource_InvokeOnUserConnected(object sender, OnUserEventArgs e)
        {
            DispatchOnConnected();
        }

        private void DispatchOnConnected()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (isFirstConnect)
                {
                    isFirstConnect = false;
                    IsRecording = true;
                }
            }));
        }

        private void AudioEvents_OnAudioDeviceAdded(object sender, AudioEvents.MMDeviceAddedRemovedArgs e)
        {
            Console.WriteLine("Added audio device: " + e.deviceId);
            var guid = MMDevices.ParseWasapiGuid(e.deviceId);
            if (guid != Guid.Empty)
            {
                Dispatcher.Invoke(new Action<Guid>(AddInput), guid);
            }
        }

        private void AudioEvents_OnAudioDeviceRemoved(object sender, AudioEvents.MMDeviceAddedRemovedArgs e)
        {
            Console.WriteLine("Removed audio device: " + e.deviceId);
            var guid = MMDevices.ParseWasapiGuid(e.deviceId);
            if (guid != Guid.Empty)
            {
                Dispatcher.Invoke(new Action<Guid>(RemoveInput), guid);
            }
        }

        private void AddInput(Guid guid)
        {
            var existing = InputList.ToList();
            if (!existing.Any(m => m.Guid == guid))
            {
                if (!IsRecording)
                    RefreshInputDevices(guid);
                else
                {
                    MMDevice found = MMDevices.FindDeviceByGuid(guid);

                    if (found != null)
                    {
                        var added = new InputDeviceModel(found);
                        InputList.Add(added);
                        if (SelectedInputSource == null)
                            SelectedInputSource = added;
                    }
                }

                SelectActiveInputGroup();
            }
        }

        private void RemoveInput(Guid guid)
        {
            if (SelectedInputSource != null && SelectedInputSource.Guid == guid)
                StopRecording();

            if (!IsRecording)
                RefreshInputDevices(guid);
            else
            {
                foreach (var existing in InputList.ToList())
                {
                    if (existing.Guid == guid)
                        InputList.Remove(existing);
                }
            }

            SelectActiveInputGroup();
        }

        private void RefreshInputDevices(Guid guid)
        {
            Guid selected = SelectedInputSource == null ? Guid.Empty : SelectedInputSource.Guid;
            InitializeInputDevices();
            if (selected == null || selected == Guid.Empty)
            {
                selected = guid;
            }

            SelectedInputSource = InputList.Where(m => m.Guid == guid).FirstOrDefault();
        }

        private void LoadSettings()
        {
            var settings = Settings.Container;

            string strCodec = settings.Codec;
            string strInputDevice = settings.InputDevice;

            var codecs = comboBoxCodecs.Items.OfType<CodecModel>();
            if (codecs.Any())
            {
                CodecModel selectCodecItem = null;
                if (!string.IsNullOrWhiteSpace(strCodec))
                {
                    selectCodecItem = codecs.Where(m => m.Text == strCodec).FirstOrDefault();
                }
                SelectedCodec = selectCodecItem ?? codecs.First();
                this.codec = SelectedCodec.Codec;
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

            UseWaveEvent = settings.UseWaveInEvent;
        }

        private void Settings_SaveSettings(object sender, EventArgs e)
        {
            var settings = Settings.Container;

            var selectedCodec = SelectedCodec;
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

            settings.UseWaveInEvent = UseWaveEvent;
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

        private void PopulateCodecsCombo()
        {
            if (Codecs != null)
            {
                var sorted = CodecsAvailableSorted();

                foreach (var codec in sorted)
                {
                    this.comboBoxCodecs.Items.Add(new CodecModel() { Text = codec.Name(), Codec = codec });
                }
            }
            else
                comboBoxCodecs.Items.Add("Codec List");
            this.comboBoxCodecs.SelectedIndex = 0;
        }

        int wasapiBufferPaddingMultiplier = 3;
        private void UpdateMinBufferDurationForDevice(InputDeviceModel model)
        {
            int min = 50;
            if (model != null)
            {
                switch (model.Provider)
                {
                    case DeviceProvider.Wasapi:
                        {
                            var mmdevice = model.MMDevice;
                            min = mmdevice.MinBufferDurationMs;
                            BufferTargetMs = min * wasapiBufferPaddingMultiplier;
                            break;
                        }
                    default:
                        {
                            BufferTargetMs = Math.Max(model.LastLatency.HasValue ? model.LastLatency.Value : 0, min);
                            break;
                        }
                }
            }
            else
                BufferTargetMs = min;

            BufferMinMs = min;
        }

        public static DependencyProperty ShowAdvancedControlsProperty = DependencyProperty.Register("ShowAdvancedControls", typeof(bool), typeof(AudioInWPF), new PropertyMetadata(true, OnAdvancedControlsChanged));
        internal bool ShowAdvancedControls { get { return (bool)GetValue(ShowAdvancedControlsProperty); } }
        private static void OnAdvancedControlsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AudioInWPF target = (AudioInWPF)d;
            if (((bool)e.NewValue) == false)
            {
                target.UseWaveEvent = false;
                if (target.comboInputGroups.Items != null && target.comboInputGroups.Items.Count > 0)
                    target.comboInputGroups.SelectedIndex = 0;
                if (target.comboBoxCodecs.Items != null && target.comboBoxCodecs.Items.Count > 0)
                    target.comboBoxCodecs.SelectedIndex = 0;
            }
        }

        public static DependencyProperty UseWaveEventProperty = DependencyProperty.Register("UseWaveEvent", typeof(bool), typeof(AudioInWPF), new PropertyMetadata(false, OnUseWaveEventChanged));
        public bool UseWaveEvent
        {
            get { return (bool)GetValue(UseWaveEventProperty); }
            private set { SetValue(UseWaveEventProperty, value); }
        }
        private static void OnUseWaveEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AudioInWPF)d).RestartRecording();
        }

        public static DependencyProperty LevelManagerProperty = DependencyProperty.Register("LevelManager", typeof(AudioLevelManager), typeof(AudioInWPF), new PropertyMetadata(null));

        public static DependencyProperty IsRecordingProperty = DependencyProperty.Register("IsRecording", typeof(bool), typeof(AudioInWPF), new PropertyMetadata(false, OnIsRecordingChanged));
        public static DependencyProperty InputSourceListProperty = DependencyProperty.Register("InputSourceList", typeof(ObservableCollection<InputDeviceModel>), typeof(AudioInWPF), new PropertyMetadata(InputList));
        public static DependencyProperty SelectedInputSourceProperty = DependencyProperty.Register("SelectedInputSource", typeof(InputDeviceModel), typeof(AudioInWPF), new PropertyMetadata(null, OnSelectedInputSourceChanged));
        public static DependencyProperty SelectedCodecProperty = DependencyProperty.Register("SelectedCodec", typeof(CodecModel), typeof(AudioInWPF), new PropertyMetadata(null));
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

        public bool IsRecording
        {
            get { return (bool)this.GetValue(IsRecordingProperty); }
            set
            {
                this.SetValue(IsRecordingProperty, value);
                OnPropertyChanged("IsRecording");
            }
        }

        public CodecModel SelectedCodec
        {
            get { return (CodecModel)this.GetValue(SelectedCodecProperty); }
            set { this.SetValue(SelectedCodecProperty, value); }
        }

        public InputDeviceModel SelectedInputSource
        {
            get { return (InputDeviceModel)this.GetValue(SelectedInputSourceProperty); }
            set { this.SetValue(SelectedInputSourceProperty, value); }
        }

        private static void OnSelectedInputSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AudioInWPF target = d as AudioInWPF;
            if (target != null && e.NewValue != null)
            {
                var oldModel = e.OldValue as InputDeviceModel;
                if (oldModel != null)
                {
                    oldModel.LastLatency = target.BufferTargetMs;
                }

                var newModel = e.NewValue as InputDeviceModel;
                target.UpdateMinBufferDurationForDevice(newModel);

                target.RestartRecording();
            }
        }

        private void comboBoxCodecs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RestartRecording();
        }

        volatile bool inBufferNeedsUpdate = false;
        private void inBuffer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsRecording)
                inBufferNeedsUpdate = true;
            else
                inBufferNeedsUpdate = false;
        }

        private void inBuffer_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (IsRecording && inBufferNeedsUpdate)
            {
                StopRecording();
                StartRecording();
            }

        }

        private void RestartRecording()
        {
            if (IsRecording)
            {
                StopRecording();
                StartRecording();
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
        private AudioLevelManager LevelManager
        {
            get { return (AudioLevelManager)this.GetValue(LevelManagerProperty); }
            set { this.SetValue(LevelManagerProperty, value); }
        }

        private Audio.Codecs.INetworkChatCodec codec;
        public Audio.Codecs.INetworkChatCodec Codec { get { return codec; } }
        private void StartRecording(bool shouldTryUseExclusive = true)
        {
            if (waveIn != null)
                StopRecording();
            if (SelectedInputSource != null)
            {
                this.codec = SelectedCodec.Codec;

                var deviceFormat = WaveFormat.CreateIeeeFloatWaveFormat(codec.RecordFormat.SampleRate, codec.RecordFormat.Channels);
                bool canUseExclusive = false;

                if (SelectedInputSource.Provider == DeviceProvider.Wasapi)
                {
                    var mmdevice = SelectedInputSource.MMDevice;

                    WaveFormatExtensible bestMatch;
                    canUseExclusive = mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Exclusive, deviceFormat, out bestMatch);
                    if (canUseExclusive && shouldTryUseExclusive)
                    {
                        if (bestMatch != null)
                            deviceFormat = bestMatch;
                    }
                    else
                    {
                        mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Shared, deviceFormat, out bestMatch);
                        if (bestMatch != null)
                            deviceFormat = bestMatch;
                    }


                    if (deviceFormat.Encoding != WaveFormatEncoding.IeeeFloat && deviceFormat.BitsPerSample != 16)
                    {
                        deviceFormat = mmdevice.AudioClient.MixFormat;

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
                        else
                        {
                            WaveFormat newFormat;
                            WaveFormat altWaveFormat = new WaveFormat(deviceFormat.SampleRate, 16, deviceFormat.Channels);
                            WaveFormat altFloatFormat = WaveFormat.CreateIeeeFloatWaveFormat(mmdevice.AudioClient.MixFormat.SampleRate, mmdevice.AudioClient.MixFormat.Channels);

                            if (mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Exclusive, altFloatFormat))
                            {
                                canUseExclusive = true;
                                newFormat = altFloatFormat;
                            }
                            else if (mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Exclusive, altWaveFormat))
                            {
                                canUseExclusive = true;
                                newFormat = altWaveFormat;
                            }
                            else if (mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Shared, altFloatFormat))
                            {
                                canUseExclusive = false;
                                newFormat = altFloatFormat;
                            }
                            else if (mmdevice.AudioClient.IsFormatSupported(AudioClientShareMode.Shared, altWaveFormat))
                            {
                                canUseExclusive = false;
                                newFormat = altWaveFormat;
                            }
                            else
                                throw new Exception("Device does not support 16bit PCM, or device is in use");

                            deviceFormat = newFormat;

                            Console.WriteLine("Initializing Wasapi\n  Device: {0}\n  Format: {1}\n  Mode: {2}\n  Resampling: {3}",
                                mmdevice.FriendlyName,
                                deviceFormat,
                                canUseExclusive ? "Exclusive" : "Shared",
                                deviceFormat.Equals(codec.RecordFormat) ? "NO" : "YES");
                        }
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
                    w.RecordingStopped += wasapi_RecordingStopped;
                    waveIn = w;
                    waveIn.WaveFormat = deviceFormat;
                    w.ShareMode = shareMode;

                    LevelManager = new AudioInLevelManager(w, mmdevice);
                }
                else
                {
                    Console.WriteLine("Initializing WaveIn{0}. Buffer:{1}ms Device:{2} Format:{3}", UseWaveEvent ? "Event" : "", BufferTargetMs, SelectedInputSource.WavDeviceNumber, deviceFormat);
                    if (UseWaveEvent)
                    {
                        var w = new WaveInEvent();
                        w.BufferMilliseconds = BufferTargetMs;
                        w.DeviceNumber = SelectedInputSource.WavDeviceNumber;
                        LevelManager = new AudioInLevelManager(w);
                        waveIn = w;
                    }
                    else
                    {
                        var w = new WaveIn();
                        w.BufferMilliseconds = BufferTargetMs;
                        w.DeviceNumber = SelectedInputSource.WavDeviceNumber;
                        LevelManager = new AudioInLevelManager(w);
                        waveIn = w;
                    }
                    waveIn.WaveFormat = deviceFormat;
                    canUseExclusive = false;
                }

                waveIn.DataAvailable += waveIn_DataAvailable;
                waveIn.RecordingStopped += waveIn_RecordingStopped;

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

        void waveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            VisSource.Reset();
        }

        void wasapi_RecordingStopped(object sender, StoppedEventArgs e)
        {
            Trace.WriteLine("Wasapi exception thrown: " + e.Exception);

            if (sender == waveIn)
                IsRecording = false;
        }

        private void StopRecording()
        {
            if (waveIn != null)
            {
                if (waveIn is WasapiCapture)
                    ((WasapiCapture)waveIn).RecordingStopped -= wasapi_RecordingStopped;

                waveIn.DataAvailable -= waveIn_DataAvailable;
                try
                {
                    waveIn.StopRecording();
                }
                catch (NAudio.MmException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                try
                {
                    Trace.WriteLine("Disposing waveIn");
                    waveIn.Dispose();
                    Trace.WriteLine("waveIn disposed");
                }
                catch
                {
                    Trace.WriteLine("waveIn NOT disposed");
                }
                waveIn = null;
            }
            if (LevelManager != null)
            {
                LevelManager.Dispose();
                LevelManager = null;
            }
            ControlsEnabled = true;
        }

        private void waveIn_DataAvailable(object sender, WaveInEventArgs waveInArgs)
        {
            Dispatcher.BeginInvoke(new Action<WaveInEventArgs>((args) => { HandleAudio(args); }), waveInArgs);
        }

        private void HandleAudio(WaveInEventArgs args)
        {
            if (IsRecording && !isDisposed)
            {
                byte[] toEncode = args.Buffer;

                int length = args.BytesRecorded;
                if (length > 0)
                {
                    if (waveIn.WaveFormat != codec.RecordFormat)
                    {
                        if (waveIn.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                        {
                            var floatSamples = InputResampler.ReadIeeeWav(toEncode, args.BytesRecorded, waveIn.WaveFormat);
                            foreach (var sample in floatSamples)
                                aggregator.Add(sample);
                            toEncode = InputResampler.Resample(floatSamples, floatSamples.Length, waveIn.WaveFormat, codec.RecordFormat, out length);
                        }
                        else
                        {
                            for (int i = 0; i < args.BytesRecorded + 1; i += 2)
                                aggregator.Add(InputResampler.PCMtoFloat(toEncode, i / 2));

                            toEncode = InputResampler.Resample(toEncode, args.BytesRecorded, waveIn.WaveFormat, codec.RecordFormat, out length);
                        }
                    }
                    if (toEncode == null)
                    {
                        Console.WriteLine("Encode Error: Disabling input. Please choose another record format and report this bug..");
                        StopRecording();
                    }
                    else
                    {
                        byte[] encoded = codec.Encode(toEncode, length);

                        if (encoded.Length > 0 && NetworkWPF.Client != null)
                            NetworkWPF.Client.SendAudio(codec.CodecID, encoded, encoded.Length);
                    }
                }
            }
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
            SelectActiveInputGroup();
        }

        private void SelectActiveInputGroup()
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

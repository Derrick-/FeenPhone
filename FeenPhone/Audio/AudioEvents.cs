using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.Audio
{
    static class AudioEvents
    {

        static AudioEvents()
        {
            RegisterMMNotificationClient();

            Settings.AppClosing += Settings_AppClosing;
        }

        static void Settings_AppClosing(object sender, EventArgs e)
        {
            UnRegisterMMNotificationClient();
        }

        private readonly static MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
        private readonly static NAudio.CoreAudioApi.Interfaces.IMMNotificationClient mmDevicecallback = new MMNotificationClient();

        private static void RegisterMMNotificationClient()
        {
            deviceEnum.RegisterEndpointNotificationCallback(mmDevicecallback);
        }

        private static void UnRegisterMMNotificationClient()
        {
            if (deviceEnum != null)
                deviceEnum.UnregisterEndpointNotificationCallback(mmDevicecallback);
        }

        public class ExceptionEventArgs
        {
            public Exception Exception { get; set; }
            public ExceptionEventArgs() { }
            public ExceptionEventArgs(Exception Exception) { this.Exception = Exception; }
        }

        public class MMDeviceAddedRemovedArgs
        {
            public string deviceId { get; set; }
            public MMDeviceAddedRemovedArgs() { }
            public MMDeviceAddedRemovedArgs(string pwstrDeviceId) { this.deviceId = deviceId; }
        }

        public static EventHandler<ExceptionEventArgs> OnAudioDeviceException;
        public static EventHandler<MMDeviceAddedRemovedArgs> OnAudioDeviceAdded;
        public static EventHandler<MMDeviceAddedRemovedArgs> OnAudioDeviceRemoved;

        public static void DeviceProbablyWentAway(object source, Exception ex)
        {
            Console.WriteLine("Audio Device probably went away. Try refreshing.");
            InvokeOnAudioDeviceException(source, ex);
        }

        public static void InvokeOnAudioDeviceException(object sender, Exception ex)
        {
            if (OnAudioDeviceException != null)
                OnAudioDeviceException(sender, new ExceptionEventArgs(ex));
        }

        internal static void InvokeDeviceAdded(object sender, string pwstrDeviceId)
        {
            if (OnAudioDeviceAdded != null)
                OnAudioDeviceAdded(sender, new MMDeviceAddedRemovedArgs(pwstrDeviceId));
        }

        internal static void InvokeDeviceRemoved(object sender, string deviceId)
        {
            if (OnAudioDeviceRemoved != null)
                OnAudioDeviceRemoved(sender, new MMDeviceAddedRemovedArgs(deviceId));
        }
    }
}

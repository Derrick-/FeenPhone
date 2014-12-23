using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.Audio
{
    static class AudioEvents
    {

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
            public MMDeviceAddedRemovedArgs(string pwstrDeviceId) { this.deviceId = pwstrDeviceId; }
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

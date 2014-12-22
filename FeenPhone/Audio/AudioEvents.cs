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

        public static EventHandler<ExceptionEventArgs> OnAudioDeviceException;

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

    }
}

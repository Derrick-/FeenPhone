using FeenPhone.Client;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FeenPhone.Utils
{
    static class Sounds
    {
        public static void PlayDisconnectSound()
        {
            PlaySound("WPFApp/Resources/audio/FeenPhoneDisconnectError.wav");
        }

        public static void PlaySound(string resourceString)
        {
            System.Windows.Resources.StreamResourceInfo sri = Application.GetResourceStream(new Uri(resourceString, UriKind.Relative));

                    using (WaveStream ws =
                       new BlockAlignReductionStream(
                           WaveFormatConversionStream.CreatePcmStream(
                               new WaveFileReader(sri.Stream))))
                    {
                        var length = ws.Length;
                        if (length < int.MaxValue)
                        {
                            byte[] data = new byte[length];
                            var format = ws.WaveFormat;
                            int read = ws.Read(data, 0, (int)length);
                            EventSource.InvokePlaySoundEffect(null, format, data);
                        }
                    }
        }

    }
}

using NAudio.Wave;
using NAudio.Wave.Compression;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.Audio
{
    static class InputResampler
    {
        static InputResampler()
        {
            Settings.AppClosing += Settings_AppClosing;
        }

        static bool EnableTraces = false;

        private const float IeeeFloatToPcmOffset = 32767;

        static AcmStream resampleChannelStream;
        static AcmStream resampleRateStream;
        static WaveFormat lastResampleSourceFormat;
        static WaveFormat lastResampleDestFormat;

        public static byte[] Resample(float[] ieeeSamples, int samples, WaveFormat sourceFormat, WaveFormat destPcmFormat, out int resultLength)
        {
            if (EnableTraces)
                Trace.WriteLine(string.Format("Resample {6}bytes {0} {1} {2} to {3} {4} {5}",
                    sourceFormat.Encoding, sourceFormat.BitsPerSample, sourceFormat.SampleRate,
                    destPcmFormat.Encoding, destPcmFormat.BitsPerSample, destPcmFormat.SampleRate, samples));

            int newLength;
            byte[] toResample = IeeeTo16Bit(ieeeSamples, samples, sourceFormat, out newLength);
            sourceFormat = new WaveFormat(sourceFormat.SampleRate, 16, sourceFormat.Channels);

            return ResamplePcm(ref toResample, ref newLength, sourceFormat, destPcmFormat, out resultLength);
        }

        public static byte[] Resample(byte[] toResample, int sourceLength, WaveFormat sourceFormat, WaveFormat destPcmFormat, out int resultLength)
        {
            if (EnableTraces)
                Trace.WriteLine(string.Format("Resample {6}bytes {0} {1} {2} to {3} {4} {5}",
                    sourceFormat.Encoding, sourceFormat.BitsPerSample, sourceFormat.SampleRate,
                    destPcmFormat.Encoding, destPcmFormat.BitsPerSample, destPcmFormat.SampleRate, sourceLength));

            if (sourceFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                toResample = IeeeTo16Bit(toResample, sourceFormat, out sourceLength);
                sourceFormat = new WaveFormat(sourceFormat.SampleRate, 16, sourceFormat.Channels);
            }

            return ResamplePcm(ref toResample, ref sourceLength, sourceFormat, destPcmFormat, out resultLength);
        }

        private static byte[] ResamplePcm(ref byte[] toResample, ref int sourceLength, WaveFormat sourceFormat, WaveFormat destPcmFormat, out int resultLength)
        {

            Debug.Assert(destPcmFormat.Encoding == WaveFormatEncoding.Pcm, "Codec format must be PCM");

            if (resampleRateStream != null && (!lastResampleSourceFormat.Equals(sourceFormat) || !lastResampleDestFormat.Equals(destPcmFormat)))
            {
                resampleRateStream.Dispose();
                resampleRateStream = null;
            }
            if (resampleRateStream == null)
            {
                WaveFormat sourceRateFormat = new WaveFormat(sourceFormat.SampleRate, sourceFormat.BitsPerSample, destPcmFormat.Channels);
                resampleRateStream = new AcmStream(sourceRateFormat, destPcmFormat);
                if (sourceFormat.Channels != destPcmFormat.Channels)
                {
                    WaveFormat destChanFormat = new WaveFormat(sourceFormat.SampleRate, sourceFormat.BitsPerSample, destPcmFormat.Channels);
                    if (resampleChannelStream != null)
                        resampleChannelStream.Dispose();
                    resampleChannelStream = new AcmStream(sourceFormat, destChanFormat);
                }
                lastResampleSourceFormat = sourceFormat;
                lastResampleDestFormat = destPcmFormat;
            }

            int bytesConverted;

            if (sourceFormat.Channels != destPcmFormat.Channels)
            {
                if (destPcmFormat.Channels == 1 && sourceFormat.Channels == 2)
                {
                    toResample = MixStereoToMono(toResample, sourceLength);
                    sourceLength = toResample.Length;
                }
                else
                {
                    Buffer.BlockCopy(toResample, 0, resampleChannelStream.SourceBuffer, 0, sourceLength);
                    sourceLength = resampleChannelStream.Convert(sourceLength, out bytesConverted);
                    if (bytesConverted >> 1 != sourceLength)
                    {
                        Console.WriteLine("WARNING: All input bytes were not converted.");
                    }
                    toResample = resampleChannelStream.DestBuffer;
                }
            }

            Buffer.BlockCopy(toResample, 0, resampleRateStream.SourceBuffer, 0, sourceLength);
            resultLength = resampleRateStream.Convert(sourceLength, out bytesConverted);
            if (bytesConverted != sourceLength)
            {
                Console.WriteLine("WARNING: All input bytes were not converted.");
                return null;
            }

            return resampleRateStream.DestBuffer;
        }

        private static byte[] StereoToLeftChannel(byte[] input, int length)
        {
            byte[] output = new byte[length / 2];
            int outputIndex = 0;
            for (int n = 0; n < length; n += 4)
            {
                output[outputIndex++] = (byte)(input[n] + input[n + 2]);
                output[outputIndex++] = (byte)(input[n + 1] + input[n + 3]);
            }
            return output;
        }

        private static byte[] MixStereoToMono(byte[] input, int length)
        {
            byte[] output = new byte[length / 2];
            int outputIndex = 0;
            for (int n = 0; n < length; n += 4)
            {
                int leftChannel = BitConverter.ToInt16(input, n);
                int rightChannel = BitConverter.ToInt16(input, n + 2);
                int mixed = (leftChannel + rightChannel); // / 2;
                byte[] outSample = BitConverter.GetBytes((short)mixed);

                output[outputIndex++] = outSample[0];
                output[outputIndex++] = outSample[1];
            }
            return output;
        }

        private static byte[] IeeeTo16Bit(byte[] toResample, WaveFormat sourceFormat, out int newLength)
        {
            float[] bufferF = ReadIeeeWav(toResample, toResample.Length, sourceFormat);
            return IeeeTo16Bit(bufferF, bufferF.Length, sourceFormat, out newLength);
        }

        private static byte[] IeeeTo16Bit(float[] bufferF, int samples, WaveFormat sourceFormat, out int newLength)
        {
            if (EnableTraces)
                Trace.WriteLine(string.Format("In {0} samples: {1} duration:{2}ms", sourceFormat, samples, samples / sourceFormat.Channels / (sourceFormat.SampleRate / 1000)));

            byte[] bufferB = new byte[samples * 2];

            WaveBuffer destWaveBuffer = new WaveBuffer(bufferB);
            int destOffset = 0;

            for (int n = 0; n < samples; n++)
            {
                float sample32 = bufferF[n];
                sample32 = ClipFloatSample(sample32);

                destWaveBuffer.ShortBuffer[destOffset++] = FloatToPCM(sample32);
            }

            newLength = samples * 2;

            if (EnableTraces)
                Trace.WriteLine(string.Format("Out PCM samples: {0} duration:{1}ms",
                newLength / (16 / 8),
                (newLength / (16 / 8)) / sourceFormat.Channels / (sourceFormat.SampleRate / 1000)));

            return bufferB;
        }

        public static short FloatToPCM(float sample32)
        {
            return (short)(sample32 * IeeeFloatToPcmOffset);
        }

        public static float PCMtoFloat(byte[] samples, int sampleIndex)
        {
            unsafe
            {
                fixed (byte* p = samples)
                {
                    short sample = *((short*)p + sampleIndex);
                    return PCMtoFloat(sample);
                }
            }
        }

        public static float PCMtoFloat(short sample32)
        {
            return (float)(sample32 / IeeeFloatToPcmOffset);
        }

        private static float ClipFloatSample(float sample32)
        {
            if (sample32 > 1.0f)
                sample32 = 1.0f;
            if (sample32 < -1.0f)
                sample32 = -1.0f;
            return sample32;
        }

        public static float[] ReadIeeeWav(byte[] toResample, int length, WaveFormat sourceFormat)
        {
            int bytesPerSample = (sourceFormat.BitsPerSample >> 3);

            if (bytesPerSample != 4)
                throw new InvalidOperationException(string.Format("{0}bit Ieee wav format not supported yet.", sourceFormat.BitsPerSample));

            int samples = length / bytesPerSample;

            float[] bufferF = new float[samples];

            // TODO: Use Buffer.BlockCopy??
            unsafe
            {
                fixed (byte* p = toResample)
                {
                    for (int i = 0; i < samples; i += 1)
                        bufferF[i] = *(float*)(p + i * bytesPerSample);
                }
            }
            return bufferF;
        }

        unsafe public static void ScalePCM16Volume(ref byte[] pcm16Samples, int length, double volumeScalar)
        {
            int count = length / 2;
            fixed (byte* p = pcm16Samples)
                for (int sampleIndex = 0; sampleIndex < count; sampleIndex++)
                    ApplyVolumeScalar(volumeScalar, p, sampleIndex);

        }

        internal enum RampDirection { ZeroToFull, FullToZero }
        unsafe internal static void RampPCM16Volume(ref byte[] pcm16Samples, int length, RampDirection direction)
        {
            int count = length / 2;
            fixed (byte* p = pcm16Samples)
                for (int sampleIndex = 1; sampleIndex < count; sampleIndex++)
                {
                    double volumeScalar;
                    volumeScalar = sampleIndex / (double)count;
                    if (direction == RampDirection.FullToZero)
                        volumeScalar = 1.0 - volumeScalar;
                    ApplyVolumeScalar(volumeScalar, p, sampleIndex);
                }
        }

        unsafe private static void ApplyVolumeScalar(double volumeScalar, byte* p, int sampleIndex)
        {
            short sample = *((short*)p + sampleIndex);
            double result = volumeScalar <= 0.0 ? 0 : sample * volumeScalar;
            (*((short*)p + sampleIndex)) = (short)result;
        }

        private static void Settings_AppClosing(object sender, EventArgs e)
        {
            Dispose();
        }

        internal static void Dispose()
        {
            if (resampleChannelStream != null)
            {
                resampleChannelStream.Dispose();
                resampleChannelStream = null;
            }
            if (resampleRateStream != null)
            {
                resampleRateStream.Dispose();
                resampleRateStream = null;
            }
        }
    }
}

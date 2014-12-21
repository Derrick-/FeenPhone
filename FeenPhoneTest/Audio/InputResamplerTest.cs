using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using FeenPhone.Audio;

namespace FeenPhoneTest.Audio
{
    [TestClass]
    public class InputResamplerTest
    {
        [TestCleanup]
        public void Cleanup()
        {
            InputResampler.Dispose();
        }

        [TestMethod]
        public void ChannelCountChangeTest()
        {
            int samples = 1000;
            byte[] left = CreateTestStream(samples, 0);
            byte[] right = CreateTestStream(samples, 1);
            byte[] stream = Multiplex(samples, left, right);

            WaveFormat sourceFormat = new WaveFormat(8000, 2);
            WaveFormat destFormat = new WaveFormat(8000, 1);

            int sourceLength = stream.Length;
            int expectedResultLen = sourceLength / 2;

            int resultLength;
            byte[] resultStream = InputResampler.Resample(stream, sourceLength, sourceFormat, destFormat, out resultLength);

            Assert.AreEqual(expectedResultLen, resultLength);

            for (int i = 0; i < samples; i++)
            {
                int leftValue = left[i * 2 + 1] << 8 | left[i * 2];
                int rightValue = right[i * 2 + 1] << 8 | right[i * 2];
                int resultValue = resultStream[i * 2 + 1] << 8 | resultStream[i * 2];

                if (leftValue + rightValue < short.MaxValue)
                    Assert.AreEqual(leftValue + rightValue, resultValue);
                else
                    Assert.Inconclusive("Clipping occurred.");
            }
        }

        [TestMethod]
        public void MonoRateResampleTest()
        {
            int samples = 1000;
            byte[] stream = CreateTestStream(samples, 0);

            WaveFormat sourceFormat = new WaveFormat(44100, 1);
            WaveFormat destFormat = new WaveFormat(8000, 1);

            int sourceLength = stream.Length;
            int expectedResultLen = GetExpectedConversionLength(sourceLength, sourceFormat, destFormat);

            int resultLength;
            byte[] resultStream = InputResampler.Resample(stream, sourceLength, sourceFormat, destFormat, out resultLength);

            Assert.AreEqual(expectedResultLen, resultLength);

        }

        [TestMethod]
        public void LongerSourceStereoResampleTest()
        {
            int samples = 500;
            byte[] testStream = CreateStereoSampleStream(samples);

            byte[] stream = new byte[testStream.Length + 1000];
            testStream.CopyTo(stream, 0);

            WaveFormat sourceFormat = new WaveFormat(48000, 2);
            WaveFormat destFormat = new WaveFormat(44100, 1);

            int sourceLength = testStream.Length;
            int expectedResultLen = GetExpectedConversionLength(sourceLength, sourceFormat, destFormat);

            int resultLength;
            byte[] resultStream = InputResampler.Resample(stream, sourceLength, sourceFormat, destFormat, out resultLength);

            Assert.AreEqual(expectedResultLen, resultLength);

        }


        [TestMethod]
        public void StereoRateResampleTest()
        {
            int samples = 1000;
            byte[] stream = CreateStereoSampleStream(samples);

            WaveFormat sourceFormat = new WaveFormat(8000, 2);
            WaveFormat destFormat = new WaveFormat(2000, 2);

            int sourceLength = stream.Length;
            int expectedResultLen = GetExpectedConversionLength(sourceLength, sourceFormat, destFormat);

            int resultLength;
            byte[] resultStream = InputResampler.Resample(stream, sourceLength, sourceFormat, destFormat, out resultLength);

            Assert.AreEqual(expectedResultLen, resultLength);

        }

        [TestMethod]
        public void StereoToMonoRateResampleTest()
        {
            int samples = 1000;
            byte[] stream = CreateStereoSampleStream(samples);

            WaveFormat sourceFormat = new WaveFormat(8000, 2);
            WaveFormat destFormat = new WaveFormat(2000, 1);

            int sourceLength = stream.Length;
            int expectedResultLen = GetExpectedConversionLength(sourceLength, sourceFormat, destFormat);

            int resultLength;
            byte[] resultStream = InputResampler.Resample(stream, sourceLength, sourceFormat, destFormat, out resultLength);

            Assert.AreEqual(expectedResultLen, resultLength);

        }

        private static int GetExpectedConversionLength(int sourceLength, WaveFormat sourceFormat, WaveFormat destFormat)
        {
            float channelFactor = (float)destFormat.Channels / (float)sourceFormat.Channels;
            int expectedResultLen = (int)((float)sourceLength * ((float)destFormat.SampleRate / (float)sourceFormat.SampleRate) * channelFactor);
            return expectedResultLen;
        }

        private static byte[] CreateStereoSampleStream(int samples)
        {
            byte[] left = CreateTestStream(samples, 0);
            byte[] right = CreateTestStream(samples, 1);
            byte[] stream = Multiplex(samples, left, right);
            return stream;
        }

        private static byte[] CreateTestStream(int samples, byte chanNum)
        {
            byte[] stream = new byte[samples * 2];

            for (int i = 0; i < samples; i++)
            {
                stream[i * 2] = (byte)(i * (chanNum + 1));
                stream[i * 2 + 1] = chanNum;
            }
            return stream;
        }

        private static byte[] Multiplex(int samples, byte[] left, byte[] right)
        {
            byte[] stream = new byte[samples * 4];
            for (int i = 0; i < samples; i++)
            {
                stream[i * 4] = left[i * 2];
                stream[i * 4 + 1] = left[i * 2 + 1];
                stream[i * 4 + 2] = right[i * 2];
                stream[i * 4 + 3] = right[i * 2 + 1];
            }
            return stream;
        }
    }
}

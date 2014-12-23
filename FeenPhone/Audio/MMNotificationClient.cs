using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeenPhone.Audio
{

    static class MMDevices
    {
        public readonly static MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
        private readonly static NAudio.CoreAudioApi.Interfaces.IMMNotificationClient mmDevicecallback = new MMNotificationClient();

        static MMDevices()
        {
            RegisterMMNotificationClient();

            Settings.AppClosing += Settings_AppClosing;
        }

        static void Settings_AppClosing(object sender, EventArgs e)
        {
            UnRegisterMMNotificationClient();
        }


        private static void RegisterMMNotificationClient()
        {
            deviceEnum.RegisterEndpointNotificationCallback(mmDevicecallback);
        }

        private static void UnRegisterMMNotificationClient()
        {
            if (deviceEnum != null)
                deviceEnum.UnregisterEndpointNotificationCallback(mmDevicecallback);
        }

        static Regex regexWasapiIdentifier = new Regex(@"^({(\d\.?)+}\.)?({[0-9a-f\-]+})$");
        public static System.Guid GetWasapiGuid(MMDevice device)
        {
            var matches = regexWasapiIdentifier.Matches(device.ID);
            if (matches.Count > 0)
            {
                Guid toReturn;
                var guidStr = matches[0].Groups[matches[0].Groups.Count - 1].Value;
                if (Guid.TryParse(guidStr, out toReturn))
                    return toReturn;
            }

            return Guid.Empty;
        }


    }

    class MMNotificationClient : IMMNotificationClient
    {
        public void OnDeviceStateChanged(string deviceId, NAudio.CoreAudioApi.DeviceState newState)
        {
            Trace.WriteLine("OnDeviceStateChanged:" + deviceId + " NewState: " + newState);
            if (newState.HasFlag(NAudio.CoreAudioApi.DeviceState.Active))
                AudioEvents.InvokeDeviceAdded(this, deviceId);
            else
                AudioEvents.InvokeDeviceRemoved(this, deviceId);
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            Trace.WriteLine("OnDeviceAdded:" + pwstrDeviceId);
            AudioEvents.InvokeDeviceAdded(this, pwstrDeviceId);
        }

        public void OnDeviceRemoved(string deviceId)
        {
            Trace.WriteLine("OnDeviceRemoved:" + deviceId);
            AudioEvents.InvokeDeviceRemoved(this, deviceId);
        }

        public void OnDefaultDeviceChanged(NAudio.CoreAudioApi.DataFlow flow, NAudio.CoreAudioApi.Role role, string defaultDeviceId)
        {
            Trace.WriteLine("OnDefaultDeviceChanged:" + defaultDeviceId + " Flow:" + flow + " Role:" + role);
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, NAudio.CoreAudioApi.PropertyKey key)
        {
            Trace.WriteLine("OnPropertyValueChanged:" + pwstrDeviceId + " key:" + key);
        }
    }
}

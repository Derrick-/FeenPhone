using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.Audio
{
    class MMNotificationClient : IMMNotificationClient
    {
        public void OnDeviceStateChanged(string deviceId, NAudio.CoreAudioApi.DeviceState newState)
        {
            Trace.WriteLine("OnDeviceStateChanged:" + deviceId + " NewState: " + newState);
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
            Trace.WriteLine("OnDefaultDeviceChanged:" + defaultDeviceId + " Flow:" + flow + " Role:" + role );
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, NAudio.CoreAudioApi.PropertyKey key)
        {
            Trace.WriteLine("OnPropertyValueChanged:" + pwstrDeviceId + " key:" + key);
        }
    }
}

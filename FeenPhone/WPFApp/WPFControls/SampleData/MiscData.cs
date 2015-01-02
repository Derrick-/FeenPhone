using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.WPFApp.Controls.SampleData
{
    public class UserAudioPlayerMock
    {
        public string OutputFormat { get; set; }
        public string CodecName { get; set; }
        public int BufferedDurationMs { get; set; }
        public int MaxBufferedDurationMs { get; set; }
        public int BufferTarget { get; set; }
        public int DroppedPackets { get; set; }
        
        public UserMock User { get; set; }
    }

    public class UserMock
    {
        public string Username { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone
{
    static class Timekeeper
    {
        private static System.Diagnostics.Stopwatch StopWatch = new System.Diagnostics.Stopwatch();
        static Timekeeper()
        {
            StopWatch.Start();
        }
        public static TimeSpan Elapsed { get { return StopWatch.Elapsed; } }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone
{
    class CodecFormatException : Exception
    {
        public CodecFormatException(Exception inner)
            : base("Unable to decode data stream", inner)
        {

        }
    }
}

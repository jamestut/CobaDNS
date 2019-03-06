using System;
using System.Collections.Generic;
using System.Text;

namespace CobaDNS.Message.ResourceData
{
    class Unknown : Base
    {
        byte[] data;

        UInt16 type;

        public ResourceRecord.RecordType Type { get { return (ResourceRecord.RecordType)type; } }

        public ushort Size { get { return (ushort)data.Length; } }

        public byte[] ToByteArray()
        {
            byte[] ret = new byte[data.Length];
            Array.Copy(data, ret, data.Length);
            return ret;
        }

        private Unknown() { }

        public static Unknown FromByteArray(UInt16 type, byte[] arr, int offset, int length)
        {
            Unknown ret = new Unknown();
            ret.type = type;
            ret.data = new byte[length];
            Array.Copy(arr, offset, ret.data, 0, length);
            return ret;
        }

        public string Stringify()
        {
            return String.Format("UnKnown | Type {0} | Size: {1}", type, Size);
        }
    }
}

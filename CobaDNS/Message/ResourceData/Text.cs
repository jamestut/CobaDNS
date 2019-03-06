using System;
using System.Collections.Generic;
using System.Text;

namespace CobaDNS.Message.ResourceData
{
    struct Text : Base
    {
        string data;

        public ResourceRecord.RecordType Type { get { return ResourceRecord.RecordType.TXT; } }

        public ushort Size { get { return (ushort)data.Length; } }

        public byte[] ToByteArray()
        {
            return Encoding.ASCII.GetBytes(data);
        }

        public static Text FromByteArray(byte[] arr, int offset, int length)
        {
            Text ret = new Text();
            ret.data = Encoding.ASCII.GetString(arr, offset, length);
            return ret;
        }

        public string Stringify()
        {
            return "TXT | " + data;
        }
    }
}

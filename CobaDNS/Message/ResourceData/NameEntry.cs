using System;
using System.Collections.Generic;
using System.Text;

namespace CobaDNS.Message.ResourceData
{
    class NameEntry : Base
    {
        private DnsName name;

        private ResourceRecord.RecordType type;

        public ResourceRecord.RecordType Type { get { return type; } }

        public ushort Size { get { return name.Size; } }

        public string Name { get { return name.Domain; } set { name.Domain = value; } }

        public NameEntry(ResourceRecord.RecordType type)
        {
            switch (type)
            {
                case ResourceRecord.RecordType.NS:
                case ResourceRecord.RecordType.CNAME:
                case ResourceRecord.RecordType.PTR:
                    this.type = type;
                    break;
                default:
                    throw new ArgumentException();
            }
            name = new DnsName();
        }

        public byte[] ToByteArray()
        {
            byte[] ret = new byte[name.Size];
            name.SerializeDomain(ret, 0);
            return ret;
        }

        public static NameEntry FromByteArray(ResourceRecord.RecordType type, byte[] arr, int payloadBase, int offset, out int size)
        {
            NameEntry ret = new NameEntry(type);
            ret.name = DnsName.FromByteArray(arr, payloadBase, offset, out size);
            return ret;
        }

        public string Stringify()
        {
            return Type.ToString() + " | " + Name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CobaDNS.Message.ResourceData
{
    class SOA : Base
    {
        private DnsName masterName = new DnsName();

        private DnsName responsibleName = new DnsName();

        [Endianness(EndiannessAttribute.Kind.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct Meta
        {
            public UInt32 serial;
            public UInt32 refresh;
            public UInt32 retry;
            public UInt32 expire;
            public UInt32 minimum;
        }

        private Meta meta;

        public ResourceRecord.RecordType Type { get { return ResourceRecord.RecordType.SOA; } }

        public ushort Size { get { return (ushort)(masterName.Size + responsibleName.Size + Marshal.SizeOf(typeof(Meta))); } }

        public String MasterName { get { return masterName.Domain; } set { masterName.Domain = value; } }

        public String ResponsibleName { get { return responsibleName.Email; } set { responsibleName.Domain = value; } }

        public UInt32 Serial { get { return meta.serial; } set { meta.serial = value; } }

        public UInt32 Refresh { get { return meta.refresh; } set { meta.refresh = value; } }

        public UInt32 Retry { get { return meta.retry; } set { meta.retry = value; } }

        public UInt32 Expire { get { return meta.expire; } set { meta.expire = value; } }

        public UInt32 Minimum { get { return meta.minimum; } set { meta.minimum = value; } }

        public byte[] ToByteArray()
        {
            int offset = 0;
            byte[] ret = new byte[Size];

            masterName.SerializeDomain(ret, offset);
            offset += masterName.Size;

            responsibleName.SerializeDomain(ret, offset);
            offset += responsibleName.Size;

            Serialization.Serialize(meta, ret, offset);

            return ret;
        }

        public static SOA FromByteArray(byte[] arr, int payloadBase, int offset, out int size)
        {
            int origOffset = offset;

            SOA ret = new SOA();
            int nameDataSize;

            ret.masterName = DnsName.FromByteArray(arr, payloadBase, offset, out nameDataSize);
            offset += nameDataSize;

            ret.responsibleName = DnsName.FromByteArray(arr, payloadBase, offset, out nameDataSize);
            offset += nameDataSize;

            ret.meta = Serialization.Deserialize<Meta>(arr, offset, Marshal.SizeOf<Meta>());
            offset += Marshal.SizeOf<Meta>();

            size = offset - origOffset;
            return ret;
        }

        public string Stringify()
        {
            return "SOA | " + MasterName + " | " + ResponsibleName;
        }
    }
}

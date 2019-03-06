using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CobaDNS.Message.ResourceData
{
    class MailExchange : Base
    {
        DnsName exchange = new DnsName();

        [Endianness(EndiannessAttribute.Kind.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        struct Meta
        {
            public UInt16 pref;
        }

        Meta meta;

        public ResourceRecord.RecordType Type { get { return ResourceRecord.RecordType.MX; } }

        public ushort Size { get { return (ushort)(exchange.Size + 2); } }

        public string ExchangeDomain { get { return exchange.Domain; } set { exchange.Domain = value; } }

        public UInt16 Preference { get { return meta.pref; } set { meta.pref = value; } }

        public byte[] ToByteArray()
        {
            byte[] ret = new byte[Size];
            Serialization.Serialize(meta, ret, 0);
            exchange.SerializeDomain(ret, 2);
            return ret;
        }

        public static MailExchange FromByteArray(byte[] arr, int payloadBase, int offset, out int size)
        {
            MailExchange ret = new MailExchange();
            ret.meta = Serialization.Deserialize<Meta>(arr, offset, 2);
            int nameDataSize;
            ret.exchange = DnsName.FromByteArray(arr, payloadBase, offset + 2, out nameDataSize);
            size = nameDataSize + 2;
            return ret;
        }

        public string Stringify()
        {
            return String.Format("MX | Pref {0} | {1}", Preference, ExchangeDomain);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CobaDNS.Message
{
    class Question
    {
        public enum QuestionType : UInt16
        {
            A = 1,
            NS = 2,
            CNAME = 5,
            SOA = 6,
            PTR = 12,
            MX = 15,
            TXT = 16,
            IncZoneTx = 251,
            AllZoneTx = 252,
            MailBox = 253,
            MailAgent = 254,
            All = 255,
        }

        [Endianness(EndiannessAttribute.Kind.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct Meta
        {
            public UInt16 qType;
            public UInt16 qClass;
        }

        private Meta meta;

        private DnsName name;

        public QuestionType QType { get { return (QuestionType)meta.qType; } set { meta.qType = (UInt16)value; } }

        public int Size { get { return name.Size + 4; } }

        public string Name { get { return name.Domain; } set { name.Domain = value; } }

        public Question()
        {
            meta.qClass = 1; //fixed to "IN"
            name = new DnsName();
        }

        public byte[] ToByteArray()
        {
            byte[] ret = new byte[Size];
            name.SerializeDomain(ret, 0);
            Serialization.Serialize(meta, ret, Size - 4);
            return ret;
        }

        public static Question FromByteArray(byte[] arr, int payloadBase, int offset, out int size)
        {
            Question ret = new Question();

            int nameDataSize;
            ret.name = DnsName.FromByteArray(arr, payloadBase, offset, out nameDataSize);
            offset += nameDataSize;

            ret.meta = Serialization.Deserialize<Meta>(arr, offset, 4);

            size = 4 + nameDataSize;
            return ret;
        }
    }
}
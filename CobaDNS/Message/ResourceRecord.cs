using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CobaDNS.Message
{
    class ResourceRecord
    {
        public enum RecordType : UInt16
        {
            A = 1,
            NS = 2,
            CNAME = 5,
            SOA = 6,
            PTR = 12,
            MX = 15,
            TXT = 16,
        }

        private DnsName name = new DnsName();

        [Message.Endianness(Message.EndiannessAttribute.Kind.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct Meta
        {
            public UInt16 rType;
            public UInt16 rClass;
            public UInt32 ttl;
            public UInt16 rdLength;
        }

        private Meta meta;

        private ResourceData.Base data;

        public string Name { get { return name.Domain; } set { name.Domain = value; } }

        public ResourceData.Base Data { get { return data; } set { data = value; } }

        public RecordType Type { get { return data.Type; } }

        public UInt32 TimeToLive { get { return meta.ttl; } set { meta.ttl = value; } }

        public UInt16 DataSize { get { return data.Size; } }

        public UInt16 Size { get { return (UInt16)(name.Size + Marshal.SizeOf(meta) + DataSize); } }

        public ResourceRecord()
        {
            //who else uses L3 other than IP?
            meta.rClass = 1;
        }

        private void UpdateMeta()
        {
            meta.rdLength = data.Size;
            meta.rType = (UInt16)data.Type;
        }

        public byte[] ToByteArray()
        {
            byte[] ret = new byte[Size];
            ToByteArray(ret, 0);
            return ret;
        }

        public void ToByteArray(byte[] data, int offset)
        {
            UpdateMeta();

            name.SerializeDomain(data, offset);
            offset += name.Size;

            Serialization.Serialize(meta, data, offset);
            offset += Marshal.SizeOf(meta);

            Array.Copy(this.data.ToByteArray(), 0, data, offset, this.data.Size);
        }

        public static ResourceRecord FromByteArray(byte[] data, int payloadBase, int offset, out int size)
        {
            int origOffset = offset;

            ResourceRecord rr = new ResourceRecord();

            int nameDataSize;
            rr.name = DnsName.FromByteArray(data, payloadBase, offset, out nameDataSize);
            offset += nameDataSize;

            rr.meta = Serialization.Deserialize<Meta>(data, offset, Marshal.SizeOf<Meta>());
            offset += Marshal.SizeOf<Meta>();

            //the use of pointer complicates things
            int recordDataReadSize = 0;
            switch((RecordType)rr.meta.rType)
            {
                case RecordType.A:
                    rr.data = ResourceData.Address.FromByteArray(data, offset);
                    recordDataReadSize = rr.data.Size;
                    break;
                case RecordType.NS:
                case RecordType.PTR:
                case RecordType.CNAME:
                    rr.data = ResourceData.NameEntry.FromByteArray((RecordType)rr.meta.rType, data, payloadBase, offset, out recordDataReadSize);
                    break;
                case RecordType.MX:
                    rr.data = ResourceData.MailExchange.FromByteArray(data, payloadBase, offset, out recordDataReadSize);
                    break;
                case RecordType.SOA:
                    rr.data = ResourceData.SOA.FromByteArray(data, payloadBase, offset, out recordDataReadSize);
                    break;
                case RecordType.TXT:
                    rr.data = ResourceData.Text.FromByteArray(data, offset, rr.meta.rdLength);
                    recordDataReadSize = rr.data.Size;
                    break;
                default:
                    rr.data = ResourceData.Unknown.FromByteArray((UInt16)rr.meta.rType, data, offset, rr.meta.rdLength);
                    recordDataReadSize = rr.data.Size;
                    break;
            }
            offset += recordDataReadSize;

            size = offset - origOffset;
            return rr;
        }

        public string Stringify()
        {
            return String.IsNullOrEmpty(Name) ? "(no name)" : Name;
        }
    }
}

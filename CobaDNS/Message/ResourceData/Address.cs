using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CobaDNS.Message.ResourceData
{
    class Address : Base
    {
        [Endianness(EndiannessAttribute.Kind.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct Meta
        {
            public UInt32 address;
        }

        private Meta meta;
        
        public ResourceRecord.RecordType Type { get { return ResourceRecord.RecordType.A; } }

        public ushort Size { get { return 4; } }

        public String Ipv4Address
        {
            get
            {
                return (meta.address >> 24).ToString() + "." + ((meta.address >> 16) & 0xFF).ToString() + "." + ((meta.address >> 8) & 0xFF).ToString() + "." + (meta.address & 0xFF).ToString();
            }

            set
            {
                var strOctets = value.Split('.');
                if(strOctets.Length != 4)
                {
                    throw new FormatException();
                }

                meta.address = 0;

                for(int i = 0; i < 4; ++i)
                {
                    meta.address |= uint.Parse(strOctets[i]) << ((3 - i) * 8);
                }
            }
        }

        public static Address FromByteArray(byte[] arr, int offset = 0)
        {
            Address ret = new Address();
            ret.meta = Serialization.Deserialize<Meta>(arr, offset, 4);
            return ret;
        }

        public byte[] ToByteArray()
        {
            return Serialization.Serialize(meta);
        }

        public string Stringify()
        {
            return "A | " + Ipv4Address;
        }
    }
}

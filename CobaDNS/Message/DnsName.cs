using System;
using System.Collections.Generic;
using System.Text;

namespace CobaDNS.Message
{
    class DnsName
    {
        private string[] domainComponents;
        private int len;

        public string Domain
        {
            get
            {
                return String.Join('.', domainComponents);
            }

            set
            {
                if (!value.EndsWith('.')) value += ".";
                if (value.Length > 63) throw new ArgumentOutOfRangeException();
                domainComponents = value.Split('.');
                CalculateSize();
            }
        }

        public string Email
        {
            get
            {
                StringBuilder sb = new StringBuilder(domainComponents[0]);
                if(domainComponents.Length >= 2) { sb.Append('@'); }
                for(int i=1; i<domainComponents.Length - 1; ++i)
                {
                    sb.Append(domainComponents[i]);
                    sb.Append('.');
                }
                return sb.ToString();
            }
        }

        public byte Size { get { return (byte)len; } }

        public DnsName()
        {
            domainComponents = new string[1] { string.Empty };
            CalculateSize();
        }

        private void CalculateSize()
        {
            int size = 0;
            foreach (var cmp in domainComponents)
            {
                size += cmp.Length + 1;
            }
            len = size;
        }

        public void SerializeDomain(byte[] data, int offset)
        {
            foreach (var cmp in domainComponents)
            {
                data[offset++] = (byte)cmp.Length;
                Array.Copy(Encoding.ASCII.GetBytes(cmp), 0, data, offset, cmp.Length);
                offset += cmp.Length;
            }
        }

        public static DnsName FromByteArray(byte[] arr, int payloadBase, int offset, out int size)
        {
            //these variables are used for size calculation
            int origIncOffset = offset;
            int origOffset = offset;
            bool pointerFollowed = false;

            DnsName ret = new DnsName();
            int cmpLen;
            List<String> components = new List<string>();
            
            while (true)
            {
                cmpLen = arr[offset++];
                if(cmpLen >= 192)
                {
                    //this is a pointer entry
                    int newOffset = ((cmpLen & 63) << 8) | arr[offset++];
                    if (!pointerFollowed)
                    {
                        origIncOffset = offset;
                        pointerFollowed = true;
                    }
                    offset = newOffset + payloadBase;
                } else
                {
                    components.Add(Encoding.ASCII.GetString(arr, offset, cmpLen));
                    offset += cmpLen;
                }
                if (cmpLen == 0) break;
            }
            ret.domainComponents = components.ToArray();

            ret.CalculateSize();

            size = (pointerFollowed ? origIncOffset : offset) - origOffset;
            return ret;
        }
    }
}

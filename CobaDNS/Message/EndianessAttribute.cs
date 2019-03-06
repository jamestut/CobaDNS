using System;
using System.Collections.Generic;
using System.Text;

namespace CobaDNS.Message
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class EndiannessAttribute : Attribute
    {
        public enum Kind
        {
            Big,
            Little
        }

        public EndiannessAttribute(Kind endianness)
        {
            this.Endian = endianness;
        }

        public Kind Endian { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CobaDNS.Message
{
    static class PackedByteExtensions
    {
        public static bool ToBool(this byte b)
        {
            return b != 0;
        }

        public static bool GetFlagAt(this byte b, byte offset)
        {
            if(offset > 7) { throw new ArgumentOutOfRangeException(); }
            return ((byte)(b & (0x80 >> offset))).ToBool();
        }

        public static void ClearFlagAt(ref this byte b, byte offset)
        {
            if (offset > 7) { throw new ArgumentOutOfRangeException(); }
            b = (byte)(b & (0xFF ^ (0x80 >> offset)));
        }

        public static void SetFlagAt(ref this byte b, byte offset)
        {
            if (offset > 7) { throw new ArgumentOutOfRangeException(); }
            b = (byte)(b | (0x80 >> offset));
        }

        public static void DefineFlagAt(ref this byte b, byte offset, bool val)
        {
            if (val) b.SetFlagAt(offset); else b.ClearFlagAt(offset);
        }

        public static byte GetPartValue(this byte b, byte offset, byte len)
        {
            if ((offset + len) > 8) { throw new ArgumentOutOfRangeException(); }
            return (byte)(len == 0 ? 0 : ((b >> (8 - offset - len)) & (0xFF >> len)));
        }

        public static void SetPartValue(ref this byte b, byte offset, byte len, byte val)
        {
            if ((offset + len) > 8) { throw new ArgumentOutOfRangeException(); }
            if(val > (0xFF >> (8 - len))) { throw new ArgumentOutOfRangeException(); }

            byte resetMask = (byte)~((byte)((byte)(0xFF << (8 - len)) >> offset));
            b = (byte)((b & resetMask) | (val << ((8 - offset) - len)));
        }
    }
}

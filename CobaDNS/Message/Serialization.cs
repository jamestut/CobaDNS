using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CobaDNS.Message
{
    class Serialization
    {
        public static bool IsTypeIntegral(Type type)
        {
            switch(Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        private static void ConvertEndian(Type type, byte[] data, int offset)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            EndiannessAttribute typeEndian = null;
            if (type.IsDefined(typeof(EndiannessAttribute), false))
            {
                typeEndian = (EndiannessAttribute)type.GetCustomAttributes(typeof(EndiannessAttribute), false)[0];
            }

            foreach (FieldInfo field in fields)
            {
                //we only accept integral type now
                if (IsTypeIntegral(field.FieldType))
                {
                    int fieldOffset = Marshal.OffsetOf(type, field.Name).ToInt32() + offset;
                    int length = Marshal.SizeOf(field.FieldType);

                    bool fieldEndianDefined = field.IsDefined(typeof(EndiannessAttribute), false);
                    if ((typeEndian == null && !fieldEndianDefined) || length <= 1)
                    {
                        continue;
                    }

                    EndiannessAttribute fieldEndian = fieldEndianDefined
                        ? (EndiannessAttribute)field.GetCustomAttributes(typeof(EndiannessAttribute), false)[0]
                        : typeEndian;

                    if(fieldEndian.Endian == EndiannessAttribute.Kind.Big && BitConverter.IsLittleEndian ||
                        fieldEndian.Endian == EndiannessAttribute.Kind.Little && !BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(data, fieldOffset, length);
                    }
                }
            }
        }

        public static T Deserialize<T>(byte[] data) where T : struct
        {
            return Deserialize<T>(data, 0, data.Length);
        }

        public static T Deserialize<T>(byte[] data, int offset, int length) where T : struct
        {
            byte[] buffer = new byte[length];
            Array.Copy(data, offset, buffer, 0, buffer.Length);
            ConvertEndian(typeof(T), buffer, 0);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public static byte[] Serialize<T>(T obj) where T : struct
        {
            byte[] data = new byte[Marshal.SizeOf(obj)];
            Serialize(obj, data, 0);
            return data;
        }

        public static void Serialize<T>(T obj, byte[] data, int offset)
        {
            if(data.Length - offset < Marshal.SizeOf(obj))
            {
                throw new IndexOutOfRangeException();
            }

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(obj, IntPtr.Add(handle.AddrOfPinnedObject(), (int)offset), false);
                ConvertEndian(typeof(T), data, offset);
            }
            finally
            {
                handle.Free();
            }
        }
    }
}

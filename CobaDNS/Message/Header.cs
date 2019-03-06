using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CobaDNS.Message
{
    [Message.Endianness(Message.EndiannessAttribute.Kind.Big)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header
    {
        private UInt16 id;

        //Q/R, OpCode (4), AA, TC, RD
        private byte flag0;
        //RA, reserved (3), RespCode (4)
        private byte flag1;

        //question section count
        private UInt16 qdCount;
        //answer section count
        private UInt16 anCount;
        //athority section count
        private UInt16 nsCount;
        //additional section count
        private UInt16 arCount;

        public enum DnsOpCodes : byte
        {
            Query = 0,
            InverseQuery = 1,
            Status = 2,
            Notify = 4,
            Update = 5, 
        }

        public enum DnsResponseCodes : byte
        {
            Success = 0,
            FormatError = 1,
            ServerFailure = 2,
            NameError = 3,
            NotSupported = 4,
            Refused = 5,
            YxDomain = 6,
            YxRrSet = 7,
            NxRrSet = 8,
            NotAuthoritative = 9,
            NotZone = 10,
        }

        public UInt16 Id { get { return id; } set { id = value; } }

        public UInt16 QuestionCount { get { return qdCount; } set { qdCount = value; } }

        public UInt16 AnswerCount { get { return anCount; } set { anCount = value; } }

        public UInt16 AuthorityCount { get { return nsCount; } set { nsCount = value; } }

        public UInt16 AdditionalRecordsCount { get { return arCount; } set { arCount = value; } }

        public bool Response { get { return flag0.GetFlagAt(0); } set { flag0.DefineFlagAt(0, value); } }

        public bool Query { get { return !Response; } set { Response = !value; } }

        public DnsOpCodes OpCode { get { return (DnsOpCodes)flag0.GetPartValue(1, 4); } set { flag0.SetPartValue(1, 4, (byte)value); } }

        public bool AuthoritativeAnswer { get { return flag0.GetFlagAt(5); } set { flag0.DefineFlagAt(5, value); } }

        public bool ResponseTruncated { get { return flag0.GetFlagAt(6); } set { flag0.DefineFlagAt(6, value); } }

        public bool RecursionDesired { get { return flag0.GetFlagAt(7); } set { flag0.DefineFlagAt(7, value); } }

        public bool RecursionAvailable { get { return flag1.GetFlagAt(0); } set { flag1.DefineFlagAt(0, value); } }

        public DnsResponseCodes ResponseCode { get { return (DnsResponseCodes)flag1.GetPartValue(4, 4); } set { flag1.SetPartValue(4, 4, (byte)value); } }

        public byte[] ToByteArray()
        {
            return Serialization.Serialize(this);
        }

        public static Header FromByteArray(byte[] arr, int offset = 0)
        {
            return Serialization.Deserialize<Header>(arr, offset, 12);
        }

        public string Stringify()
        {
            string qrStr = String.Format("{0}: {1}",
                Query ? "Query" : "Response",
                Query ? OpCode.ToString() : ResponseCode.ToString());

            List<String> flags = new List<string>();
            if (AuthoritativeAnswer) { flags.Add("AA"); }
            if (ResponseTruncated) { flags.Add("TR"); }
            if (RecursionDesired) { flags.Add("RD"); }
            if (RecursionAvailable) { flags.Add("RA"); }
            string flagStr = String.Join(',', flags);

            return String.Format("ID: {0} | {1} | {2}", Id, qrStr, flagStr);
        }
    }
}

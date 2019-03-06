using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CobaDNS.Message
{
    class Payload
    {
        private Header header;

        private List<Question> questions = new List<Question>();

        private List<ResourceRecord> answers = new List<ResourceRecord>();

        private List<ResourceRecord> authority = new List<ResourceRecord>();

        private List<ResourceRecord> additional = new List<ResourceRecord>();

        public Header Header { get { UpdateHeader(); return header; } set { header = value; } }

        public List<Question> Questions { get { return questions; } set { questions = value; } }

        public List<ResourceRecord> Answers { get { return answers; } set { answers = value; } }

        public List<ResourceRecord> Authority { get { return authority; } set { authority = value; } }

        public List<ResourceRecord> Additional { get { return additional; } set { additional = value; } }

        public int Size
        {
            get
            {
                int ret = Marshal.SizeOf<Header>();
                foreach(var q in questions)
                {
                    ret += q.Size;
                }
                foreach (var rrs in new []{answers, authority, additional}) {
                   foreach(var rr in rrs)
                    {
                        ret += rr.Size;
                    }
                }
                return ret;
            }
        }

        private void UpdateHeader()
        {
            header.QuestionCount = (ushort)(questions == null ? 0 : questions.Count);
            header.AnswerCount = (ushort)(answers == null ? 0 : answers.Count);
            header.AuthorityCount = (ushort)(authority == null ? 0 : authority.Count);
            header.AdditionalRecordsCount = (ushort)(additional == null ? 0 : additional.Count);
        }

        public byte[] ToByteArray()
        {
            byte[] ret = new byte[Size];
            ToByteArray(ret, 0);
            return ret;
        }

        public void ToByteArray(byte[] data, int offset)
        {
            UpdateHeader();
            Array.Copy(header.ToByteArray(), 0, data, offset, Marshal.SizeOf<Header>());
            offset += Marshal.SizeOf<Header>();
            
            //questions
            foreach(var q in questions)
            {
                Array.Copy(q.ToByteArray(), 0, data, offset, q.Size);
                offset += q.Size;
            }

            //other RR sections
            foreach(var rrs in new[] {answers, authority, additional})
            {
                foreach(var rr in rrs)
                {
                    rr.ToByteArray(data, offset);
                    offset += rr.Size;
                }
            }
        }

        public static Payload FromByteArray(byte[] data, int offset)
        {
            Payload ret = new Payload();
            int payloadBase = offset;

            ret.header = Serialization.Deserialize<Header>(data, offset, Marshal.SizeOf<Header>());
            offset += Marshal.SizeOf<Header>();

            int itemReadDataSize = 0;

            for(int i=0; i<ret.header.QuestionCount; ++i)
            {
                var q = Question.FromByteArray(data, payloadBase, offset, out itemReadDataSize);
                ret.Questions.Add(q);
                offset += itemReadDataSize;
            }

            foreach(var p in new [] { (ret.header.AnswerCount, ret.answers), (ret.header.AuthorityCount, ret.authority), (ret.header.AdditionalRecordsCount, ret.additional)})
            {
                for(int i=0; i<p.Item1; ++i)
                {
                    var rr = ResourceRecord.FromByteArray(data, payloadBase, offset, out itemReadDataSize);
                    p.Item2.Add(rr);
                    offset += itemReadDataSize;
                }
            }

            return ret;
        }
    }
}

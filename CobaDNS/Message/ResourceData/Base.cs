using System;
using System.Collections.Generic;
using System.Text;
using CobaDNS.Message;

namespace CobaDNS.Message.ResourceData
{
    interface Base
    {
        ResourceRecord.RecordType Type { get; }

        UInt16 Size { get; }

        byte[] ToByteArray();

        string Stringify();
    }
}

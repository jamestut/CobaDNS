using System;
using System.Net.Sockets;
using CobaDNS.Message;
using CobaDNS.Message.ResourceData;
using System.Runtime.InteropServices;
using System.Net;
using System.Collections.Generic;

namespace CobaDNS
{
    class Program
    {
        static void PrintUsage()
        {
            Console.WriteLine("Usage: CobaDNS [options] [server IPv4] [resource name]");
            Console.WriteLine("Options:");
            Console.WriteLine("-r         Ask name server to perform recursive lookup.");
            Console.WriteLine("-inv       Convert entered address for inverse PTR lookup.");
            Console.WriteLine("-t         Use TCP instead of UDP transport.");
            Console.WriteLine("-q [type]  Resource type to query. Default is 'A'. Valid values:");
            foreach(var name in Enum.GetNames(typeof(Message.Question.QuestionType)))
            {
                Console.Write("             ");
                Console.WriteLine(name);
            }
        }

        static void DoNothing() { }

        static void PrintPayload(Payload p)
        {
            Console.WriteLine(p.Header.Stringify());
            foreach(var kvp in new[] { ("Answer", p.Answers), ("Authority", p.Authority), ("Additional", p.Additional) })
            {
                Console.WriteLine("{0} section ({1} RRs)", kvp.Item1, kvp.Item2.Count);
                foreach(var rr in kvp.Item2)
                {
                    Console.Write(" - ");
                    Console.WriteLine(rr.Stringify());
                    Console.Write("   ");
                    Console.WriteLine(rr.Data.Stringify());
                }
            }
        }

        static void ProceedUdp(Payload p, IPEndPoint serverEp)
        {
            var reqData = p.ToByteArray();

            UdpClient client = new UdpClient();
            client.Connect(serverEp);
            client.Send(reqData, reqData.Length);
            var respData = client.Receive(ref serverEp);

            Payload pr = Payload.FromByteArray(respData, 0);
            PrintPayload(pr);
        }

        static void ProceedTcp(Payload p, IPEndPoint serverEp)
        {
            var reqData = p.ToByteArray();
            
            byte[] len = BitConverter.GetBytes((ushort)reqData.Length);
            if (BitConverter.IsLittleEndian) { Array.Reverse(len); }

            TcpClient client = new TcpClient();
            client.Connect(serverEp);
            var stream = client.GetStream();
            stream.Write(len);
            stream.Write(reqData);
            stream.Flush();

            stream.Read(len, 0, 2);
            if (BitConverter.IsLittleEndian) { Array.Reverse(len); }
            UInt16 replyLen = BitConverter.ToUInt16(len, 0);
            byte[] respData = new byte[replyLen];
            stream.Read(respData, 0, respData.Length);
            stream.Close();
            client.Close();

            Payload pr = Payload.FromByteArray(respData, 0);
            PrintPayload(pr);
        }

        static string ReverseIpv4(string addr)
        {
            var splt = addr.Split('.');
            if(splt.Length != 4) { return null; }
            string[] components = new string[4];
            for(int i=0; i<4; ++i)
            {
                byte icmp;
                if(!byte.TryParse(splt[i], out icmp)) { return null; }
                components[3 - i] = icmp.ToString();
            }
            return string.Join(".", components) + ".in-addr.arpa";
        }

        static void Main(string[] args)
        {
            var options = Support.ProgramOptions.Parse(args, 0);
            if(String.IsNullOrWhiteSpace(options.QueryName) || String.IsNullOrWhiteSpace(options.ServerIp))
            {
                PrintUsage();
                return;
            }

            //process
            Header hr = new Header();
            hr.Id = 4321;
            hr.OpCode = Header.DnsOpCodes.Query;
            hr.RecursionDesired = options.RecursionDesired;

            Question q = new Question();
            q.QType = options.QuestionType;
            if(options.InverseLookup)
            {
                string invAddr = ReverseIpv4(options.QueryName);
                if(invAddr == null) { Console.WriteLine("-inv option must be used with IPv4 address!"); return; }
                q.Name = invAddr;
                Console.WriteLine("Resolving for " + invAddr);
            }
            else { q.Name = options.QueryName; }

            Payload p = new Payload();
            p.Questions.Add(q);
            p.Header = hr;

            IPEndPoint serverEp;
            try
            {
                serverEp = new IPEndPoint(IPAddress.Parse(options.ServerIp), 53);
                if (options.UseTcp) ProceedTcp(p, serverEp); else ProceedUdp(p, serverEp);
            } catch(Exception ex)
            {
                Console.WriteLine("Please enter only valid IPv4 address for the DNS server.");
            }

            //set breakpoint here :)
            DoNothing();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using CobaDNS.Message;

namespace CobaDNS.Support
{
    struct ProgramOptions
    {
        public bool RecursionDesired { get; private set; }

        public bool InverseLookup { get; private set; }

        public Question.QuestionType QuestionType { get; private set; }

        public string QueryName { get; private set; }

        public string ServerIp { get; private set; }

        public bool UseTcp { get; private set; }

        public static ProgramOptions Parse(string[] args, int begin)
        {
            ProgramOptions ret = new ProgramOptions();
            ret.QuestionType = Question.QuestionType.A;
            for(int i=begin; i<args.Length; ++i)
            {
                if (args[i] == "-r") { ret.RecursionDesired = true; }
                else if (args[i] == "-t") { ret.UseTcp = true; }
                else if (args[i] == "-inv") { ret.InverseLookup = true; }
                else if (args[i] == "-q")
                {
                    ++i;
                    object qType;
                    if (!Enum.TryParse(typeof(Message.Question.QuestionType), args[i], out qType))
                    {
                        return ret;
                    }
                    ret.QuestionType = (Message.Question.QuestionType)qType;
                }
                else if(i == args.Length - 2)
                {
                    ret.ServerIp = args[i];
                }
                else if (i == args.Length - 1)
                {
                    ret.QueryName = args[i];
                }
            }
            return ret;
        }
    }
}

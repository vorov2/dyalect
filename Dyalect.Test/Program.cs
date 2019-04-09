using Dyalect.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var res = Exe.Eval(SourceBuffer.FromFile(FindFile("tests")));
            Console.WriteLine("Result: {0}", res);
        }

        static string ReadFile(string name)
        {
            return File.ReadAllText(FindFile(name));
        }

        static string FindFile(string name)
        {
            return Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "Tests", name + ".dy");
        }
    }
}

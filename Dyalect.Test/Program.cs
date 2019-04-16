using Dyalect.Parser;
using Dyalect.Runtime;
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
            try
            {
                var res = Exe.Eval(SourceBuffer.FromFile(FindFile("test")));
                Console.WriteLine("Result: {0}", res);
            }
            catch (DyCodeException ex)
            {
                Console.WriteLine(ex.ToString());
            }
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

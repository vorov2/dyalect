//using Dyalect.Compiler;
//using Dyalect.Linker;
//using Dyalect.Parser;
//using Dyalect.Runtime;
//using System;
//using System.IO;

//namespace Dyalect
//{
//    public static class TestRunner
//    {
//        private static string startupPath;

//        public static void Main()
//        {
//            startupPath = Path.Combine(Path.GetDirectoryName(typeof(Tests).Assembly.Location), "Tests");
//            typeof(Tests)

//            try
//            {
//                Run();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Tests failed: {0}", ex.Message);
//            }
//        }

//        private static void Run()
//        {
//            var file = FindFile("tests");

//            var linker = new DyLinker(FileLookup.Create(startupPath), BuilderOptions.Default);
//            var cres = linker.Make(SourceBuffer.FromFile(file));

//            if (!cres.Success)
//                throw new DyBuildException(cres.Messages);

//            var m = new DyMachine(cres.Value);
//            m.Execute();
//        }

//        private static string FindFile(string name)
//        {
//            return Path.Combine(startupPath, name + ".dy");
//        }
//    }
//}

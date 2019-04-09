using System;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Linker
{
    internal sealed class Lang : ForeignUnit
    {
        internal const string PrintName = "print";

        public Lang()
        {
            FileName = "\\lang";
            RegisterGlobal<DyObject, DyObject>(PrintName, Print);
        }

        public static DyObject Print(DyObject[] args)
        {
            foreach (var a in args)
                Console.Write(a.AsString());

            Console.WriteLine();
            return DyNil.Instance;
        }
    }
}

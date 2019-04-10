using Dyalect.Runtime.Types;
using System;

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

        public static DyObject CreateTuple(DyObject[] args)
        {
            return new DyTuple(new string[args.Length], args);
        }

        public static DyObject CreateRecord(DyObject[] args)
        {
            var len = args.Length / 2;
            var keys = new string[len];
            var values = new DyObject[len];

            for (var i = 0; i < args.Length; i++)
            {
                var key = args[0].TypeId == StandardType.Nil ? null : args[0].AsString();
                keys[i] = key;
                values[i] = args[1];
                i++;
            }

            return new DyTuple(keys, values);
        }
    }
}

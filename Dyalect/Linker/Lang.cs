using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Linker
{
    internal sealed class Lang : ForeignUnit
    {
        internal const string PrintName = "print";
        internal const string CreateTupleName = "createTuple";
        internal const string CreateArrayName = "createArray";

        public Lang()
        {
            FileName = "\\lang";
            RegisterGlobal<DyObject, DyObject>(PrintName, Print);
            RegisterGlobal<DyObject, DyObject>(CreateTupleName, CreateTuple);
            RegisterGlobal<DyObject, DyObject>(CreateArrayName, CreateArray);
        }

        public static DyObject Print(DyObject[] args)
        {
            foreach (var a in args)
                Console.Write(a.AsString());

            Console.WriteLine();
            return DyNil.Instance;
        }

        public static DyObject CreateArray(DyObject[] args) => new DyArray(args);

        public static DyObject CreateTuple(DyObject[] args)
        {
            var len = args.Length;
            var keys = new string[len];
            var values = new DyObject[len];

            for (var i = 0; i < args.Length; i++)
            {
                var v = args[i];

                if (v.TypeId == StandardType.Label)
                {
                    var label = (DyLabel)v;
                    keys[i] = label.Label;
                    values[i] = label.Value;
                }
                else
                    values[i] = v;
            }

            return new DyTuple(keys, values);
        }
    }
}

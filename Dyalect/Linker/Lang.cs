using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;

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
            RegisterGlobal(PrintName, Print);
            RegisterGlobal(CreateTupleName, CreateTuple);
            RegisterGlobal(CreateArrayName, CreateArray);
            RegisterGlobal("makeList", MakeList);
        }

        public static DyObject Print(ExecutionContext ctx, DyObject[] args)
        {
            foreach (var a in args)
            {
                if (a.TypeId == StandardType.String)
                    Console.Write(a.GetString());
                else
                    Console.Write(a.ToString(ctx));

                if (ctx.Error != null)
                    break;
            }

            Console.WriteLine();
            return DyNil.Instance;
        }

        private static DyObject MakeList(DyObject size)
        {
            var n = size.GetInteger();
            var lst = new List<DyObject>();

            while (n > 0)
                lst.Add(new DyInteger(n--));

            return new DyArray(lst.ToArray());
        }

        public static DyObject CreateArray(DyObject[] args) => new DyArray(args);

        public static DyObject CreateTuple(DyObject[] args)
        {
            var len = args.Length;

            if (len == 2)
            {
                var a1 = args[0];
                var a2 = args[1];
                DyLabel la1 = null, la2 = null;
                return DyTuple.Create(
                    a1.TypeId == StandardType.Label ? (la1 = (DyLabel)a1).Label : null,
                    la1 != null ? la1.Value : a1,
                    la2.TypeId == StandardType.Label ? (la2 = (DyLabel)a2).Label : null,
                    la2 != null ? la2.Value : a2
                    );
            }

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

            return DyTuple.Create(keys, values);
        }
    }
}

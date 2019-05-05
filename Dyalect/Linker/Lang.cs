using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dyalect.Linker
{
    internal sealed class Lang : ForeignUnit
    {
        public const string CreateTupleName = "createTuple";
        public const string CreateArrayName = "createArray";

        public Lang()
        {
            FileName = "lang";
        }

        [Function("convertToNumber")] 
        public DyObject ToNumber(ExecutionContext ctx, DyObject[] args)
        {
            if (args?.Length < 1)
                return Default();

            var arg = args[0];

            if (arg.TypeId == StandardType.Integer || arg.TypeId == StandardType.Float)
                return arg;
            else if (arg.TypeId == StandardType.String || arg.TypeId == StandardType.Char)
            {
                var str = arg.GetString();
                if (int.TryParse(str, out var i4))
                    return new DyInteger(i4);
                else if (double.TryParse(str, out var r8))
                    return new DyFloat(r8);
                
            }

            return DyInteger.Zero;
        }

        [Function("print")]
        public DyObject Print(ExecutionContext ctx, DyObject[] args)
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

        [Function("read")]
        public DyObject Read(ExecutionContext ctx, DyObject[] args)
        {
            return new DyString(Console.ReadLine());
        }

        [Function("makeArray")]
        public DyObject MakeArray(ExecutionContext ctx, DyObject[] args)
        {
            var size = args[0];
            var n = size.GetInteger();
            var lst = new List<DyObject>();

            while (n > 0)
                lst.Add(new DyInteger(n--));

            return new DyArray(lst);
        }

        [Function(CreateArrayName)]
        public DyObject CreateArray(ExecutionContext ctx, DyObject[] args) => new DyArray(args.ToList());

        [Function(CreateTupleName)]
        public DyObject CreateTuple(ExecutionContext ctx, DyObject[] args)
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
                    a2.TypeId == StandardType.Label ? (la2 = (DyLabel)a2).Label : null,
                    la2 != null ? la2.Value : a2
                    );
            }

            if (len == 3)
            {
                var a1 = args[0];
                var a2 = args[1];
                var a3 = args[2];
                DyLabel la1 = null, la2 = null, la3 = null;
                return DyTuple.Create(
                    a1.TypeId == StandardType.Label ? (la1 = (DyLabel)a1).Label : null,
                    la1 != null ? la1.Value : a1,
                    a2.TypeId == StandardType.Label ? (la2 = (DyLabel)a2).Label : null,
                    la2 != null ? la2.Value : a2,
                    a3.TypeId == StandardType.Label ? (la3 = (DyLabel)a3).Label : null,
                    la3 != null ? la3.Value : a3
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

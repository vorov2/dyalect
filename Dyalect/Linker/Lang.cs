using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dyalect.Linker
{
    internal sealed class Lang : ForeignUnit
    {
        public const string CreateArrayName = "createArray";

        public Lang()
        {
            FileName = "lang";
        }

        protected override void Initialize()
        {
            for (var i = 0; i < StandardType.TypeNames.Length; i++)
                Add(StandardType.TypeNames[i], DyNil.Instance);
        }

        public override void Execute(ExecutionContext ctx)
        {
            for (var i = 0; i < StandardType.TypeNames.Length; i++)
                Modify(i, ctx.Types[i]);
        }

        [Function("convertToNumber")] 
        public DyObject ToNumber(ExecutionContext ctx, DyObject value)
        {
            if (value.TypeId == StandardType.Integer || value.TypeId == StandardType.Float)
                return value;
            else if (value.TypeId == StandardType.String || value.TypeId == StandardType.Char)
            {
                var str = value.GetString();
                if (int.TryParse(str, out var i4))
                    return new DyInteger(i4);
                else if (double.TryParse(str, out var r8))
                    return new DyFloat(r8);
                
            }

            return DyInteger.Zero;
        }

        [Function("print")]
        public DyObject Print(ExecutionContext ctx, [VarArg]DyObject values, [Default(",")]DyObject separator, [Default("\n")]DyObject terminator)
        {
            foreach (var a in (DyTuple)values)
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
        public DyObject Read(ExecutionContext ctx)
        {
            return new DyString(Console.ReadLine());
        }

        [Function("makeArray")]
        public DyObject MakeArray(ExecutionContext ctx, DyObject size)
        {
            var n = size.GetInteger();
            var lst = new List<DyObject>();

            while (n > 0)
                lst.Add(new DyInteger(n--));

            return new DyArray(lst);
        }

        [Function("assert")]
        public DyObject Assert(ExecutionContext ctx, DyObject expected, DyObject got)
        {
            if (!Eq(expected.ToObject(), got.ToObject()))
                return Err.AssertFailed($"Expected {expected.ToString(ctx)}, got {got.ToString(ctx)}").Set(ctx);

            return DyNil.Instance;
        }

        private bool Eq(object x, object y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is IList xs && y is IList ys)
            {
                if (xs.Count != ys.Count)
                    return false;

                for (var i = 0; i < xs.Count; i++)
                    if (!Eq(xs[i], ys[i]))
                        return false;

                return true;
            }

            return Equals(x, y);
        }

        [Function(CreateArrayName)]
        public DyObject CreateArray(ExecutionContext ctx, [VarArg]DyObject items) => new DyArray(Enumerable.ToList((DyTuple)items));
    }
}

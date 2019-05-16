using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections;
using System.Linq;

namespace Dyalect.Linker
{
    internal sealed class Lang : ForeignUnit
    {
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
            var fst = true;

            foreach (var a in (DyTuple)values)
            {
                if (!fst)
                    Console.Write(separator.TypeId == StandardType.String ? separator.GetString() : separator.ToString(ctx).ToString());

                if (a.TypeId == StandardType.String)
                    Console.Write(a.GetString());
                else
                    Console.Write(a.ToString(ctx));

                fst = false;

                if (ctx.Error != null)
                    break;
            }

            Console.Write(terminator.TypeId == StandardType.String ? terminator.GetString() : terminator.ToString(ctx).ToString());
            return DyNil.Instance;
        }

        [Function("read")]
        public DyObject Read(ExecutionContext ctx)
        {
            return new DyString(Console.ReadLine());
        }

        [Function("assert")]
        public DyObject Assert(ExecutionContext ctx, DyObject expected, DyObject got)
        {
            if (!Eq(expected.ToObject(), got.ToObject()))
                return ctx.AssertFailed($"Expected {expected.ToString(ctx)}, got {got.ToString(ctx)}");

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

        [Function("parse")]
        public DyObject Parse(ExecutionContext ctx, DyObject expression)
        {
            if (expression.TypeId != StandardType.String)
                return ctx.InvalidType(StandardType.StringName, expression.TypeName(ctx));

            try
            {
                var p = new DyParser();
                var res = p.Parse(SourceBuffer.FromString(expression.GetString()));

                if (!res.Success)
                    return ctx.FailedReadLiteral(res.Messages.First().ToString());

                if (res.Value.Root == null || res.Value.Root.Nodes.Count == 0)
                    return ctx.FailedReadLiteral("Empty expression.");
                else if (res.Value.Root.Nodes.Count > 1)
                    return ctx.FailedReadLiteral("Only single expressions allowed.");

                return LiteralEvaluator.Eval(res.Value.Root.Nodes[0]);
            }
            catch (Exception ex)
            {
                return ctx.FailedReadLiteral(ex.Message);
            }
        }
    }
}

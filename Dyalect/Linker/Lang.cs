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
            //idle
        }

        public override void Execute(ExecutionContext ctx)
        {
            //idle
        }

        [Function("print")]
        public DyObject Print(ExecutionContext ctx, [VarArg]DyObject values, [Default(",")]DyObject separator, [Default("\n")]DyObject terminator)
        {
            var fst = true;

            foreach (var a in (DyTuple)values)
            {
                if (!fst)
                    Console.Write(separator.TypeId == DyType.String ? separator.GetString() : separator.ToString(ctx).ToString());

                if (a.TypeId == DyType.String)
                    Console.Write(a.GetString());
                else
                    Console.Write(a.ToString(ctx));

                fst = false;

                if (ctx.Error != null)
                    break;
            }

            Console.Write(terminator.TypeId == DyType.String ? terminator.GetString() : terminator.ToString(ctx).ToString());
            return DyNil.Instance;
        }

        [Function("read")]
        public DyObject Read(ExecutionContext ctx)
        {
            return new DyString(Console.ReadLine());
        }

        [Function("rnd")]
        public DyObject Randomize(ExecutionContext ctx, [Default(int.MaxValue)]DyObject max, [Default(0)]DyObject min, [Default]DyObject seed)
        {
            var iseed = 0;

            if (seed.TypeId != DyType.Nil)
                iseed = (int)seed.GetInteger();
            else
            {
                var dt = DateTime.Now;
                var dt2 = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
                iseed = (int)(dt2 - dt).Ticks;
            }

            var rnd = new Random(iseed);
            return DyInteger.Get(rnd.Next((int)min.GetInteger(), (int)max.GetInteger()));
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

        [Function("sqrt")]
        public DyObject Sqrt(ExecutionContext ctx, DyObject n)
        {
            if (n.TypeId != DyType.Float && n.TypeId != DyType.Integer)
                return ctx.InvalidType(DyTypeNames.Float, n);

            return new DyFloat(Math.Sqrt(n.GetFloat()));
        }

        [Function("parse")]
        public DyObject Parse(ExecutionContext ctx, DyObject expression)
        {
            if (expression.TypeId != DyType.String)
                return ctx.InvalidType(DyTypeNames.String, expression);

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

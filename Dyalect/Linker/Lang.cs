using Dyalect.Compiler;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Linker
{
    internal sealed class Lang : ForeignUnit
    {
        private readonly DyTuple? startupArguments;

        public Lang() : this(null) { }

        public Lang(DyTuple? args)
        {
            FileName = "lang";
            startupArguments = args;
        }

        protected override void Execute(ExecutionContext ctx) =>
            Add("args", startupArguments ?? (DyObject)DyNil.Instance);

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

                if (ctx.Error is not null)
                    break;
            }

            Console.Write(terminator.TypeId == DyType.String ? terminator.GetString() : terminator.ToString(ctx).ToString());
            return DyNil.Instance;
        }

        [Function("caller")]
        public DyObject Caller(ExecutionContext ctx)
        {
            if (ctx.CallStack.Count > 2)
            {
                var cp = ctx.CallStack[ctx.CallStack.Count - 2];
                if (!ReferenceEquals(cp, global::Dyalect.Runtime.Caller.External))
                    return cp.Function;
            }

            return DyNil.Instance;
        }

        [Function("current")]
        public DyObject Current(ExecutionContext ctx)
        {
            if (ctx.CallStack.Count > 1)
                return ctx.CallStack.Peek().Function;

            return DyNil.Instance;
        }

        [Function("readLine")]
        public DyObject Read(ExecutionContext _) => new DyString(Console.ReadLine() ?? "");

        [Function("rnd")]
        public DyObject Randomize(ExecutionContext _, [Default(int.MaxValue)]DyObject max, [Default(0)]DyObject min, [Default]DyObject seed)
        {
            int iseed;

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
            if (!Eq(expected?.ToObject(), got?.ToObject()))
                return ctx.AssertFailed($"Expected {expected?.ToString(ctx)}, got {got?.ToString(ctx)}");

            return DyNil.Instance;
        }

        private bool Eq(object? x, object? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is string a && y is string b)
            {
                a = a.Replace("\r\n", "\n");
                b = b.Replace("\r\n", "\n");
                return Equals(a, b);
            }

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
                return ctx.InvalidType(n);

            return new DyFloat(Math.Sqrt(n.GetFloat()));
        }

        [Function("min")]
        public DyObject Min(ExecutionContext ctx, DyObject x, DyObject y)
        {
            if (x.GetTypeInfo(ctx).Lt(ctx, x, y).GetBool())
                return x;
            else
                return y;
        }

        [Function("max")]
        public DyObject Max(ExecutionContext ctx, DyObject x, DyObject y)
        {
            if (x.GetTypeInfo(ctx).Gt(ctx, x, y).GetBool())
                return x;
            else
                return y;
        }

        [Function("abs")]
        public DyObject Abs(ExecutionContext ctx, DyObject value)
        {
            if (value.TypeId == DyType.Integer)
                return new DyInteger(Math.Abs(value.GetInteger()));
            else if (value.TypeId == DyType.Float)
                return new DyFloat(Math.Abs(value.GetFloat()));
            else
                return ctx.InvalidType(value);
        }

        [Function("round")]
        public DyObject Round(ExecutionContext ctx, DyObject number, [Default(2)]DyObject digits)
        {
            if (number.TypeId != DyType.Float)
                return ctx.InvalidType(number);
            else if (digits.TypeId != DyType.Integer)
                return ctx.InvalidType(digits);

            return new DyFloat(Math.Round(number.GetFloat(), (int)digits.GetInteger()));
        }

        [Function("sign")]
        public DyObject Sign(ExecutionContext ctx, DyObject x)
        {
            if (ReferenceEquals(x, DyInteger.Zero)) 
                return DyInteger.Zero;

            if (x.GetTypeInfo(ctx).Lt(ctx, x, DyInteger.Zero).GetBool())
                return DyInteger.MinusOne;
            
            return DyInteger.One;
        }

        [Function("parse")]
        public DyObject Parse(ExecutionContext ctx, DyObject expression)
        {
            if (expression.TypeId != DyType.String)
                return ctx.InvalidType(expression);

            try
            {
                var res = DyParser.Parse(SourceBuffer.FromString(expression.GetString()));

                if (!res.Success)
                    return ctx.FailedReadLiteral(res.Messages.First().ToString());

                if (res.Value!.Root is null || res.Value!.Root.Nodes.Count == 0)
                    return ctx.FailedReadLiteral("Empty expression.");
                else if (res.Value!.Root.Nodes.Count > 1)
                    return ctx.FailedReadLiteral("Only single expressions allowed.");

                return LiteralEvaluator.Eval(res.Value!.Root.Nodes[0]);
            }
            catch (Exception ex)
            {
                return ctx.FailedReadLiteral(ex.Message);
            }
        }

        [Function("eval")]
        public DyObject Eval(ExecutionContext ctx, DyObject source, DyObject args)
        {
            if (source is not DyString strObj)
                return ctx.InvalidType(source);

            var tup = args as DyTuple;
            var code = strObj.Value;

            var sb = new StringBuilder();
            sb.Append("func __x12(");

            if (tup is not null)
            {
                for (var i = 0; i < tup.Count; i++)
                {
                    var o = tup.Values[i];

                    if (o is not DyLabel lab)
                        continue;
                    
                    if (i > 0)
                        sb.Append(',');

                    sb.Append(lab.Label);
                }
            }

            sb.Append("){");
            sb.Append(code);
            sb.Append('}');
            sb.Append("__x12");

            var linker = new DyLinker(FileLookup.Default, BuilderOptions.Default());
            var result = linker.Make(SourceBuffer.FromString(sb.ToString()));

            if (!result.Success)
                throw new DyBuildException(result.Messages);

            var newctx = DyMachine.CreateExecutionContext(result.Value!);
            var result2 = DyMachine.Execute(newctx);
            var func = (DyFunction)result2.Value!;
            var argsList = new List<DyObject>();

            if (tup is null)
                return func!.Call(newctx, argsList.ToArray());

            for (var i = 0; i < tup.Count; i++)
            {
                var o = tup.Values[i];

                if (o is DyLabel lab)
                    argsList.Add(lab.Value);
            }

            return func!.Call(newctx, argsList.ToArray());
        }

        [Function("__invoke")]
        public DyObject Invoke(ExecutionContext ctx, DyObject func, [VarArg] DyObject values)
        {
            var fn = (DyFunction)func;
            var arr = ((DyTuple)values).Values;
            return fn.Call(ctx, arr);
        }

        [Function("__makeObject")]
        public DyObject MakeObject(ExecutionContext _, DyObject arg)
        {
            var dict = new Dictionary<string, DyObject>();

            if (arg is DyTuple tuple)
            {
                foreach (var obj in tuple.Values)
                {
                    var key = obj.GetLabel();

                    if (key != null)
                        dict[key] = obj.GetTaggedValue();
                }
            }

            return new DyWrapper(dict);
        }
    }
}

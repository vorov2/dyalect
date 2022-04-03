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
        private System.IO.TextWriter? consoleOutput;

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

            if (terminator is DyString str && !string.IsNullOrEmpty(str.Value))
                Console.Write(str.Value);
            else if (terminator is not DyNil)
                terminator.ToString(ctx).ToString();

            return DyNil.Instance;
        }

        [Function("setOut")]
        public DyObject SetOutput(ExecutionContext ctx, [Default]DyObject output)
        {
            if (output is DyNil)
            {
                if (consoleOutput is not null)
                    Console.SetOut(consoleOutput);
            }
            else if (output is not DyFunction fn)
                ctx.InvalidType(DyType.Function, output);
            else
            {
                if (consoleOutput is null)
                    consoleOutput = Console.Out;

                Console.SetOut(new ConsoleTextWriter(ctx, fn));
            }

            return DyNil.Instance;
        }

        [Function("rawget")]
        public DyObject RawGet(ExecutionContext ctx, DyObject values, DyObject index)
        {
            if (!index.IsInteger(ctx)) return Default();
            return ctx.RuntimeContext.Types[values.TypeId].GetDirect(ctx, values, index);
        }

        [Function("rawset")]
        public DyObject RawSet(ExecutionContext ctx, DyObject values, DyObject index, DyObject value)
        {
            if (!index.IsInteger(ctx)) return Default();
            ctx.RuntimeContext.Types[values.TypeId].SetDirect(ctx, values, index, value);
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
        public DyObject Read(ExecutionContext ctx) => new DyString(Console.ReadLine() ?? "");

        [Function("rnd")]
        public DyObject Randomize(ExecutionContext ctx, [Default(0)]DyObject min, [Default(int.MaxValue)]DyObject max, [Default]DyObject seed)
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

            var imin = (int)min.GetInteger();
            var imax = (int)max.GetInteger();

            if (imin > imax)
                return ctx.InvalidValue("min", "max");

            var rnd = new Random(iseed);
            return DyInteger.Get(rnd.Next(imin, imax));
        }

        [Function("assert")]
        public DyObject Assert(ExecutionContext ctx, [Default(true)]DyObject expect, DyObject got, [Default]DyObject errorText)
        {
            if (!Eq(ctx, expect?.ToObject(), got?.ToObject()))
            {
                if (errorText.TypeId == DyType.String)
                    return ctx.AssertionFailed(errorText.GetString());

                return ctx.AssertionFailed($"Expected {expect?.ToString(ctx)}, got {got?.ToString(ctx)}.");
            }

            return DyNil.Instance;
        }

        private bool Eq(ExecutionContext ctx, object? x, object? y)
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
                    if (!Eq(ctx, xs[i], ys[i]))
                        return false;

                return true;
            }

            if (x is DyObject xa && y is DyObject ba)
                return xa.Equals(ba, ctx);

            return Equals(x, y);
        }

        [Function("sqrt")]
        public DyObject Sqrt(ExecutionContext ctx, DyObject x)
        {
            if (x.TypeId != DyType.Float && x.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Float, DyType.Integer, x);

            return new DyFloat(Math.Sqrt(x.GetFloat()));
        }

        [Function("pow")]
        public DyObject Pow(ExecutionContext ctx, DyObject x, DyObject y)
        {
            if (x.TypeId != DyType.Float && x.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Float, DyType.Integer, x);

            if (y.TypeId != DyType.Float && y.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Float, DyType.Integer, y);

            return new DyFloat(Math.Pow(x.GetFloat(), y.GetFloat()));
        }

        [Function("min")]
        public DyObject Min(ExecutionContext ctx, DyObject x, DyObject y)
        {
            if (x.Lesser(y, ctx))
                return x;
            else
                return y;
        }

        [Function("max")]
        public DyObject Max(ExecutionContext ctx, DyObject x, DyObject y)
        {
            if (x.Greater(y, ctx))
                return x;
            else
                return y;
        }

        [Function("abs")]
        public DyObject Abs(ExecutionContext ctx, DyObject value)
        {
            if (value.Lesser(DyInteger.Zero, ctx))
                return value.Negate(ctx);

            return value;
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

            if (x.Lesser(DyInteger.Zero, ctx))
                return DyInteger.MinusOne;
            
            return DyInteger.One;
        }

        [Function("parse")]
        public DyObject Parse(ExecutionContext ctx, DyObject expression)
        {
            if (!expression.IsString(ctx)) return Default();

            try
            {
                var res = DyParser.Parse(SourceBuffer.FromString(expression.GetString()));

                if (!res.Success)
                    return ctx.ParsingFailed(res.Messages.First().ToString());

                if (res.Value!.Root is null || res.Value!.Root.Nodes.Count == 0)
                    return ctx.ParsingFailed("Empty expression.");
                else if (res.Value!.Root.Nodes.Count > 1)
                    return ctx.ParsingFailed("Only single expressions allowed.");

                return LiteralEvaluator.Eval(res.Value!.Root.Nodes[0]);
            }
            catch (Exception ex)
            {
                return ctx.ParsingFailed(ex.Message);
            }
        }

        [Function("eval")]
        public DyObject Eval(ExecutionContext ctx, DyObject source, DyObject args)
        {
            if (!source.IsString(ctx)) return Default();

            var tup = args as DyTuple;
            var sb = new StringBuilder();
            sb.Append("func __x12(");
            
            if (tup is not null)
            {
                var tv = tup.UnsafeAccessValues();
                
                for (var i = 0; i < tup.Count; i++)
                {
                    var o = tv[i];

                    if (o is not DyLabel lab)
                        continue;
                    
                    if (i > 0)
                        sb.Append(',');

                    sb.Append(lab.Label);
                }
            }

            sb.Append("){");
            sb.Append(source.GetString());
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
            
            var tvv = tup.UnsafeAccessValues();

            for (var i = 0; i < tup.Count; i++)
            {
                var o = tvv[i];

                if (o is DyLabel lab)
                    argsList.Add(lab.Value);
            }

            return func!.Call(newctx, argsList.ToArray());
        }

        [Function("__invoke")]
        public DyObject Invoke(ExecutionContext ctx, DyObject func, [VarArg] DyObject values)
        {
            var fn = (DyFunction)func;
            var arr = ((DyTuple)values).GetValues();
            return fn.Call(ctx, arr);
        }
    }
}

using Dyalect.Codegen;
using Dyalect.Compiler;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Dyalect.Linker;

[GeneratedModule]
internal sealed partial class Lang : ForeignUnit
{
    private static readonly char[] invalidChars = new[] { ' ', '\t', '\n', '\r', '\'', '"' };

    private readonly DyTuple? startupArguments;
    private const string VAR_CONSOLEOUTPUT = "sys.ConsoleOutput";

    public Lang() : this(null) { }

    public Lang(DyTuple? args)
    {
        FileName = "lang";
        startupArguments = args;
    }

    protected override void Execute(ExecutionContext ctx) => Add("args", startupArguments ?? Nil);

    private static void Process(StringBuilder sb, IEnumerable<DyObject> seq)
    {
        var c = 0;

        foreach (DyObject o in seq)
        {
            if (c > 0)
                sb.Append(", ");

            Process(sb, o);
            c++;
        }
    }

    private static void Process(StringBuilder sb, DyLabel lab)
    {
        if (lab.Mutable)
            sb.Append("var ");

        foreach (var ta in lab.EnumerateAnnotations())
        {
            sb.Append(ta.ToString());
            sb.Append(' ');
        }

        if (lab.Label.IndexOfAny(invalidChars) != -1)
            sb.Append(StringUtil.Escape(lab.Label));
        else
            sb.Append(lab.Label);

        sb.Append(':');
        sb.Append(' ');
        Process(sb, lab.Value);
    }

    private static void Process(StringBuilder sb, DyObject obj)
    {
        switch (obj.TypeId)
        {
            case Dy.Array:
                sb.Append('[');
                Process(sb, (IEnumerable<DyObject>)obj);
                sb.Append(']');
                break;
            case Dy.Tuple:
                sb.Append('(');
                Process(sb, (IEnumerable<DyObject>)obj);
                if (((DyTuple)obj).Count == 1)
                    sb.Append(',');
                sb.Append(')');
                break;
            case Dy.Dictionary:
                {
                    sb.Append('[');
                    var c = 0;

                    foreach (var (k, v) in ((DyDictionary)obj).Dictionary)
                    {
                        if (c > 0)
                            sb.Append(',');

                        Process(sb, k);
                        sb.Append(": ");
                        Process(sb, v);
                        c++;
                    }

                    sb.Append(']');
                }
                break;
            case Dy.Set:
                sb.Append("Set(");
                Process(sb, (IEnumerable<DyObject>)obj);
                sb.Append(')');
                break;
            case Dy.Char:
                sb.Append(StringUtil.Escape(obj.ToString(), "'"));
                break;
            case Dy.String:
                sb.Append(StringUtil.Escape(obj.ToString()));
                break;
            case Dy.Label:
                Process(sb, (DyLabel)obj);
                break;
            case Dy.Variant:
                var dyv = (DyVariant)obj;
                sb.Append('@');
                sb.Append(dyv.Constructor);
                if (dyv.Fields.Count > 0)
                {
                    sb.Append('(');
                    Process(sb, (IEnumerable<DyObject>)dyv.Fields);
                    sb.Append(')');
                }
                break;
            default:
                if (obj is DyClass cls)
                {
                    sb.Append(cls.Constructor);
                    sb.Append('(');
                    Process(sb, (IEnumerable<DyObject>)cls.Fields);
                    sb.Append(')');
                }
                else
                    sb.Append(obj);
                break;
        }
    }

    [StaticMethod("toLiteral")]
    public static string ToLiteral(DyObject value)
    {
        var sb = new StringBuilder();
        Process(sb, value);
        return sb.ToString();
    }

    [StaticMethod("referenceEquals")]
    public static bool Equals(DyObject value, DyObject other) => ReferenceEquals(value, other);

    [StaticMethod("print")]
    public static void Print(ExecutionContext ctx, [VarArg] DyTuple values, [Default(",")]string separator, [Default("\n")]DyObject terminator)
    {
        var fst = true;
        
        foreach (var a in values)
        {
            if (!fst && !string.IsNullOrEmpty(separator))
                Console.Write(separator);

            if (a is DyString s)
                Console.Write(s.Value);
            else
                Console.Write(a.ToString(ctx));

            fst = false;

            if (ctx.Error is not null)
                break;
        }

        if (terminator.TypeId is Dy.String or Dy.Char)
            Console.Write(terminator.ToString());
        else if (terminator.TypeId is not Dy.Nil)
            throw new DyCodeException(DyError.InvalidType, terminator);
    }

    [StaticMethod("setOut")]
    public static void SetOutput(ExecutionContext ctx, DyObject? output = null)
    {
        if (output is null)
        {
            var outputWriter = ctx.GetContextVariable<TextWriter>(VAR_CONSOLEOUTPUT);
            if (outputWriter is not null)
                Console.SetOut(outputWriter);
        }
        else
        {
            if (!ctx.HasContextVariable(VAR_CONSOLEOUTPUT))
                ctx.SetContextVariable(VAR_CONSOLEOUTPUT, Console.Out);

            Console.SetOut(new ConsoleTextWriter(ctx, output));
        }
    }

    [StaticMethod("constructorName")]
    public static string? GetConstructorName(DyObject value) => value is IProduction c ? c.Constructor : null;

    [StaticMethod("typeName")]
    public static string GetTypeName(DyObject value)
    {
        if (value.TypeId is Dy.TypeInfo)
            return ((DyTypeInfo)value).ReflectedTypeName;
        else
            return value.TypeName;
    }

    [StaticMethod("rawget")]
    public static DyObject RawGet(ExecutionContext ctx, DyObject values, DyInteger index)
    {
        if (values is DyClass cls)
            return cls.Fields.GetItem(ctx, index);

        return ctx.RuntimeContext.Types[values.TypeId].RawGet(ctx, values, index);
    }

    [StaticMethod("rawset")]
    public static void RawSet(ExecutionContext ctx, DyObject values, DyInteger index, DyObject value)
    {
        if (values is DyClass cls)
            cls.Fields.SetItem(ctx, index, value);
        else
            ctx.RuntimeContext.Types[values.TypeId].RawSet(ctx, values, index, value);
    }

    [StaticMethod("caller")]
    public static DyObject Caller(ExecutionContext ctx)
    {
        if (ctx.CallStack.Count > 2)
        {
            var cp = ctx.CallStack[ctx.CallStack.Count - 2];
            if (!ReferenceEquals(cp, global::Dyalect.Runtime.Caller.External))
                return cp.Function;
        }

        return Nil;
    }

    [StaticMethod("current")]
    public static DyObject Current(ExecutionContext ctx)
    {
        if (ctx.CallStack.Count > 1)
            return ctx.CallStack.Peek().Function;

        return Nil;
    }

    [StaticMethod("readLine")]
    public static string Read() => Console.ReadLine() ?? "";

    [StaticMethod("rnd")]
    public static int Randomize(ExecutionContext ctx, int min = 0, int max = int.MaxValue, int? seed = null)
    {
        if (seed is null)
        {
            var dt = DateTime.UtcNow;
            var dt2 = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
            seed = (int)(dt2 - dt).Ticks;
        }

        if (min > max)
        {
            ctx.InvalidValue("min", "max");
            return default;
        }

        var rnd = new Random(seed.Value);
        return rnd.Next(min, max);
    }

    [StaticMethod("assert")]
    public static void Assert(ExecutionContext ctx, [Default(true)]DyObject expect, DyObject got, string? errorText = null)
    {
        if (!Eq(ctx, expect.ToObject(), got.ToObject()))
        {
            if (errorText is not null)
                ctx.AssertionFailed(errorText);
            else
                ctx.AssertionFailed($"Expected \"{expect.ToString(ctx)}\" :: {expect.TypeName}, got \"{got.ToString(ctx)}\" :: {got.TypeName}.");
        }
    }

    private static bool Eq(ExecutionContext ctx, object? x, object? y)
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

    [StaticMethod("sqrt")]
    public static double Sqrt(double x) => Math.Sqrt(x);

    [StaticMethod("pow")]
    public static double Pow(double x, double y) => Math.Pow(x, y);

    [StaticMethod("min")]
    public static DyObject Min(ExecutionContext ctx, DyObject x, DyObject y)
    {
        if (x.Lesser(y, ctx))
            return x;
        else
            return y;
    }

    [StaticMethod("max")]
    public static DyObject Max(ExecutionContext ctx, DyObject x, DyObject y)
    {
        if (x.Greater(y, ctx))
            return x;
        else
            return y;
    }

    [StaticMethod("abs")]
    public static DyObject Abs(ExecutionContext ctx, DyObject value)
    {
        if (value.Lesser(DyInteger.Zero, ctx))
            return value.Negate(ctx);

        return value;
    }

    [StaticMethod("round")]
    public static double Round(double number, int digits = 2) => Math.Round(number, digits);
    
    [StaticMethod("sign")]
    public static DyObject Sign(ExecutionContext ctx, DyObject x)
    {
        if (ReferenceEquals(x, DyInteger.Zero)) 
            return DyInteger.Zero;

        if (x.Lesser(DyInteger.Zero, ctx))
            return DyInteger.MinusOne;
        
        return DyInteger.One;
    }

    [StaticMethod("parse")]
    public static DyObject Parse(ExecutionContext ctx, string expression)
    {
        var res = DyParser.Parse(SourceBuffer.FromString(expression));

        if (!res.Success)
            return ctx.ParsingFailed(res.Messages.First().ToString());

        if (res.Value!.Root is null || res.Value!.Root.Nodes.Count == 0)
            return ctx.ParsingFailed("Empty expression.");
        else if (res.Value!.Root.Nodes.Count > 1)
            return ctx.ParsingFailed("Only single expressions allowed.");

        return LiteralEvaluator.Eval(res.Value!.Root.Nodes[0]);
    }

    [StaticMethod("eval")]
    public static DyObject Eval(string source, DyTuple? args = null)
    {
        var sb = new StringBuilder();
        sb.Append("func __x12(");
        
        if (args is not null)
        {
            var tv = args.UnsafeAccess();
            
            for (var i = 0; i < args.Count; i++)
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
        sb.Append(source);
        sb.Append('}');
        sb.Append("__x12");

        var linker = new DyLinker(FileLookup.Default, BuilderOptions.Default());
        var result = linker.Make(SourceBuffer.FromString(sb.ToString()));

        if (!result.Success)
            throw new DyBuildException(result.Messages);

        var newctx = DyMachine.CreateExecutionContext(result.Value!);
        var result2 = DyMachine.Execute(newctx);
        var func = result2.Value!;
        var argsList = new List<DyObject>();

        if (args is null)
            return func!.Invoke(newctx, argsList.ToArray());
        
        var tvv = args.UnsafeAccess();

        for (var i = 0; i < args.Count; i++)
        {
            var o = tvv[i];

            if (o is DyLabel lab)
                argsList.Add(lab.Value);
        }

        return func!.Invoke(newctx, argsList.ToArray());
    }

    [StaticMethod("__invoke")]
    public static DyObject Invoke(ExecutionContext ctx, DyObject functor, params DyObject[] values) => functor.Invoke(ctx, values);
}

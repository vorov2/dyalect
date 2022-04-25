using Dyalect.Debug;
using Dyalect.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Dyalect.Runtime.Types;

internal sealed class DyStringTypeInfo : DyCollectionTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
        | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
        | SupportedOperations.Get | SupportedOperations.Len | SupportedOperations.Iter | SupportedOperations.Lit;

    public override string ReflectedTypeName => nameof(DyType.String);

    public override int ReflectedTypeId => DyType.String;

    public DyStringTypeInfo() => AddMixin(DyType.Collection, DyType.Comparable);

    #region Operations
    protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        var str1 = left.TypeId == DyType.String || left.TypeId == DyType.Char ? left.GetString() : left.ToString(ctx).Value;

        if (ctx.HasErrors)
            return DyNil.Instance;

        var str2 = right.TypeId == DyType.String || right.TypeId == DyType.Char ? right.GetString() : right.ToString(ctx).Value;
        return new DyString(str1 + str2);
    }

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
            return left.GetString() == right.GetString() ? DyBool.True : DyBool.False;
        return base.EqOp(left, right, ctx); //Important! Should redirect to base
    }

    protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
            return left.GetString() != right.GetString() ? DyBool.True : DyBool.False;
        return base.NeqOp(left, right, ctx); //Important! Should redirect to base
    }

    protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
            return left.GetString().CompareTo(right.GetString()) > 0 ? DyBool.True : DyBool.False;
        return base.GtOp(left, right, ctx);
    }

    protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
            return left.GetString().CompareTo(right.GetString()) < 0 ? DyBool.True : DyBool.False;
        return base.LtOp(left, right, ctx);
    }

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
    {
        var len = arg.GetString().Length;
        return DyInteger.Get(len);
    }

    protected override DyObject ContainsOp(DyObject self, DyObject field, ExecutionContext ctx)
    {
        var str = self.GetString();

        if (field.TypeId == DyType.String)
            return str.Contains(field.GetString()) ? DyBool.True : DyBool.False;
        else if (field.TypeId == DyType.Char)
            return str.Contains(field.GetChar()) ? DyBool.True : DyBool.False;
        else
            return ctx.InvalidType(DyType.String, DyType.Char, field);
    }

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) => new DyString(arg.GetString());

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => new DyString(StringUtil.Escape(arg.GetString()));

    protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

    protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
    {
        self.SetItem(index, value, ctx);
        return DyNil.Instance;
    }
    #endregion

    #region Instance
    protected override DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject fromElem, DyObject toElem)
    {
        var str = (DyString)self;

        if (fromElem.TypeId != DyType.Integer)
            return ctx.InvalidType(DyType.Integer, fromElem);

        if (toElem.TypeId != DyType.Nil && toElem.TypeId != DyType.Integer)
            return ctx.InvalidType(DyType.Integer, DyType.Nil, toElem);

        var beg = (int)fromElem.GetInteger();
        var end = ReferenceEquals(toElem, DyNil.Instance) ? str.Count - 1 : (int)toElem.GetInteger();

        if (beg == 0 && end == str.Count - 1)
            return self;

        if (beg < 0)
            beg = str.Count + beg;

        if (beg >= str.Count)
            return ctx.IndexOutOfRange();

        if (end < 0)
            end = str.Count + end - 1;

        if (end >= str.Count)
            return ctx.IndexOutOfRange();

        var len = end - beg + 1;

        if (len < 0)
            return ctx.IndexOutOfRange();

        if (len == 0)
            return DyString.Empty;

        return new DyString(str.Value.Substring(beg, len));
    }

    private DyObject IndexOf(ExecutionContext ctx, DyString self, DyStringLike value, DyInteger fromIndex, DyInteger? count)
    {
        var str = self.Value;
        var icount = (int)(count is null ? str.Length - fromIndex.Value : count.Value);

        if (fromIndex.Value < 0 || fromIndex.Value > str.Length)
            return ctx.IndexOutOfRange();

        if (icount < 0 || icount > str.Length - fromIndex.Value)
            return ctx.IndexOutOfRange();

        return DyInteger.Get(str.IndexOf(value.GetString(), (int)fromIndex.Value, icount));
    }

    private DyObject LastIndexOf(ExecutionContext ctx, DyString self, DyStringLike value, DyInteger? fromIndex, DyInteger? count)
    {
        var str = self.Value;
        var ifrom = fromIndex is null ? str.Length - 1 : (int)fromIndex.Value;
        var icount = count is null ? ifrom + 1 : (int)count.Value;

        if (ifrom < 0 || ifrom > str.Length)
            return ctx.IndexOutOfRange();

        if (icount < 0 || ifrom - icount + 1 < 0)
            return ctx.IndexOutOfRange();

        return DyInteger.Get(str.LastIndexOf(value.GetString(), ifrom, icount));
    }

    private DyObject Split(ExecutionContext ctx, DyString self, DyTuple tuple)
    {
        var allChars = true;
        var values = tuple.GetValues();

        for (var i = 0; i < values.Length; i++)
            if (values[i].TypeId != DyType.Char)
            {
                allChars = false;
                break;
            }

        return allChars ? SplitByChars(ctx, self, values) : SplitByStrings(ctx, self, values);
    }

    private static DyObject SplitByStrings(ExecutionContext ctx, DyString self, DyObject[] args)
    {
        var xs = new string[args.Length];

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, args[i]);

            xs[i] = args[i].GetString();
        }

        var arr = self.Value.Split(xs, StringSplitOptions.RemoveEmptyEntries);
        var list = new DyObject[arr.Length];

        for (var i = 0; i < arr.Length; i++)
            list[i] = new DyString(arr[i]);

        return new DyArray(list);
    }

    private static DyObject SplitByChars(ExecutionContext _, DyString self, DyObject[] args)
    {
        var xs = new char[args.Length];

        for (var i = 0; i < args.Length; i++)
            xs[i] = args[i].GetChar();

        var arr = self.Value.Split(xs, StringSplitOptions.RemoveEmptyEntries);
        var list = new DyObject[arr.Length];

        for (var i = 0; i < arr.Length; i++)
            list[i] = new DyString(arr[i]);

        return new DyArray(list);
    }

    private DyObject Capitalize(ExecutionContext ctx, DyString self)
    {
        var str = self.Value;
        return str.Length == 0 ? DyString.Empty : new DyString(char.ToUpper(str[0]) + str[1..].ToLower());
    }

    private DyObject Upper(ExecutionContext _, DyString self) => new DyString(self.Value.ToUpper());

    private DyObject Lower(ExecutionContext _, DyString self) => new DyString(self.Value.ToLower());

    private DyObject StartsWith(ExecutionContext _, DyString self, DyStringLike a) =>
        self.Value.StartsWith(a.GetString()) ? DyBool.True : DyBool.False;

    private DyObject EndsWith(ExecutionContext _, DyString self, DyStringLike a) =>
        self.Value.EndsWith(a.GetString()) ? DyBool.True : DyBool.False;

    private DyObject Substring(ExecutionContext ctx, DyObject self, DyObject from, DyObject to)
    {
        if (from.TypeId != DyType.Integer)
            return ctx.InvalidType(DyType.Integer, from);

        if (to.TypeId != DyType.Nil && to.TypeId != DyType.Integer)
            return ctx.InvalidType(DyType.Integer, DyType.Nil, to);

        var str = self.GetString();
        var i = (int)from.GetInteger();

        if (i < 0)
            i = str.Length + i;

        if (i >= str.Length)
            return ctx.IndexOutOfRange();

        if (to.TypeId == DyType.Nil)
            return new DyString(str[i..]);

        var j = (int)to.GetInteger();

        if (j < 0 || j + i > str.Length)
            return ctx.IndexOutOfRange();

        return new DyString(self.GetString().Substring(i, j));
    }

    private DyObject Trim(ExecutionContext ctx, DyObject self, DyObject arg) =>
        new DyString(self.GetString().Trim(GetChars(arg, ctx)));

    private DyObject TrimStart(ExecutionContext ctx, DyObject self, DyObject arg) =>
        new DyString(self.GetString().TrimStart(GetChars(arg, ctx)));

    private DyObject TrimEnd(ExecutionContext ctx, DyObject self, DyObject arg) =>
        new DyString(self.GetString().TrimEnd(GetChars(arg, ctx)));

    private static char[] GetChars(DyObject arg, ExecutionContext ctx)
    {
        if (arg.TypeId == DyType.String)
            return arg.GetString().ToCharArray();

        var values = ((DyTuple)arg).GetValues();
        var chs = new char[values.Length];

        for (var i = 0; i < values.Length; i++)
        {
            if (values[i].TypeId != DyType.Char)
            {
                ctx.InvalidType(DyType.Char, values[i]);
                return Array.Empty<char>();
            }
            chs[i] = values[i].GetChar();
        }

        return chs;
    }

    private DyObject IsEmpty(ExecutionContext _, DyObject self) =>
        string.IsNullOrWhiteSpace(self.GetString()) ? DyBool.True : DyBool.False;

    private DyObject Reverse(ExecutionContext _, DyString self)
    {
        var str = self.Value;
        var sb = new StringBuilder(str.Length);

        for (var i = 0; i < str.Length; i++)
            sb.Append(str[str.Length - i - 1]);
        
        return new DyString(sb.ToString());
    }

    private DyObject PadLeft(ExecutionContext ctx, DyObject self, DyObject width, DyObject ch)
    {
        if (width.TypeId != DyType.Integer)
            return ctx.InvalidType(DyType.Integer, width);

        if (ch.TypeId != DyType.Char && ch.TypeId != DyType.String)
            return ctx.InvalidType(DyType.String, DyType.Char, ch);

        var str = self.GetString();
        return new DyString(str.PadLeft((int)width.GetInteger(), ch.GetChar()));
    }

    private DyObject PadRight(ExecutionContext ctx, DyObject self, DyObject len, DyObject with)
    {
        if (len.TypeId != DyType.Integer)
            return ctx.InvalidType(DyType.Integer, len);

        if (with.TypeId != DyType.Char && with.TypeId != DyType.String)
            return ctx.InvalidType(DyType.String, DyType.Char, with);

        var str = self.GetString();
        return new DyString(str.PadRight((int)len.GetInteger(), with.GetChar()));
    }

    private DyObject Replace(ExecutionContext ctx, DyObject self, DyObject oldValue, DyObject newValue, DyObject ignoreCase)
    {
        if (oldValue.TypeId != DyType.String && oldValue.TypeId != DyType.Char)
            return ctx.InvalidType(DyType.String, DyType.Char, oldValue);

        if (newValue.TypeId != DyType.String && newValue.TypeId != DyType.Char)
            return ctx.InvalidType(DyType.String, DyType.Char, newValue);

        return new DyString(self.GetString().Replace(oldValue.GetString(), newValue.GetString(),
            ignoreCase.IsFalse() ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
    }

    private DyObject Remove(ExecutionContext ctx, DyObject self, DyObject from, DyObject count)
    {
        if (from.TypeId != DyType.Integer)
            return ctx.InvalidType(from);

        int fri = (int)from.GetInteger();
        int c;
        var str = self.GetString();

        if (count.TypeId == DyType.Integer)
            c = (int)count.GetInteger();
        else if (count.TypeId == DyType.Nil)
            c = str.Length - fri;
        else
            return ctx.InvalidType(DyType.Integer, DyType.Nil, count);

        if (fri + c > str.Length)
            return ctx.IndexOutOfRange();

        return new DyString(str.Remove(fri, c));
    }

    private DyObject Format(ExecutionContext ctx, DyString self, DyTuple args)
    {
        var vals = args.GetValues();
        var arr = new object[vals.Length];

        for (var i = 0; i < vals.Length; i++)
        {
            var o = vals[i].ToString(ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            arr[i] = o;
        }

        return new DyString(self.GetString().Format(arr));
    }

    private DyObject ToCharArray(ExecutionContext _, DyString self) =>
        new DyArray(self.Value.ToCharArray().Select(c => new DyChar(c)).ToArray());

    protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
        name switch
        {
            Method.IndexOf => Func.Instance<DyString, DyStringLike, DyInteger, DyInteger>(name, IndexOf, "value", new("index", 0), new("count", Nil)),
            Method.LastIndexOf => Func.Instance<DyString, DyStringLike, DyInteger, DyInteger>(name, LastIndexOf, "value", new("index", Nil), new("count", Nil)),
            Method.Split => Func.Instance<DyString, DyTuple>(name, Split, new("separators", ParKind.VarArg)),
            Method.Upper => Func.Instance<DyString>(name, Upper),
            Method.Lower => Func.Instance<DyString>(name, Lower),
            Method.StartsWith => Func.Instance<DyString, DyStringLike>(name, StartsWith, "value"),
            Method.EndsWith => Func.Instance<DyString, DyStringLike>(name, EndsWith, "value"),
            Method.Substring => Func.Member(name, Substring, -1, new Par("index"), new Par("count", DyNil.Instance)),
            Method.Capitalize => Func.Instance<DyString>(name, Capitalize),
            Method.Trim => Func.Member(name, Trim, 0, new Par("chars", ParKind.VarArg)),
            Method.TrimStart => Func.Member(name, TrimStart, 0, new Par("chars", ParKind.VarArg)),
            Method.TrimEnd => Func.Member(name, TrimEnd, 0, new Par("chars", ParKind.VarArg)),
            Method.IsEmpty => Func.Member(name, IsEmpty),
            Method.PadLeft => Func.Member(name, PadLeft, -1, new Par("width"), new Par("char", DyChar.WhiteSpace)),
            Method.PadRight => Func.Member(name, PadRight, -1, new Par("width"), new Par("char", DyChar.WhiteSpace)),
            Method.Replace => Func.Member(name, Replace, -1, new Par("value"), new Par("other"), new Par("ignoreCase", DyBool.False)),
            Method.Remove => Func.Member(name, Remove, -1, new Par("index"), new Par("count", DyNil.Instance)),
            Method.Reverse => Func.Instance<DyString>(name, Reverse),
            Method.ToCharArray => Func.Instance<DyString>(name, ToCharArray),
            Method.Format => Func.Instance<DyString, DyTuple>(name, Format, new("values", ParKind.VarArg)),
            _ => base.InitializeInstanceMember(self, name, ctx),
        };

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Integer => long.TryParse(self.GetString(), out var i8) ? new DyInteger(i8) : DyInteger.Zero,
            DyType.Float => double.TryParse(self.GetString(), out var r8) ? new DyFloat(r8) : DyFloat.Zero,
            _ => base.CastOp(self, targetType, ctx)
        };
    #endregion

    #region Statics
    private DyObject Concat(ExecutionContext ctx, DyTuple tuple)
    {
        var arr = new List<string>();

        if (!Collect(ctx, tuple.GetValues(), arr))
            return DyNil.Instance;

        return new DyString(string.Concat(arr));
    }

    private static bool Collect(ExecutionContext ctx, DyObject[] values, List<string> arr)
    {
        if (ctx.HasErrors)
            return false;

        for (var i = 0; i < values.Length; i++)
        {
            var a = values[i];

            if (a.TypeId == DyType.String || a.TypeId == DyType.Char)
                arr.Add(a.GetString());
            else
            {
                var res = DyString.ToString(a, ctx);

                if (ctx.HasErrors)
                    return false;

                arr.Add(res);
            }
        }

        return true;
    }

    private static DyObject Join(ExecutionContext ctx, DyTuple tuple, DyStringLike separator)
    {
        var strArr = new List<string>();

        if (!Collect(ctx, tuple.GetValues(), strArr))
            return DyNil.Instance;

        return new DyString(string.Join(separator.GetString(), strArr));
    }

    private static DyObject Repeat(ExecutionContext ctx, DyStringLike value, DyInteger count)
    {
        var sb = new StringBuilder();
        var str = value.GetString();

        for (var i = 0; i < count.Value; i++)
            sb.Append(str);

        return new DyString(sb.ToString());
    }

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
        name switch
        {
            Method.String or Method.Concat => Func.Static<DyTuple>(name, Concat, new("values", ParKind.VarArg)),
            Method.Join => Func.Static<DyTuple, DyStringLike>(name, Join, new("values", ParKind.VarArg), new("separator", ",")),
            Method.Default => Func.Static(name, _ => DyString.Empty),
            Method.Repeat => Func.Static<DyStringLike, DyInteger>(name, Repeat, "value", "count"),
            Method.Format => Func.Static<DyString, DyTuple>(name, Format, "template", new("values", ParKind.VarArg)),
            _ => base.InitializeStaticMember(name, ctx),
        };
    #endregion
}

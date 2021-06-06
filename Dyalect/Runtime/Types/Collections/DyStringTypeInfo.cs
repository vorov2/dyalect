using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyStringTypeInfo : DyCollectionTypeInfo
    {
        public DyStringTypeInfo() : base(DyType.String) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
            | SupportedOperations.Get | SupportedOperations.Len | SupportedOperations.Iter;

        public override string TypeName => DyTypeNames.String;

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
            return ctx.InvalidType(right);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
                return left.GetString().CompareTo(right.GetString()) < 0 ? DyBool.True : DyBool.False;
            return ctx.InvalidType(right);
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = arg.GetString().Length;
            return DyInteger.Get(len);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => (DyString)arg.GetString();

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
        }

        protected override DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject fromElem, DyObject toElem)
        {
            var str = (DyString)self;

            if (fromElem.TypeId != DyType.Integer)
                return ctx.InvalidType(fromElem);

            if (toElem.TypeId != DyType.Nil && toElem.TypeId != DyType.Integer)
                return ctx.InvalidType(toElem);

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

        private DyObject Contains(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var str = self.GetString();

            if (value.TypeId == DyType.String)
                return str.Contains(value.GetString()) ? DyBool.True : DyBool.False;
            else if (value.TypeId == DyType.Char)
                return str.Contains(value.GetChar()) ? DyBool.True : DyBool.False;
            else
                return ctx.InvalidType(value);
        }

        private DyObject IndexOf(ExecutionContext ctx, DyObject self, DyObject value, DyObject fromIndex, DyObject count)
        {
            var str = self.GetString();
            var ifrom = (int)fromIndex.GetInteger();
            var icount = count == DyNil.Instance ? str.Length - ifrom : (int)count.GetInteger();

            if (ifrom < 0 || ifrom > str.Length)
                return ctx.IndexOutOfRange();

            if (icount < 0 || icount > str.Length - ifrom)
                return ctx.IndexOutOfRange();

            if (value.TypeId == DyType.String)
                return DyInteger.Get(str.IndexOf(value.GetString(), ifrom, icount));
            else if (value.TypeId == DyType.Char)
                return DyInteger.Get(str.IndexOf(value.GetChar(), ifrom, icount));
            else
                return ctx.InvalidType(value);
        }

        private DyObject LastIndexOf(ExecutionContext ctx, DyObject self, DyObject value, DyObject fromIndex, DyObject count)
        {
            var str = self.GetString();
            var ifrom = fromIndex.TypeId == DyType.Nil ? str.Length - 1 : (int)fromIndex.GetInteger();
            var icount = count == DyNil.Instance ? ifrom + 1 : (int)count.GetInteger();

            if (ifrom < 0 || ifrom > str.Length)
                return ctx.IndexOutOfRange();

            if (icount < 0 || ifrom - icount + 1 < 0)
                return ctx.IndexOutOfRange();

            if (value.TypeId == DyType.String)
                return DyInteger.Get(str.LastIndexOf(value.GetString(), ifrom, icount));
            else if (value.TypeId == DyType.Char)
                return DyInteger.Get(str.LastIndexOf(value.GetChar(), ifrom, icount));
            else
                return ctx.InvalidType(value);
        }

        private DyObject Split(ExecutionContext ctx, DyObject self, DyObject arg)
        {
            var allChars = true;
            var values = ((DyTuple)arg).Values;

            for (var i = 0; i < values.Length; i++)
                if (values[i].TypeId != DyType.Char)
                {
                    allChars = false;
                    break;
                }

            return allChars ? SplitByChars(ctx, self, values) : SplitByStrings(ctx, self, values);
        }

        private static DyObject SplitByStrings(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var xs = new string[args.Length];

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].TypeId != DyType.String)
                    return ctx.InvalidType(args[i]);

                xs[i] = args[i].GetString();
            }

            var arr = self.GetString().Split(xs, StringSplitOptions.RemoveEmptyEntries);
            var list = new DyObject[arr.Length];

            for (var i = 0; i < arr.Length; i++)
                list[i] = new DyString(arr[i]);

            return new DyArray(list);
        }

        private static DyObject SplitByChars(ExecutionContext _, DyObject self, DyObject[] args)
        {
            var xs = new char[args.Length];

            for (var i = 0; i < args.Length; i++)
                xs[i] = args[i].GetChar();

            var arr = self.GetString().Split(xs, StringSplitOptions.RemoveEmptyEntries);
            var list = new DyObject[arr.Length];

            for (var i = 0; i < arr.Length; i++)
                list[i] = new DyString(arr[i]);

            return new DyArray(list);
        }

        private DyObject Capitalize(ExecutionContext _, DyObject self)
        {
            var str = self.GetString();
            return str.Length == 0 ? DyString.Empty : new DyString(char.ToUpper(str[0]) + str[1..].ToLower());
        }

        private DyObject Upper(ExecutionContext _, DyObject self) =>
            new DyString(self.GetString().ToUpper());

        private DyObject Lower(ExecutionContext _, DyObject self) =>
            new DyString(self.GetString().ToLower());

        private DyObject StartsWith(ExecutionContext ctx, DyObject self, DyObject a)
        {
            if (a.TypeId == DyType.String)
                return self.GetString().StartsWith(a.GetString()) ? DyBool.True : DyBool.False;

            if (a.TypeId == DyType.Char)
                return self.GetString().StartsWith(a.GetChar()) ? DyBool.True : DyBool.False;

            return ctx.InvalidType(a);
        }

        private DyObject EndsWith(ExecutionContext ctx, DyObject self, DyObject a)
        {
            if (a.TypeId == DyType.String)
                return self.GetString().EndsWith(a.GetString()) ? DyBool.True : DyBool.False;

            if (a.TypeId == DyType.Char)
                return self.GetString().EndsWith(a.GetChar()) ? DyBool.True : DyBool.False;

            return ctx.InvalidType(a);
        }

        private DyObject Substring(ExecutionContext ctx, DyObject self, DyObject from, DyObject to)
        {
            if (from.TypeId != DyType.Integer)
                return ctx.InvalidType(from);

            if (!ReferenceEquals(to, DyNil.Instance) && to.TypeId != DyType.Integer)
                return ctx.InvalidType(to);

            var str = self.GetString();
            var i = (int)from.GetInteger();

            if (i < 0)
                i = str.Length + i;

            if (i >= str.Length)
                return ctx.IndexOutOfRange();

            if (ReferenceEquals(to, DyNil.Instance))
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
            var values = ((DyTuple)arg).Values;
            var chs = new char[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i].TypeId != DyType.Char)
                {
                    ctx.InvalidType(values[i]);
                    return Array.Empty<char>();
                }
                chs[i] = values[i].GetChar();
            }

            return chs;
        }

        private DyObject IsEmpty(ExecutionContext ctx, DyObject self) =>
            string.IsNullOrWhiteSpace(self.GetString()) ? DyBool.True : DyBool.False;

        private DyObject Reverse(ExecutionContext ctx, DyObject self)
        {
            var str = self.GetString();
            var sb = new StringBuilder(str.Length);
            for (var i = 0; i < str.Length; i++)
                sb.Append(str[str.Length - i - 1]);
            return new DyString(sb.ToString());
        }

        private DyObject PadLeft(ExecutionContext ctx, DyObject self, DyObject len, DyObject with)
        {
            if (len.TypeId != DyType.Integer)
                return ctx.InvalidType(len);

            if (with.TypeId != DyType.Char)
                return ctx.InvalidType(with);

            var str = self.GetString();
            return new DyString(str.PadLeft((int)len.GetInteger(), with.GetChar()));
        }

        private DyObject PadRight(ExecutionContext ctx, DyObject self, DyObject len, DyObject with)
        {
            if (len.TypeId != DyType.Integer)
                return ctx.InvalidType(len);

            if (with.TypeId != DyType.Char)
                return ctx.InvalidType(with);

            var str = self.GetString();
            return new DyString(str.PadRight((int)len.GetInteger(), with.GetChar()));
        }

        private DyObject Replace(ExecutionContext ctx, DyObject self, DyObject oldValue, DyObject newValue, DyObject ignoreCase)
        {
            if (oldValue.TypeId != DyType.String && oldValue.TypeId != DyType.Char)
                return ctx.InvalidType(oldValue);

            if (newValue.TypeId != DyType.String && newValue.TypeId != DyType.Char)
                return ctx.InvalidType(newValue);

            return new DyString(self.GetString().Replace(oldValue.GetString(), newValue.GetString(),
                ignoreCase.GetBool() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
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
                return ctx.InvalidType(count);

            if (fri + c > str.Length)
                return ctx.IndexOutOfRange();

            return new DyString(str.Remove(fri, c));
        }

        private DyObject ToCharArray(ExecutionContext ctx, DyObject self) =>
            new DyArray(self.GetString().ToCharArray().Select(c => new DyChar(c)).ToArray());

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "indexOf" => Func.Member(name, IndexOf, -1, new Par("value"),
                    new Par("fromIndex", DyInteger.Get(0)), new Par("count", DyNil.Instance)),
                "lastIndexOf" => Func.Member(name, LastIndexOf, -1, new Par("value"),
                    new Par("fromIndex", DyNil.Instance), new Par("count", DyNil.Instance)),
                "contains" => Func.Member(name, Contains, -1, new Par("value")),
                "split" => Func.Member(name, Split, 0, new Par("separators", true)),
                "upper" => Func.Member(name, Upper),
                "lower" => Func.Member(name, Lower),
                "startsWith" => Func.Member(name, StartsWith, -1, new Par("value")),
                "endsWith" => Func.Member(name, EndsWith, -1, new Par("value")),
                "substring" => Func.Member(name, Substring, -1, new Par("start"), new Par("len", DyNil.Instance)),
                "capitalize" => Func.Member(name, Capitalize),
                "trim" => Func.Member(name, Trim, 0, new Par("chars", true)),
                "trimStart" => Func.Member(name, TrimStart, 0, new Par("chars", true)),
                "trimEnd" => Func.Member(name, TrimEnd, 0, new Par("chars", true)),
                "isEmpty" => Func.Member(name, IsEmpty),
                "padLeft" => Func.Member(name, PadLeft, -1, new Par("to"), new Par("with", new DyChar(' '))),
                "padRight" => Func.Member(name, PadRight, -1, new Par("to"), new Par("with", new DyChar(' '))),
                "replace" => Func.Member(name, Replace, -1, new Par("value"), new Par("with"), new Par("ignoreCase", DyBool.False)),
                "remove" => Func.Member(name, Remove, -1, new Par("from"), new Par("count", DyNil.Instance)),
                "reverse" => Func.Member(name, Reverse),
                "toCharArray" => Func.Member(name, ToCharArray),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };
        #endregion

        #region Statics
        private DyObject Concat(ExecutionContext ctx, DyObject tuple)
        {
            var values = ((DyTuple)tuple).Values;
            var arr = new List<string>();

            if (!Collect(ctx, values, arr))
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

        private static DyObject Join(ExecutionContext ctx, DyObject values, DyObject separator)
        {
            if (separator.TypeId != DyType.String)
                return ctx.InvalidType(separator);

            var arr = ((DyTuple)values).Values;
            var strArr = new List<string>();

            if (!Collect(ctx, arr, strArr))
                return DyNil.Instance;

            return new DyString(string.Join(separator.GetString(), strArr));
        }

        private static DyObject Repeat(ExecutionContext ctx, DyObject value, DyObject count)
        {
            if (count.TypeId is not DyType.Integer)
                return ctx.InvalidType(count);

            if (value.TypeId is not DyType.Char and not DyType.String)
                return ctx.InvalidType(value);

            var c = (int)count.GetInteger();
            var sb = new StringBuilder();

            for (var i = 0; i < c; i++)
                sb.Append(value.GetString());

            return new DyString(sb.ToString());
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "String" => Func.Static(name, Concat, 0, new Par("values", true)),
                "concat" => Func.Static(name, Concat, 0, new Par("values", true)),
                "join" => Func.Static(name, Join, 0, new Par("values", true), new Par("separator", new DyString(","))),
                "default" => Func.Static(name, _ => DyString.Empty),
                "repeat" => Func.Static(name, Repeat, -1, new Par("value"), new Par("count")),
                _ => base.InitializeStaticMember(name, ctx),
            };
        #endregion
    }
}

using Dyalect.Debug;
using Dyalect.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyStringTypeInfo : DyCollectionTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
            | SupportedOperations.Get | SupportedOperations.Len | SupportedOperations.Iter | SupportedOperations.Lit;

        public override string TypeName => DyTypeNames.String;

        public override int ReflectedTypeId => DyType.String;

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

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) => new DyString(arg.GetString());

        protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => new DyString(StringUtil.Escape(arg.GetString()));

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

        private DyObject Contains(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var str = self.GetString();

            if (value.TypeId == DyType.String)
                return str.Contains(value.GetString()) ? DyBool.True : DyBool.False;
            else if (value.TypeId == DyType.Char)
                return str.Contains(value.GetChar()) ? DyBool.True : DyBool.False;
            else
                return ctx.InvalidType(DyType.String, DyType.Char, value);
        }

        private DyObject IndexOf(ExecutionContext ctx, DyObject self, DyObject value, DyObject fromIndex, DyObject count)
        {
            var str = self.GetString();
            var ifrom = (int)fromIndex.GetInteger();
            var icount = count.TypeId == DyType.Nil ? str.Length - ifrom : (int)count.GetInteger();

            if (ifrom < 0 || ifrom > str.Length)
                return ctx.IndexOutOfRange();

            if (icount < 0 || icount > str.Length - ifrom)
                return ctx.IndexOutOfRange();

            if (value.TypeId == DyType.String)
                return DyInteger.Get(str.IndexOf(value.GetString(), ifrom, icount));
            else if (value.TypeId == DyType.Char)
                return DyInteger.Get(str.IndexOf(value.GetChar(), ifrom, icount));
            else
                return ctx.InvalidType(DyType.String, DyType.Char, value);
        }

        private DyObject LastIndexOf(ExecutionContext ctx, DyObject self, DyObject value, DyObject fromIndex, DyObject count)
        {
            var str = self.GetString();
            var ifrom = fromIndex.TypeId == DyType.Nil ? str.Length - 1 : (int)fromIndex.GetInteger();
            var icount = count.TypeId == DyType.Nil ? ifrom + 1 : (int)count.GetInteger();

            if (ifrom < 0 || ifrom > str.Length)
                return ctx.IndexOutOfRange();

            if (icount < 0 || ifrom - icount + 1 < 0)
                return ctx.IndexOutOfRange();

            if (value.TypeId == DyType.String)
                return DyInteger.Get(str.LastIndexOf(value.GetString(), ifrom, icount));
            else if (value.TypeId == DyType.Char)
                return DyInteger.Get(str.LastIndexOf(value.GetChar(), ifrom, icount));
            else
                return ctx.InvalidType(DyType.String, DyType.Char, value);
        }

        private DyObject Split(ExecutionContext ctx, DyObject self, DyObject arg)
        {
            var allChars = true;
            var values = ((DyTuple)arg).GetValues();

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
                    return ctx.InvalidType(DyType.String, args[i]);

                xs[i] = args[i].GetString();
            }

            var arr = self.GetString().Split(xs, StringSplitOptions.RemoveEmptyEntries);
            var list = new DyObject[arr.Length];

            for (var i = 0; i < arr.Length; i++)
                list[i] = new DyString(arr[i]);

            return new DyArray(list);
        }

        private static DyObject SplitByChars(ExecutionContext ctx, DyObject self, DyObject[] args)
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

        private DyObject Capitalize(ExecutionContext ctx, DyObject self)
        {
            var str = self.GetString();
            return str.Length == 0 ? DyString.Empty
                : new DyString(char.ToUpper(str[0]) + str[1..].ToLower());
        }

        private DyObject Upper(ExecutionContext ctx, DyObject self) =>
            new DyString(self.GetString().ToUpper());

        private DyObject Lower(ExecutionContext ctx, DyObject self) =>
            new DyString(self.GetString().ToLower());

        private DyObject StartsWith(ExecutionContext ctx, DyObject self, DyObject a)
        {
            if (a.TypeId == DyType.String)
                return self.GetString().StartsWith(a.GetString()) ? DyBool.True : DyBool.False;

            if (a.TypeId == DyType.Char)
                return self.GetString().StartsWith(a.GetChar()) ? DyBool.True : DyBool.False;

            return ctx.InvalidType(DyType.String, DyType.Char, a);
        }

        private DyObject EndsWith(ExecutionContext ctx, DyObject self, DyObject a)
        {
            if (a.TypeId == DyType.String)
                return self.GetString().EndsWith(a.GetString()) ? DyBool.True : DyBool.False;

            if (a.TypeId == DyType.Char)
                return self.GetString().EndsWith(a.GetChar()) ? DyBool.True : DyBool.False;

            return ctx.InvalidType(DyType.String, DyType.Char, a);
        }

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

        private DyObject Format(ExecutionContext ctx, DyObject self, DyObject args)
        {
            if (self.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, self);

            var vals = ((DyTuple)args).GetValues();
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

        private DyObject ToCharArray(ExecutionContext ctx, DyObject self) =>
            new DyArray(self.GetString().ToCharArray().Select(c => new DyChar(c)).ToArray());

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                Method.IndexOf => Func.Member(name, IndexOf, -1, new Par("value"),
                    new Par("index", DyInteger.Zero), new Par("count", DyNil.Instance)),
                Method.LastIndexOf => Func.Member(name, LastIndexOf, -1, new Par("value"),
                    new Par("index", DyNil.Instance), new Par("count", DyNil.Instance)),
                Method.Contains => Func.Member(name, Contains, -1, new Par("value")),
                Method.Split => Func.Member(name, Split, 0, new Par("separators", true)),
                Method.Upper => Func.Member(name, Upper),
                Method.Lower => Func.Member(name, Lower),
                Method.StartsWith => Func.Member(name, StartsWith, -1, new Par("value")),
                Method.EndsWith => Func.Member(name, EndsWith, -1, new Par("value")),
                Method.Substring => Func.Member(name, Substring, -1, new Par("index"), new Par("count", DyNil.Instance)),
                Method.Capitalize => Func.Member(name, Capitalize),
                Method.Trim => Func.Member(name, Trim, 0, new Par("chars", true)),
                Method.TrimStart => Func.Member(name, TrimStart, 0, new Par("chars", true)),
                Method.TrimEnd => Func.Member(name, TrimEnd, 0, new Par("chars", true)),
                Method.IsEmpty => Func.Member(name, IsEmpty),
                Method.PadLeft => Func.Member(name, PadLeft, -1, new Par("width"), new Par("char", DyChar.WhiteSpace)),
                Method.PadRight => Func.Member(name, PadRight, -1, new Par("width"), new Par("char", DyChar.WhiteSpace)),
                Method.Replace => Func.Member(name, Replace, -1, new Par("value"), new Par("other"), new Par("ignoreCase", DyBool.False)),
                Method.Remove => Func.Member(name, Remove, -1, new Par("index"), new Par("count", DyNil.Instance)),
                Method.Reverse => Func.Member(name, Reverse),
                Method.ToCharArray => Func.Member(name, ToCharArray),
                Method.Format => Func.Member(name, Format, 0, new Par("values", true)),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Integer => long.TryParse(self.GetString(), out var i8) ? DyInteger.Get(i8) : DyInteger.Zero,
                DyType.Float => double.TryParse(self.GetString(), out var r8) ? new DyFloat(r8) : DyFloat.Zero,
                _ => base.CastOp(self, targetType, ctx)
            };
        #endregion

        #region Statics
        private DyObject Concat(ExecutionContext ctx, DyObject tuple)
        {
            var values = ((DyTuple)tuple).GetValues();
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
            if (separator.TypeId != DyType.String && separator.TypeId != DyType.Char)
                return ctx.InvalidType(DyType.String, DyType.Char, separator);

            var arr = ((DyTuple)values).GetValues();
            var strArr = new List<string>();

            if (!Collect(ctx, arr, strArr))
                return DyNil.Instance;

            return new DyString(string.Join(separator.GetString(), strArr));
        }

        private static DyObject Repeat(ExecutionContext ctx, DyObject value, DyObject count)
        {
            if (count.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, count);

            if (value.TypeId != DyType.Char && value.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, DyType.Char, value);

            var c = (int)count.GetInteger();
            var sb = new StringBuilder();

            for (var i = 0; i < c; i++)
                sb.Append(value.GetString());

            return new DyString(sb.ToString());
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                Method.String => Func.Static(name, Concat, 0, new Par("values", true)),
                Method.Concat => Func.Static(name, Concat, 0, new Par("values", true)),
                Method.Join => Func.Static(name, Join, 0, new Par("values", true), new Par("separator", new DyString(","))),
                Method.Default => Func.Static(name, ctx => DyString.Empty),
                Method.Repeat => Func.Static(name, Repeat, -1, new Par("value"), new Par("count")),
                Method.Format => Func.Static(name, Format, 1, new Par("template"), new Par("values", true)),
                _ => base.InitializeStaticMember(name, ctx),
            };
        #endregion
    }
}

using Dyalect.Debug;
using Dyalect.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public sealed class DyString : DyCollection, IEnumerable<DyObject>
    {
        public static readonly DyString Empty = new DyString("");
        internal readonly string Value;

        public override int Count => Value.Length;

        public DyString(string str) : base(DyType.String)
        {
            Value = str;
        }

        internal override DyObject GetValue(int index) => GetItem(index, ExecutionContext.Default);

        internal override DyObject[] GetValues()
        {
            var arr = new DyObject[Value.Length];

            for (var i = 0; i < Value.Length; i++)
                arr[i] = new DyChar(Value[i]);

            return arr;
        }

        public override object ToObject() => Value;

        public override string ToString() => Value;

        public override int GetHashCode() => Value.GetHashCode();

        protected internal override bool GetBool() => !string.IsNullOrEmpty(Value);

        public override bool Equals(DyObject obj) =>
            obj is DyString s ? Value == s.Value : base.Equals(obj);

        internal protected override string GetString() => Value;

        public static implicit operator string(DyString str) => str.Value;

        public static implicit operator DyString(string str) => new DyString(str);

        public static string ToString(DyObject value, ExecutionContext ctx)
        {
            var res = value;

            while (res.TypeId != DyType.String && res.TypeId != DyType.Char)
            {
                res = res.ToString(ctx);

                if (ctx.HasErrors)
                    return null;
            }

            return res.GetString();
        }

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId != DyType.Integer)
                return ctx.IndexInvalidType(index);

            var idx = (int)index.GetInteger();
            return GetItem(idx, ctx);
        }

        protected internal override DyObject GetItem(int idx, ExecutionContext ctx)
        {
            if (idx < 0 || idx >= Value.Length)
                return ctx.IndexOutOfRange(idx);

            return new DyChar(Value[idx]);
        }

        public override DyObject Clone() => this;

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeId);
            writer.Write(Value);
        }
    }

    internal sealed class DyStringTypeInfo : DyTypeInfo
    {
        public DyStringTypeInfo() : base(DyType.String)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
            | SupportedOperations.Get | SupportedOperations.Len;

        public override string TypeName => DyTypeNames.String;

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var str1 = left.TypeId == DyType.String || left.TypeId == DyType.Char ? left.GetString() : left.ToString(ctx).Value;
            var str2 = right.TypeId == DyType.String || right.TypeId == DyType.Char ? right.GetString() : right.ToString(ctx).Value;
            return new DyString(str1 + str2);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
                return left.GetString() == right.GetString() ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
                return left.GetString() != right.GetString() ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
                return left.GetString().CompareTo(right.GetString()) > 0 ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
                return left.GetString().CompareTo(right.GetString()) < 0 ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = arg.GetString().Length;
            return DyInteger.Get(len);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => (DyString)StringUtil.Escape(arg.GetString());

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject GetOp(DyObject self, int index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
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
                return ctx.IndexOutOfRange(fromIndex);

            if (icount < 0 || icount > str.Length - ifrom)
                return ctx.IndexOutOfRange(count);

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
                return ctx.IndexOutOfRange(fromIndex);

            if (icount < 0 || ifrom - icount + 1 < 0)
                return ctx.IndexOutOfRange(fromIndex);

            if (value.TypeId == DyType.String)
                return DyInteger.Get(str.LastIndexOf(value.GetString(), ifrom, icount));
            else if (value.TypeId == DyType.Char)
                return DyInteger.Get(str.LastIndexOf(value.GetChar(), ifrom, icount));
            else
                return ctx.InvalidType(value);
        }

        private DyObject Split(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var allChars = true;
            var values = ((DyTuple)args[0]).Values;

            for (var i = 0; i < values.Length; i++)
                if (values[i].TypeId != DyType.Char)
                {
                    allChars = false;
                    break;
                }

            return allChars ? SplitByChars(ctx, self, values) : SplitByStrings(ctx, self, values);
        }

        private DyObject SplitByStrings(ExecutionContext ctx, DyObject self, DyObject[] args)
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

        private DyObject SplitByChars(ExecutionContext ctx, DyObject self, DyObject[] args)
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

        private DyObject Capitalize(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var str = self.GetString();
            return str.Length == 0 ? DyString.Empty : new DyString(char.ToUpper(str[0]) + str.Substring(1).ToLower());
        }

        private DyObject Upper(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return new DyString(self.GetString().ToUpper());
        }

        private DyObject Lower(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return new DyString(self.GetString().ToLower());
        }

        private DyObject StartsWith(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var a = args.TakeOne(DyNil.Instance);

            if (a.TypeId == DyType.String)
                return self.GetString().StartsWith(a.GetString()) ? DyBool.True : DyBool.False;

            if (a.TypeId == DyType.Char)
                return self.GetString().StartsWith(a.GetChar()) ? DyBool.True : DyBool.False;

            return ctx.InvalidType(a);
        }

        private DyObject EndsWith(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var a = args.TakeOne(DyNil.Instance);

            if (a.TypeId == DyType.String)
                return self.GetString().EndsWith(a.GetString()) ? DyBool.True : DyBool.False;

            if (a.TypeId == DyType.Char)
                return self.GetString().EndsWith(a.GetChar()) ? DyBool.True : DyBool.False;

            return ctx.InvalidType(a);
        }

        private DyObject Substring(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var from = args.TakeOne(DyNil.Instance);
            var to = args.TakeAt(1, null);

            if (from.TypeId != DyType.Integer)
                return ctx.InvalidType(from);

            if (!ReferenceEquals(to, DyNil.Instance) && to.TypeId != DyType.Integer)
                return ctx.InvalidType(to);

            var str = self.GetString();
            var i = (int)from.GetInteger();

            if (i < 0 || i >= str.Length)
                return ctx.IndexOutOfRange(i);

            if (ReferenceEquals(to, DyNil.Instance))
                return new DyString(str.Substring(i));

            var j = (int)to.GetInteger();

            if (j < 0 || j + i > str.Length)
                return ctx.IndexOutOfRange(j);

            return new DyString(self.GetString().Substring(i, j));
        }

        private DyObject Trim(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return new DyString(self.GetString().Trim(GetChars(args, ctx)));
        }

        private DyObject TrimStart(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return new DyString(self.GetString().TrimStart(GetChars(args, ctx)));
        }

        private DyObject TrimEnd(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return new DyString(self.GetString().TrimEnd(GetChars(args, ctx)));
        }

        private char[] GetChars(DyObject[] args, ExecutionContext ctx)
        {
            if (args[0] == null)
                return Statics.EmptyChars;

            var values = ((DyTuple)args[0]).Values;
            var chs = new char[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i].TypeId != DyType.Char)
                {
                    ctx.InvalidType(values[i]);
                    return Statics.EmptyChars;
                }
                chs[i] = values[i].GetChar();
            }

            return chs;
        }

        private DyObject IsEmpty(ExecutionContext ctx, DyObject self)
        {
            return string.IsNullOrWhiteSpace(self.GetString()) ? DyBool.True : DyBool.False;
        }

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
                return ctx.IndexOutOfRange(fri + c);

            return new DyString(str.Remove(fri, c));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            switch (name)
            {
                case "indexOf":
                    return DyForeignFunction.Member(name, IndexOf, -1, new Par("value"), new Par("fromIndex", DyInteger.Get(0)), new Par("count", DyNil.Instance));
                case "lastIndexOf":
                    return DyForeignFunction.Member(name, LastIndexOf, -1, new Par("value"), new Par("fromIndex", DyNil.Instance), new Par("count", DyNil.Instance));
                case "contains":
                    return DyForeignFunction.Member(name, Contains, -1, new Par("value"));
                case "split":
                    return DyForeignFunction.Member(name, Split, 0, new Par("separators", true));
                case "upper":
                    return DyForeignFunction.Member(name, Upper, -1, Statics.EmptyParameters);
                case "lower":
                    return DyForeignFunction.Member(name, Lower, -1, Statics.EmptyParameters);
                case "startsWith":
                    return DyForeignFunction.Member(name, StartsWith, -1, new Par("value"));
                case "endsWith":
                    return DyForeignFunction.Member(name, EndsWith, -1, new Par("value"));
                case "sub":
                    return DyForeignFunction.Member(name, Substring, -1, new Par("start"), new Par("len", DyNil.Instance));
                case "capitalize":
                    return DyForeignFunction.Member(name, Capitalize, -1, Statics.EmptyParameters);
                case "trim":
                    return DyForeignFunction.Member(name, Trim, 0, new Par("chars", true));
                case "trimStart":
                    return DyForeignFunction.Member(name, TrimStart, 0, new Par("chars", true));
                case "trimEnd":
                    return DyForeignFunction.Member(name, TrimEnd, 0, new Par("chars", true));
                case "isEmpty":
                    return DyForeignFunction.Member(name, IsEmpty);
                case "padLeft":
                    return DyForeignFunction.Member(name, PadLeft, -1, new Par("to"), new Par("with", new DyChar(' ')));
                case "padRight":
                    return DyForeignFunction.Member(name, PadRight, -1, new Par("to"), new Par("with", new DyChar(' ')));
                case "replace":
                    return DyForeignFunction.Member(name, Replace, -1, new Par("value"), new Par("with"),
                        new Par("ignoreCase", (DyObject)DyBool.False));
                case "remove":
                    return DyForeignFunction.Member(name, Remove, -1, new Par("from"), new Par("count", DyNil.Instance));
                case "reverse":
                    return DyForeignFunction.Member(name, Reverse);
                default:
                    return null;
            }
        }
        #endregion

        #region Statics
        private DyObject Concat(ExecutionContext ctx, DyObject tuple)
        {
            var values = ((DyTuple)tuple).Values;
            var arr = new string[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                var a = values[i];

                if (a.TypeId == DyType.String || a.TypeId == DyType.Char)
                    arr[i] = a.GetString();
                else
                {
                    var res = DyString.ToString(a, ctx);

                    if (ctx.HasErrors)
                        return DyNil.Instance;

                    arr[i] = res;
                }
            }

            return new DyString(string.Concat(arr));
        }

        private static DyObject Join(ExecutionContext ctx, DyObject values, DyObject separator)
        {
            if (separator.TypeId != DyType.String)
                return ctx.InvalidType(separator);

            var arr = ((DyTuple)values).Values;
            var strArr = new string[arr.Length];

            for (var i = 0; i < arr.Length; i++)
            {
                var a = arr[i];

                if (a.TypeId == DyType.String || a.TypeId == DyType.Char)
                    strArr[i] = a.GetString();
                else
                {
                    strArr[i] = arr[i].ToString(ctx);

                    if (ctx.HasErrors)
                        return DyNil.Instance;
                }
            }

            return new DyString(string.Join(separator.GetString(), strArr));
        }

        private static DyObject Repeat(ExecutionContext ctx, DyObject value, DyObject count)
        {
            if (count.TypeId != DyType.Integer)
                return ctx.InvalidType(count);

            if (value.TypeId != DyType.Char)
                return ctx.InvalidType(value);

            return new DyString(new string(value.GetChar(), (int)count.GetInteger()));
        }

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            switch (name)
            {
                case "String":
                    return DyForeignFunction.Static(name, Concat, 0, new Par("values", true));
                case "concat":
                    return DyForeignFunction.Static(name, Concat, 0, new Par("values", true));
                case "join":
                    return DyForeignFunction.Static(name, Join, 0, new Par("values", true), new Par("separator", new DyString(",")));
                case "default":
                    return DyForeignFunction.Auto(AutoKind.Generated, (c, _) => DyString.Empty);
                case "repeat":
                    return DyForeignFunction.Static(name, Repeat, -1, new Par("value"), new Par("count"));
                default:
                    return null;
            }
        }
        #endregion
    }
}

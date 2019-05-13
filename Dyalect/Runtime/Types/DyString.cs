using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public sealed class DyString : DyObject, IEnumerable<DyObject>
    {
        public static readonly DyString Empty = new DyString("");
        internal readonly string Value;

        public DyString(string str) : base(StandardType.String)
        {
            Value = str;
        }

        public override object ToObject() => Value;

        public override string ToString() => Value;

        public override int GetHashCode() => Value.GetHashCode();

        protected internal override bool GetBool() => !string.IsNullOrEmpty(Value);

        public override bool Equals(object obj)
        {
            if (obj is DyString s)
                return Value.Equals(s.Value);
            else
                return false;
        }

        internal protected override string GetString() => Value;

        public static implicit operator string(DyString str) => str.Value;

        public static implicit operator DyString(string str) => new DyString(str);

        public static string ToString(DyObject value, ExecutionContext ctx)
        {
            var res = value;

            while (res.TypeId != StandardType.String && res.TypeId != StandardType.Char)
            {
                res = res.ToString(ctx);

                if (ctx.HasErrors)
                    return null;
            }

            return res.GetString();
        }

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId != StandardType.Integer)
                return Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);

            var idx = (int)index.GetInteger();

            if (idx < 0 || idx >= Value.Length)
                return Err.IndexOutOfRange(this.TypeName(ctx), idx).Set(ctx);

            return new DyString(Value[idx].ToString());
        }

        public IEnumerator<DyObject> GetEnumerator() => Value.Select(c => new DyChar(c)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal sealed class DyStringTypeInfo : DyTypeInfo
    {
        public DyStringTypeInfo() : base(StandardType.String, false)
        {

        }

        public override string TypeName => StandardType.StringName;

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var str1 = left.TypeId == StandardType.String || left.TypeId == StandardType.Char ? left.GetString() : left.ToString(ctx).Value;
            var str2 = right.TypeId == StandardType.String || right.TypeId == StandardType.Char ? right.GetString() : right.ToString(ctx).Value;
            return new DyString(str1 + str2);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetString() == right.GetString() ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetString() != right.GetString() ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetString().CompareTo(right.GetString()) > 0 ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetString().CompareTo(right.GetString()) < 0 ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = arg.GetString().Length;
            return DyInteger.Get(len);
        }

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) => StringUtil.Escape(arg.GetString());

        private DyObject Contains(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var a = args.TakeOne(DyNil.Instance);
            var str = self.GetString();

            if (a.TypeId == StandardType.String)
                return str.Contains(a.GetString()) ? DyBool.True : DyBool.False;
            else if (a.TypeId == StandardType.Char)
                return str.Contains(a.GetChar()) ? DyBool.True : DyBool.False;
            else
                return Err.InvalidType(StandardType.CharName, a.TypeName(ctx)).Set(ctx);
        }

        private DyObject IndexOf(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var a = args.TakeOne(DyNil.Instance);
            var str = self.GetString();

            if (a.TypeId == StandardType.String)
                return DyInteger.Get(str.IndexOf(a.GetString()));
            else if (a.TypeId == StandardType.Char)
                return DyInteger.Get(str.IndexOf(a.GetChar()));
            else
                return Err.InvalidType(StandardType.CharName, a.TypeName(ctx)).Set(ctx);
        }

        private DyObject LastIndexOf(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var a = args.TakeOne(DyNil.Instance);
            var str = self.GetString();

            if (a.TypeId == StandardType.String)
                return DyInteger.Get(str.LastIndexOf(a.GetString()));
            else if (a.TypeId == StandardType.Char)
                return DyInteger.Get(str.LastIndexOf(a.GetChar()));
            else
                return Err.InvalidType(StandardType.CharName, a.TypeName(ctx)).Set(ctx);
        }

        private DyObject Split(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var allChars = true;

            for (var i = 0; i < args.Length; i++)
                if (args[i].TypeId != StandardType.Char)
                {
                    allChars = false;
                    break;
                }

            return allChars ? SplitByChars(ctx, self, args) : SplitByStrings(ctx, self, args);
        }

        private DyObject SplitByStrings(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var xs = new string[args.Length];

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].TypeId != StandardType.String)
                    return Err.InvalidType(StandardType.StringName, args[i].TypeName(ctx)).Set(ctx);

                xs[i] = args[i].GetString();
            }

            var arr = self.GetString().Split(xs, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<DyObject>(arr.Length);

            for (var i = 0; i < arr.Length; i++)
                list.Add(new DyString(arr[i]));

            return new DyArray(list);
        }

        private DyObject SplitByChars(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var xs = new char[args.Length];

            for (var i = 0; i < args.Length; i++)
                xs[i] = args[i].GetChar();

            var arr = self.GetString().Split(xs, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<DyObject>(arr.Length);

            for (var i = 0; i < arr.Length; i++)
                list.Add(new DyString(arr[i]));

            return new DyArray(list);
        }

        private DyObject Capitalize(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var str = self.GetString();
            return str.Length == 0 ? DyString.Empty : new DyString(char.ToUpper(str[0]) + str.Substring(1));
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

            if (a.TypeId == StandardType.String)
                return self.GetString().StartsWith(a.GetString()) ? DyBool.True : DyBool.False;

            if (a.TypeId == StandardType.Char)
                return self.GetString().StartsWith(a.GetChar()) ? DyBool.True : DyBool.False;

            return Err.InvalidType(StandardType.StringName, a.TypeName(ctx)).Set(ctx);
        }

        private DyObject EndsWith(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var a = args.TakeOne(DyNil.Instance);

            if (a.TypeId == StandardType.String)
                return self.GetString().EndsWith(a.GetString()) ? DyBool.True : DyBool.False;

            if (a.TypeId == StandardType.Char)
                return self.GetString().EndsWith(a.GetChar()) ? DyBool.True : DyBool.False;

            return Err.InvalidType(StandardType.StringName, a.TypeName(ctx)).Set(ctx);
        }

        private DyObject Substring(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var from = args.TakeOne(DyNil.Instance);
            var to = args.TakeAt(1, null);

            if (from.TypeId != StandardType.Integer)
                return Err.InvalidType(StandardType.IntegerName, from.TypeName(ctx)).Set(ctx);

            if (!ReferenceEquals(to, DyNil.Instance) && to.TypeId != StandardType.Integer)
                return Err.InvalidType(StandardType.IntegerName, to.TypeName(ctx)).Set(ctx);

            var str = self.GetString();
            var i = (int)from.GetInteger();

            if (i < 0 || i >= str.Length)
                return Err.IndexOutOfRange(self.TypeName(ctx), i).Set(ctx);

            if (ReferenceEquals(to, DyNil.Instance))
                return new DyString(str.Substring(i));

            var j = (int)to.GetInteger();

            if (j < 0 || j + i > str.Length)
                return Err.IndexOutOfRange(self.TypeName(ctx), j).Set(ctx);

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
            if (args == null || args.Length == 0)
                return Statics.EmptyChars;

            var chs = new char[args.Length];

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].TypeId != StandardType.Char)
                {
                    ctx.Error = Err.InvalidType(StandardType.CharName, args[i].TypeName(ctx));
                    return Statics.EmptyChars;
                }
                chs[i] = args[i].GetChar();
            }

            return chs;
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            switch (name)
            {
                case Builtins.Len:
                    return DyForeignFunction.Member(name, LenAdapter, -1,Statics.EmptyParameters);
                case "indexOf":
                    return DyForeignFunction.Member(name, IndexOf, -1, new Par("value"));
                case "contains":
                    return DyForeignFunction.Member(name, Contains, -1, new Par("value"));
                case "lastIndexOf":
                    return DyForeignFunction.Member(name, LastIndexOf, -1, new Par("value"));
                case "split":
                    return DyForeignFunction.Member(name, Split, -1, new Par("separators", true));
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
                default:
                    return null;
            }
        }
        #endregion

        #region Statics
        private DyObject Concat(ExecutionContext ctx, DyObject[] args)
        {
            var arr = new string[args.Length];

            for (var i = 0; i < args.Length; i++)
            {
                var a = args[i];

                if (a.TypeId == StandardType.String || a.TypeId == StandardType.Char)
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

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "concat")
                return DyForeignFunction.Static(name, Concat, 0, new Par("values", true));

            return null;
        }
        #endregion
    }
}

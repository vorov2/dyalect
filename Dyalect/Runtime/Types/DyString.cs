using Dyalect.Compiler;
using Dyalect.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId != StandardType.Integer)
                return Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);

            var idx = (int)index.GetInteger();

            if (idx < 0 || idx >= Value.Length)
                return Err.IndexOutOfRange(this.TypeName(ctx), idx).Set(ctx);

            return new DyString(Value[idx].ToString());
        }

        public IEnumerator<DyObject> GetEnumerator() => Value.Select(c => new DyString(c.ToString())).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal sealed class DyStringTypeInfo : DyTypeInfo
    {
        public static readonly DyStringTypeInfo Instance = new DyStringTypeInfo();

        private DyStringTypeInfo() : base(StandardType.String, false)
        {

        }

        public override string TypeName => StandardType.StringName;

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var str1 = left.TypeId == StandardType.String ? left.GetString() : left.ToString(ctx).Value;
            var str2 = right.TypeId == StandardType.String ? right.GetString() : right.ToString(ctx).Value;
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

            if (to != null && to.TypeId != StandardType.Integer)
                return Err.InvalidType(StandardType.IntegerName, to.TypeName(ctx)).Set(ctx);

            var str = self.GetString();
            var i = (int)from.GetInteger();

            if (i < 0 || i >= str.Length)
                return Err.IndexOutOfRange(self.TypeName(ctx), i).Set(ctx);

            if (to == null)
                return new DyString(str.Substring(i));

            var j = (int)to.GetInteger();

            if (j < 0 || j + i > str.Length)
                return Err.IndexOutOfRange(self.TypeName(ctx), j).Set(ctx);

            return new DyString(self.GetString().Substring(i, j));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == Builtins.Len)
                return DyForeignFunction.Create(name, LenAdapter);

            if (name == "indexOf")
                return DyForeignFunction.Create(name, IndexOf);

            if (name == "lastIndexOf")
                return DyForeignFunction.Create(name, LastIndexOf);

            if (name == "split")
                return DyForeignFunction.Create(name, Split);

            if (name == "upper")
                return DyForeignFunction.Create(name, Upper);

            if (name == "lower")
                return DyForeignFunction.Create(name, Lower);

            if (name == "startsWith")
                return DyForeignFunction.Create(name, StartsWith);

            if (name == "endsWith")
                return DyForeignFunction.Create(name, EndsWith);

            if (name == "sub")
                return DyForeignFunction.Create(name, Substring);

            return null;
        }
        #endregion
    }
}

using Dyalect.Debug;
using Dyalect.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyCharTypeInfo : DyTypeInfo
    {
        public DyCharTypeInfo() : base(DyType.Char) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Add | SupportedOperations.Sub
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte;

        public override string TypeName => DyTypeNames.Char;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString(arg.GetString());

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return new DyChar((char)(left.GetChar() + right.GetInteger()));

            if (right.TypeId is DyType.Char)
                return new DyString(left.GetString() + right.GetString());

            if (right.TypeId is DyType.String)
                return ctx.RuntimeContext.Types[DyType.String].Add(ctx, left, right);

            return ctx.InvalidType(right);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return new DyChar((char)(left.GetChar() - right.GetInteger()));

            if (right.TypeId is DyType.Char)
                return DyInteger.Get(left.GetChar() - right.GetChar());

            return ctx.InvalidType(right);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar() == right.GetChar() ? DyBool.True : DyBool.False;

            if (right.TypeId is DyType.String)
            {
                var str = right.GetString();
                return str.Length == 1 && left.GetChar() == str[0] ? DyBool.True : DyBool.False;
            }

            return base.EqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar() != right.GetChar() ? DyBool.True : DyBool.False;

            if (right.TypeId is DyType.String)
            {
                var str = right.GetString();
                return str.Length != 1 || left.GetChar() != str[0] ? DyBool.True : DyBool.False;
            }

            return base.NeqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar().CompareTo(right.GetChar()) > 0 ? DyBool.True : DyBool.False;

            if (right.TypeId == DyType.String)
                return left.GetString().CompareTo(right.GetString()) > 0 ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar().CompareTo(right.GetChar()) < 0 ? DyBool.True : DyBool.False;

            if (right.TypeId == DyType.String)
                return left.GetString().CompareTo(right.GetString()) < 0 ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }
        #endregion

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "isLower" => Func.Member(name, (_, c) => (DyBool)char.IsLower(c.GetChar())),
                "isUpper" => Func.Member(name, (_, c) => (DyBool)char.IsUpper(c.GetChar())),
                "isControl" => Func.Member(name, (_, c) => (DyBool)char.IsControl(c.GetChar())),
                "isDigit" => Func.Member(name, (_, c) => (DyBool)char.IsDigit(c.GetChar())),
                "isLetter" => Func.Member(name, (_, c) => (DyBool)char.IsLetter(c.GetChar())),
                "isLetterOrDigit" => Func.Member(name, (_, c) => (DyBool)char.IsLetterOrDigit(c.GetChar())),
                "isWhiteSpace" => Func.Member(name, (_, c) => (DyBool)char.IsWhiteSpace(c.GetChar())),
                "lower" => Func.Member(name, (_, c) => new DyChar(char.ToLower(c.GetChar()))),
                "upper" => Func.Member(name, (_, c) => new DyChar(char.ToUpper(c.GetChar()))),
                "order" => Func.Member(name, (_, c) => DyInteger.Get(c.GetChar())),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };

        private DyObject CreateChar(ExecutionContext ctx, DyObject obj)
        {
            if (obj.TypeId is DyType.Char)
                return obj;

            if (obj.TypeId is DyType.String)
            {
                var str = obj.ToString();
                return str.Length > 0 ? new(str[0]) : DyChar.Empty;
            }

            if (obj.TypeId is DyType.Integer)
                return new DyChar((char)obj.GetInteger());

            if (obj.TypeId is DyType.Float)
                return new DyChar((char)obj.GetFloat());

            return ctx.InvalidType(obj);
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "max" => Func.Static(name, _ => DyChar.Max),
                "min" => Func.Static(name, _ => DyChar.Min),
                "default" => Func.Static(name, _ => DyChar.Empty),
                "Char" => Func.Static(name, CreateChar, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}

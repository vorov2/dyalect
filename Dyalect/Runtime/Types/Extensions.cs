using Dyalect.Compiler;
using Dyalect.Parser;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dyalect.Runtime.Types;

public static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTrue(this DyObject self) =>
        !ReferenceEquals(self, False) && !ReferenceEquals(self, Nil);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFalse(this DyObject self) =>
        ReferenceEquals(self, False) || ReferenceEquals(self, Nil);

    //Doesn't generate an error if type check fails
    public static bool Is(this DyObject self, int typeId) => self.TypeId == typeId;

    //Generates error if type check fails
    public static bool Is(this DyObject self, ExecutionContext ctx, int typeId)
    {
        if (self.TypeId != typeId)
        {
            ctx.InvalidType(typeId, self);
            return false;
        }

        return true;
    }

    public static DyTypeInfo GetTypeInfo(this DyObject self, ExecutionContext ctx) => ctx.RuntimeContext.Types[self.TypeId];

    internal static DyVariant ToError(this DyObject self)
    {
        if (self is DyVariant v)
            return v;

        return new DyVariant(DyError.Failure, self);
    }

    //Returns a function encapsulated by an iterator, accepts: an iterator, a function
    //which is already an iterator function, a function that might return an iterator function,
    //an object that implements built-in "Iterator" method, an object that implements
    //built-in "Call" method (which supposedly returns an iterator, acts in same way as
    //"Iterator" method
    internal static DyFunction? GetIterator(this DyObject self, ExecutionContext ctx)
    {
        if (self.TypeId is Dy.Iterator)
            return ((DyIterator)self).GetIteratorFunction();

        if (self.TypeId is Dy.Function)
        {
            if (self is DyNativeIteratorFunction)
                return (DyFunction)self;

            var obj = ((DyFunction)self).Call(ctx);
            var ret = obj as DyFunction;

            if (ret is null)
                ctx.InvalidType();

            return ret;
        }

        var type = ctx.RuntimeContext.Types[self.TypeId];

        if (type.HasInstanceMember(self, Builtins.Iterate, ctx))
        {
            var inst = type.GetInstanceMember(self, Builtins.Iterate, ctx);
            return inst.GetIterator(ctx);
        }
        else
        {
            var member = type.GetInstanceMember(self, Builtins.Call, ctx);

            if (ctx.HasErrors)
            {
                ctx.Error = null;
                ctx.OperationNotSupported(Builtins.Iterate, self);
                return null;
            }

            return member.GetIterator(ctx);
        }
    }

    //Calls a native implementation of "ToString" for a given object with an exception
    //for string and TypeInfo (no native implementation of ToString for TypeInfo)
    public static DyString ToString(this DyObject self, ExecutionContext ctx, DyString? format = null)
    {
        if (self is DyString str)
            return str;
        else if (self is DyTypeInfo ti)
            return new DyString(ti.ReflectedTypeName);
        else
        {
            var t = ctx.RuntimeContext.Types[self.TypeId];
            var ret = format is null ? t.ToString(ctx, self) : t.ToStringWithFormat(ctx, self, format);

            if (ReferenceEquals(ctx.Error, DyVariant.Eta))
                ret = format is null ? ctx.InvokeEtaFunction() : ctx.InvokeEtaFunction(format);

            return ret is DyString s ? s : new DyString(ret.ToString());
        }
    }

    public static bool Equals(this DyObject left, DyObject right, ExecutionContext ctx)
    {
        var type = ctx.RuntimeContext.Types[left.TypeId];
        var ret = type.Eq(ctx, left, right);

        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
            ret = ctx.InvokeEtaFunction(right);

        return ret.IsTrue();
    }

    public static bool NotEquals(this DyObject left, DyObject right, ExecutionContext ctx)
    {
        var type = ctx.RuntimeContext.Types[left.TypeId];
        var ret = type.Neq(ctx, left, right);

        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
            ret = ctx.InvokeEtaFunction(right);

        return ret.IsTrue();
    }

    public static bool Lesser(this DyObject left, DyObject right, ExecutionContext ctx)
    {
        var type = ctx.RuntimeContext.Types[left.TypeId];
        var ret = type.Lt(ctx, left, right);

        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
            ret = ctx.InvokeEtaFunction(right);

        return ret.IsTrue();
    }

    public static bool LesserOrEquals(this DyObject left, DyObject right, ExecutionContext ctx)
    {
        var type = ctx.RuntimeContext.Types[left.TypeId];
        var ret = type.Lte(ctx, left, right);

        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
            ret = ctx.InvokeEtaFunction(right);

        return ret.IsTrue();
    }

    public static bool Greater(this DyObject left, DyObject right, ExecutionContext ctx)
    {
        var type = ctx.RuntimeContext.Types[left.TypeId];
        var ret = type.Gt(ctx, left, right);

        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
            ret = ctx.InvokeEtaFunction(right);

        return ret.IsTrue();
    }

    public static bool GreaterOrEquals(this DyObject left, DyObject right, ExecutionContext ctx)
    {
        var type = ctx.RuntimeContext.Types[left.TypeId];
        var ret = type.Gte(ctx, left, right);

        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
            ret = ctx.InvokeEtaFunction(right);

        return ret.IsTrue();
    }

    public static DyObject Concat(this DyObject left, DyObject right, ExecutionContext ctx)
    {
        var self = left.ToString(ctx);
        var type = ctx.RuntimeContext.String;
        var ret = type.Add(ctx, self, right);

        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
            ret = ctx.InvokeEtaFunction(right);

        return ret;
    }

    public static DyObject Add(this DyObject left, DyObject right, ExecutionContext ctx)
    {
        var type = ctx.RuntimeContext.Types[left.TypeId];
        var ret = type.Add(ctx, left, right);

        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
            ret = ctx.InvokeEtaFunction(right);

        return ret;
    }

    public static DyObject Negate(this DyObject self, ExecutionContext ctx)
    {
        var type = ctx.RuntimeContext.Types[self.TypeId];
        var ret = type.Neg(ctx, self);

        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
            ret = ctx.InvokeEtaFunction();

        return ret;
    }

    //Returns a function if an objects is a function or implements "Call" method
    public static DyFunction? ToFunction(this DyObject self, ExecutionContext ctx)
    {
        if (self is DyFunction func)
            return func;

        if (self.Is(Dy.TypeInfo))
        {
            var ti = (DyTypeInfo)self;

            if (ti.TryGetStaticMember(ctx, ti.ReflectedTypeName, out var value))
                return value as DyFunction;
        }
        else
        {
            var typ = ctx.RuntimeContext.Types[self.TypeId];

            if (typ.TryGetInstanceMember(ctx, self, Builtins.Call, out var value))
                return value as DyFunction;
        }

        ctx.InvalidType(Dy.Function, self);
        return default;
    }

    //Invokes a function obtained from "ToFunction"
    public static DyObject Invoke(this DyObject self, ExecutionContext ctx, params DyObject[] args)
    {
        var func = self.ToFunction(ctx);

        if (func is null)
            return Nil;

        return func.Call(ctx, args);
    }

    private static readonly char[] invalidChars = new[] { ' ', '\t', '\n', '\r', '\'', '"' };

    public static string ToLiteral(this DyObject obj, ExecutionContext ctx)
    {
        if (obj.TypeId is Dy.Char)
            return StringUtil.Escape(obj.ToString(ctx).ToString(), "'");
        else if (obj.TypeId is Dy.String)
            return StringUtil.Escape(obj.ToString(ctx).ToString());
        else
            return obj.ToString(ctx).ToString();
    }

    public static string ToLiteral(this IEnumerable<DyObject> seq, ExecutionContext ctx)
    {
        var c = 0;
        var sb = new StringBuilder();

        foreach (DyObject o in seq)
        {
            if (c > 0)
                sb.Append(", ");

            if (o is DyLabel lab)
            {
                if (lab.Mutable)
                    sb.Append("var ");

                foreach (var ta in lab.EnumerateAnnotations())
                {
                    sb.Append(ta.ToString());
                    sb.Append(' ');
                }

                if (!char.IsLower(lab.Label[0]) || lab.Label.IndexOfAny(invalidChars) != -1)
                    sb.Append(StringUtil.Escape(lab.Label));
                else
                    sb.Append(lab.Label);

                sb.Append(':');
                sb.Append(' ');
                sb.Append(lab.Value.ToLiteral(ctx));
            }
            else
                sb.Append(o.ToLiteral(ctx));

            c++;
        }

        return sb.ToString();
    }
}

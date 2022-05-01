using Dyalect.Compiler;
using System.Runtime.CompilerServices;
namespace Dyalect.Runtime.Types;

public static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsTrue(this DyObject self) =>
        !ReferenceEquals(self, False) && !ReferenceEquals(self, Nil);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsFalse(this DyObject self) =>
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
        if (self.TypeId == Dy.Iterator)
            return ((DyIterator)self).GetIteratorFunction();

        if (self.TypeId == Dy.Function)
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

        if (type.HasInstanceMember(self, Builtins.Iterator, ctx))
        {
            var inst = type.GetInstanceMember(self, Builtins.Iterator, ctx);
            return inst.GetIterator(ctx);
        }
        else
        {
            var member = type.GetInstanceMember(self, Builtins.Call, ctx);

            if (ctx.HasErrors)
            {
                ctx.Error = null;
                ctx.OperationNotSupported(Builtins.Iterator, self);
                return null;
            }

            return member.GetIterator(ctx);
        }
    }

    //Calls a native implementation of "ToString" for a given object with an exception
    //for string and TypeInfo (no native implementation of ToString for TypeInfo)
    public static DyString ToString(this DyObject self, ExecutionContext ctx)
    {
        if (self.Is(Dy.String))
            return (DyString)self;
        else if (self.Is(Dy.TypeInfo))
            return new DyString(self.ToString());
        else
        {
            var ret = ctx.RuntimeContext.Types[self.TypeId].ToString(ctx, self);

            if (ReferenceEquals(ctx.Error, DyVariant.Eta))
                ret = ctx.InvokeEtaFunction();

            return ret as DyString ?? DyString.Empty;
        }
    }

    public static DyString ToLiteral(this DyObject self, ExecutionContext ctx)
    {
        var type = ctx.RuntimeContext.Types[self.TypeId];
        var ret = type.ToLiteral(ctx, self);

        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
            ret = ctx.InvokeEtaFunction();

        return ret as DyString ?? DyString.Empty;
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
        var type = ctx.RuntimeContext.String;
        var ret = type.Add(ctx, left, right);

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
}

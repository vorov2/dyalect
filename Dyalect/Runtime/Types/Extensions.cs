using Dyalect.Compiler;
using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types;

public static class Extensions
{
    internal static bool IsTrue(this DyObject self) =>
        !ReferenceEquals(self, False) && !ReferenceEquals(self, Nil);

    internal static bool IsFalse(this DyObject self) =>
        ReferenceEquals(self, False) || ReferenceEquals(self, Nil);

    public static bool Is(this DyObject self, int typeId) => self.TypeId == typeId;

    public static bool Is(this DyObject self, ExecutionContext ctx, int typeId)
    {
        if (self.TypeId != typeId)
        {
            ctx.InvalidType(typeId, self);
            return false;
        }

        return true;
    }

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

    public static DyString ToString(this DyObject self, ExecutionContext ctx) =>
        (DyString)ctx.RuntimeContext.Types[self.TypeId].ToString(ctx, self);

    public static DyString ToLiteral(this DyObject self, ExecutionContext ctx) =>
        (DyString)ctx.RuntimeContext.Types[self.TypeId].ToLiteral(ctx, self);

    public static bool Equals(this DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.RuntimeContext.Types[left.TypeId].Eq(ctx, left, right).IsTrue();

    public static bool NotEquals(this DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.RuntimeContext.Types[left.TypeId].Neq(ctx, left, right).IsTrue();

    public static bool Lesser(this DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.RuntimeContext.Types[left.TypeId].Lt(ctx, left, right).IsTrue();

    public static bool Greater(this DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.RuntimeContext.Types[left.TypeId].Gt(ctx, left, right).IsTrue();

    public static DyObject Negate(this DyObject self, ExecutionContext ctx) =>
        ctx.RuntimeContext.Types[self.TypeId].Neg(ctx, self);

    public static DyFunction? ToFunction(this DyObject self, ExecutionContext ctx)
    {
        if (self is DyFunction func)
            return func;

        var typ = ctx.RuntimeContext.Types[self.TypeId];

        if (typ.HasInstanceMember(self, Builtins.Call, ctx))
            return typ.GetInstanceMember(self, Builtins.Call, ctx) as DyFunction;

        ctx.InvalidType(Dy.Function, self);
        return null;
    }

    public static DyObject Invoke(this DyObject self, ExecutionContext ctx, params DyObject[] args)
    {
        if (self is DyFunction func)
            return func.Call(ctx, args);

        var functor = ctx.RuntimeContext.Types[self.TypeId].GetInstanceMember(self, Builtins.Call, ctx);

        if (ctx.HasErrors)
            return DyNil.Instance;

        if (functor.TypeId != Dy.Function)
            return ctx.InvalidType(functor);

        return functor.Invoke(ctx, args);
    }
}

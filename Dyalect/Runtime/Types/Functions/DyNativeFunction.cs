﻿using Dyalect.Debug;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types;

internal class DyNativeFunction : DyFunction
{
    private readonly FunSym? sym;
    internal readonly FastList<DyObject[]> Captures;
    internal DyObject[]? Locals;
    internal Stack<CatchMark> CatchMarks = null!;
    internal int PreviousOffset;
    internal readonly int UnitId;
    internal readonly int FunctionId;

    public override string FunctionName => sym?.Name != null ? sym.Name : DefaultName;

    public override bool IsExternal => false;

    internal DyNativeFunction(FunSym? sym, int unitId, int funcId, FastList<DyObject[]> captures, int varArgIndex) :
        base(sym?.Parameters ?? Array.Empty<Par>(), varArgIndex)
    {
        this.sym = sym;
        UnitId = unitId;
        FunctionId = funcId;
        Captures = captures;
    }

    public static DyNativeFunction Create(FunSym sym, int unitId, int funcId, FastList<DyObject[]> captures, DyObject[] locals, int varArgIndex = -1)
    {
        var vars = new FastList<DyObject[]>(captures) { locals };
        return new(sym, unitId, funcId, vars, varArgIndex);
    }

    internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg) =>
        new DyNativeFunction(sym, UnitId, FunctionId, Captures, VarArgIndex)
        {
            Self = arg
        };

    protected override DyObject BindOrRun(ExecutionContext ctx, DyObject arg)
    {
        if (Auto)
        {
            try
            {
                var size = GetMemoryCells(ctx);
                var locals = size == 0 ? Array.Empty<DyObject>() : new DyObject[size];
                ctx.CallStack.Push(Caller.External);
                return DyMachine.ExecuteWithData((DyNativeFunction)BindToInstance(ctx, arg), locals, ctx);
            }
            catch (DyCodeException ex)
            {
                ctx.Error = ex.Error;
                return Nil;
            }
        }

        return BindToInstance(ctx, arg);
    }

    protected override DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] locals)
    {
        ctx.CallStack.Push(Caller.External);
        return DyMachine.ExecuteWithData(this, locals, ctx);
    }

    internal override int GetMemoryCells(ExecutionContext ctx) => ctx.RuntimeContext.Layouts[UnitId][FunctionId].Size;

    internal override DyObject[] CreateLocals(ExecutionContext ctx)
    {
        var size = ctx.RuntimeContext.Layouts[UnitId][FunctionId].Size;
        return size == 0 ? Array.Empty<DyObject>() : new DyObject[size];
    }

    protected override bool Equals(DyFunction func) => 
           func is DyNativeFunction m && m.UnitId == UnitId && m.FunctionId == FunctionId 
        && IsSameInstance(this, func);
}

﻿using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Compiler;

internal sealed class CodeWriter
{
    private readonly FastList<Op> ops;
    private readonly Stack<StackSize> locals;
    private readonly Unit frame;
    private readonly FastList<int> labels;
    private readonly FastList<int> fixups;
    private readonly Dictionary<string, int> strings;
    private readonly Dictionary<DyObject, int> objects;

    private sealed class StackSize
    {
        internal int Counter;
        internal int Max;
    }

    private CodeWriter(CodeWriter cw, Unit unit)
    {
        ops = unit.Ops;
        locals = new(cw.locals.ToArray());
        labels = new(cw.labels.ToArray());
        fixups = new(cw.fixups.ToArray());
        strings = cw.strings;
        objects = cw.objects;
        frame = unit;
    }

    public CodeWriter(Unit frame)
    {
        this.frame = frame;
        ops = frame.Ops;
        strings = new();
        objects = new();
        locals = new();
        labels = new();
        fixups = new();
    }

    public CodeWriter Clone(Unit frame) => new(this, frame);

    public void CompileOpList()
    {
        foreach (var i in fixups)
            ops[i].Data = labels[ops[i].Data];

        fixups.Clear();
        labels.Clear();
    }

    public Label DefineLabel()
    {
        var lab = new Label(labels.Count);
        labels.Add(Label.EmptyLabel);
        return lab;
    }

    public void MarkLabel(Label label) =>
        labels[label.GetIndex()] = ops.Count;

    public void Emit(OpCode op, Label label)
    {
        if (!label.IsEmpty())
        {
            fixups.Add(ops.Count);
            Emit(new(op, label.GetIndex()));
        }
        else
            Emit(new(op, 0));
    }

    private void Emit(Op op) => Emit(op, op.Code.GetStack());

    private void Emit(Op op, int size)
    {
        var ss = locals.Peek();
        ss.Counter += size;

        if (ss.Counter < 0)
            ss.Counter = 0;

        if (ss.Counter > ss.Max)
            ss.Max = ss.Counter;

        ops.Add(op);
    }

    public void StartFrame() => locals.Push(new());

    public int FinishFrame() => locals.Pop().Max;

    public int Offset => ops.Count;

    private int IndexString(string val)
    {
        if (!strings.TryGetValue(val, out var idx))
        {
            frame.Strings.Add(new(val));
            idx = frame.Strings.Count - 1;
            strings.Add(val, idx);
        }

        return idx;
    }

    private int IndexObject(DyObject val)
    {
        if (!objects.TryGetValue(val, out var idx))
        {
            frame.Objects.Add(val);
            idx = frame.Objects.Count - 1;
            objects.Add(val, idx);
        }

        return idx;
    }

    public void Push(double val)
    {
        if (val is 0D)
            Emit(Op.PushR8_0);
        else if (val is 1D)
            Emit(Op.PushR8_1);
        else
            Push(new DyFloat(val));
    }

    public void Push(long val)
    {
        if (val == 0L)
            Emit(Op.PushI8_0);
        else if (val == 1L)
            Emit(Op.PushI8_1);
        else
            Push(new DyInteger(val));
    }

    public void Push(bool val)
    {
        if (val)
            Emit(Op.PushI1_1);
        else
            Emit(Op.PushI1_0);
    }

    public void Push(string val) => Push(new DyString(val));

    public void Push(char val) => Push(new DyChar(val));

    public void Push(DyObject val) => Emit(new(OpCode.PushObj, IndexObject(val)));

    public void PushVar(ScopeVar sv)
    {
        if ((sv.Data & VarFlags.External) == VarFlags.External)
            Emit(new(OpCode.Pushext, sv.Address));
        else if ((sv.Address & byte.MaxValue) == 0)
            Emit(new(OpCode.Pushloc, sv.Address >> 8));
        else
            Emit(new(OpCode.Pushvar, sv.Address));
    }

    public void PopVar(int address)
    {
        if ((address & byte.MaxValue) == 0)
            Emit(new(OpCode.Poploc, address >> 8));
        else
            Emit(new(OpCode.Popvar, address));
    }

    public void Tag(string tag)
    {
        var idx = IndexString(tag);
        Emit(new(OpCode.Tag, idx));
    }

    public void CallNullaryMember(string name)
    {
        GetMember(name);
        CallNullaryFunction();
    }

    public void CallNullaryFunction()
    {
        FunPrep(0);
        FunCall(0);
    }

    public void StdCall(int args)
    {
        if (args == 0)
            Emit(Op.StdCall_0);
        else if (args == 1)
            Emit(Op.StdCall_1);
        else
            Emit(new(OpCode.StdCall, args), -args);
    }

    public void GetPrivate(string name) => Emit(new(OpCode.GetPriv, IndexString(name)));
    public void SetPrivate(string name) => Emit(new(OpCode.SetPriv, IndexString(name)));

    public void Contains(string field) => Emit(new(OpCode.Contains, IndexObject(new DyString(field))));

    public void SetMember(string member) => Emit(new(OpCode.SetMember, IndexString(member)));
    public void SetMemberS(string member) => Emit(new(OpCode.SetMemberS, IndexString(member)));

    public void Type(int code) => Emit(new(OpCode.Type, code));

    public void FunPrep(int argCount) => Emit(new(OpCode.FunPrep, argCount));
    public void FunArgIx(int index) => Emit(new(OpCode.FunArgIx, index));
    public void FunArgNm(string name) => Emit(new(OpCode.FunArgNm, IndexString(name)));
    public void FunCall(int argCount) => Emit(new(OpCode.FunCall, argCount));
    public void CtorCheck(string ctor) => Emit(new(OpCode.CtorCheck, IndexString(ctor)));

    public void NewArgs(int len) => Emit(new(OpCode.NewArgs, len), -len + 1);
    public void NewDict(int len) => Emit(new(OpCode.NewDict, len), -len + 1);
    public void NewTuple(int len) => Emit(new(OpCode.NewTuple, len), -len + 1);
    public void NewFun(int funHandle) => Emit(new(OpCode.NewFun, funHandle));
    public void NewFunV(int funHandle) => Emit(new(OpCode.NewFunV, funHandle));
    public void FunAttr(int attr) => Emit(new(OpCode.FunAttr, attr));
    public void NewIter(int funHandle) => Emit(new(OpCode.NewIter, funHandle));
    public void Br(Label lab) => Emit(OpCode.Br, lab);
    public void Brtrue(Label lab) => Emit(OpCode.Brtrue, lab);
    public void Brfalse(Label lab) => Emit(OpCode.Brfalse, lab);
    public void Brterm(Label lab) => Emit(OpCode.Brterm, lab);
    public void Briter(Label lab) => Emit(OpCode.Briter, lab);
    public void GetMember(string name) => Emit(new(OpCode.GetMember, IndexString(name)));
    public void HasMember(string name) => Emit(new(OpCode.HasMember, IndexString(name)));
    public void RunMod(int code) => Emit(new(OpCode.RunMod, code));
    public void RgDI(string value) => Emit(new(OpCode.RgDI, IndexString(value)));
    public void RgDI(int data) => Emit(new(OpCode.RgDI, data));
    public void RgFI(int data) => Emit(new(OpCode.RgFI, data));
    public void Start(Label lab) => Emit(OpCode.Start, lab);
    public void NewObj(string ctor) => Emit(new(OpCode.NewObj, IndexString(ctor)));
    public void NewType(string name) => Emit(new(OpCode.NewType, IndexString(name)));

    public void GetIter() => Emit(Op.GetIter);
    public void End() => Emit(Op.End);
    public void Yield() => Emit(Op.Yield);
    public void Str() => Emit(Op.Str);
    public void Get() => Emit(Op.Get);
    public void Set() => Emit(Op.Set);
    public void This() => Emit(Op.This);
    public void Type() => Emit(Op.Type);
    public void PushNil() => Emit(Op.PushNil);
    public void PushNilT() => Emit(Op.PushNilT);
    public void Nop() => Emit(Op.Nop);
    public void Pop() => Emit(Op.Pop);
    public void Shl() => Emit(Op.Shl);
    public void Shr() => Emit(Op.Shr);
    public void And() => Emit(Op.And);
    public void Or() => Emit(Op.Or);
    public void Xor() => Emit(Op.Xor);
    public void Add() => Emit(Op.Add);
    public void Sub() => Emit(Op.Sub);
    public void Mul() => Emit(Op.Mul);
    public void Div() => Emit(Op.Div);
    public void Rem() => Emit(Op.Rem);
    public void Neg() => Emit(Op.Neg);
    public void Plus() => Emit(Op.Plus);
    public void Not() => Emit(Op.Not);
    public void BitNot() => Emit(Op.BitNot);
    public void Len() => Emit(Op.Len);
    public void Gt() => Emit(Op.Gt);
    public void Lt() => Emit(Op.Lt);
    public void Eq() => Emit(Op.Eq);
    public void NotEq() => Emit(Op.NotEq);
    public void GtEq() => Emit(Op.GtEq);
    public void LtEq() => Emit(Op.LtEq);
    public void Ret() => Emit(Op.Ret);
    public void Dup() => Emit(Op.Dup);
    public void Fail() => Emit(Op.Fail);
    public void Term() => Emit(Op.Term);
    public void IsNull() => Emit(Op.IsNull);
    public void Mut() => Emit(Op.Mut);
    public void Annot() => Emit(Op.Annot);
    public void TypeCheck() => Emit(Op.TypeCheck);
    public void NewCast() => Emit(Op.NewCast);
    public void Cast() => Emit(Op.Cast);
    public void Mixin() => Emit(Op.Mixin);
    public void Debug() => Emit(Op.Debug);
}

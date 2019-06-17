using System.Collections.Generic;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Compiler
{
    internal sealed class CodeWriter
    {
        private List<Op> ops;
        private Stack<StackSize> locals;
        private Unit frame;
        private List<int> labels;
        private List<int> fixups;
        private Dictionary<string, int> strings;
        private Dictionary<double, int> floats;
        private Dictionary<long, int> integers;
        private Dictionary<char, int> chars;

        private sealed class StackSize
        {
            internal int Counter;
            internal int Max;
        }

        private CodeWriter(CodeWriter cw, Unit unit)
        {
            this.ops = unit.Ops;
            this.locals = new Stack<StackSize>(cw.locals.ToArray());
            this.labels = new List<int>(cw.labels.ToArray());
            this.fixups = new List<int>(cw.fixups.ToArray());
            this.strings = cw.strings;
            this.floats = cw.floats;
            this.integers = cw.integers;
            this.chars = cw.chars;
            this.frame = unit;
        }

        public CodeWriter(Unit frame)
        {
            this.frame = frame;
            this.ops = frame.Ops;
            strings = new Dictionary<string, int>();
            integers = new Dictionary<long, int>();
            floats = new Dictionary<double, int>();
            chars = new Dictionary<char, int>();
            locals = new Stack<StackSize>();
            labels = new List<int>();
            fixups = new List<int>();
        }

        public CodeWriter Clone(Unit frame)
        {
            return new CodeWriter(this, frame);
        }

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

        public void MarkLabel(Label label)
        {
            labels[label.GetIndex()] = ops.Count;
        }

        public void Emit(OpCode op, Label label)
        {
            if (!label.IsEmpty())
            {
                fixups.Add(ops.Count);
                Emit(new Op(op, label.GetIndex()));
            }
            else
                Emit(new Op(op, 0));
        }

        private void Emit(Op op)
        {
            var size = op.Code.GetStack();
            Emit(op, size);
        }

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

        public void StartFrame()
        {
            locals.Push(new StackSize());
        }

        public int FinishFrame()
        {
            var ret = locals.Pop();
            return ret.Max;
        }

        public int Offset => ops.Count;

        public int IndexString(string val)
        {
            if (!strings.TryGetValue(val, out var idx))
            {
                frame.IndexedStrings.Add(new DyString(val));
                idx = frame.IndexedStrings.Count - 1;
                strings.Add(val, idx);
            }

            return idx;
        }

        private int IndexFloat(double val)
        {
            if (!floats.TryGetValue(val, out var idx))
            {
                frame.IndexedFloats.Add(new DyFloat(val));
                idx = frame.IndexedFloats.Count - 1;
                floats.Add(val, idx);
            }

            return idx;
        }

        private int IndexInteger(long val)
        {
            if (!integers.TryGetValue(val, out var idx))
            {
                frame.IndexedIntegers.Add(DyInteger.Get(val));
                idx = frame.IndexedIntegers.Count - 1;
                integers.Add(val, idx);
            }

            return idx;
        }

        private int IndexChar(char val)
        {
            if (!chars.TryGetValue(val, out var idx))
            {
                frame.IndexedChars.Add(new DyChar(val));
                idx = frame.IndexedChars.Count - 1;
                chars.Add(val, idx);
            }

            return idx;
        }

        public void Push(string val)
        {
            Emit(new Op(OpCode.PushStr, IndexString(val)));
        }

        public void Push(double val)
        {
            if (val == 0D)
                Emit(Op.PushR8_0);
            else
                Emit(new Op(OpCode.PushR8, IndexFloat(val)));
        }

        public void Push(long val)
        {
            if (val == 0L)
                Emit(Op.PushI8_0);
            else if (val == 1L)
                Emit(Op.PushI8_1);
            else
                Emit(new Op(OpCode.PushI8, IndexInteger(val)));
        }

        public void Push(bool val)
        {
            if (val)
                Emit(Op.PushI1_1);
            else
                Emit(Op.PushI1_0);
        }

        public void Push(char val)
        {
            Emit(new Op(OpCode.PushCh, IndexChar(val)));
        }

        public void PushVar(ScopeVar sv)
        {
            if ((sv.Data & VarFlags.External) == VarFlags.External)
                Emit(new Op(OpCode.Pushext, sv.Address));
            else if ((sv.Address & byte.MaxValue) == 0)
                Emit(new Op(OpCode.Pushloc, sv.Address >> 8));
            else
                Emit(new Op(OpCode.Pushvar, sv.Address));
        }

        public void PopVar(int address)
        {
            if ((address & byte.MaxValue) == 0)
                Emit(new Op(OpCode.Poploc, address >> 8));
            else
                Emit(new Op(OpCode.Popvar, address));
        }

        public void Tag(string tag)
        {
            var idx = IndexString(tag);
            Emit(new Op(OpCode.Tag, idx));
        }

        public void SetMember(TypeHandle type)
        {
            if (type.IsStandard)
                Emit(new Op(OpCode.SetMemberT, type.TypeId));
            else
                Emit(new Op(OpCode.SetMember, type.TypeId));
        }

        public void SetMemberS(TypeHandle type)
        {
            if (type.IsStandard)
                Emit(new Op(OpCode.SetMemberST, type.TypeId));
            else
                Emit(new Op(OpCode.SetMemberS, type.TypeId));
        }

        public void TypeCheck(TypeHandle type)
        {
            if (type.IsStandard)
                Emit(new Op(OpCode.TypeCheckT, type.TypeId));
            else
                Emit(new Op(OpCode.TypeCheck, type.TypeId));
        }

        public void Type(TypeHandle type)
        {
            if (type.IsStandard)
                Emit(new Op(OpCode.TypeST, type.TypeId));
            else
                Emit(new Op(OpCode.TypeS, type.TypeId));
        }

        public void FunPrep(int argCount) => Emit(new Op(OpCode.FunPrep, argCount));
        public void FunArgIx(int index) => Emit(new Op(OpCode.FunArgIx, index));
        public void FunArgNm(string name) => Emit(new Op(OpCode.FunArgNm, IndexString(name)));
        public void FunCall(int argCount) => Emit(new Op(OpCode.FunCall, argCount));

        public void NewTuple(int len) => Emit(new Op(OpCode.NewTuple, len), -len + 1);
        public void NewFun(int funHandle) => Emit(new Op(OpCode.NewFun, funHandle));
        public void NewFunV(int funHandle) => Emit(new Op(OpCode.NewFunV, funHandle));
        public void NewFunA(int funHandle) => Emit(new Op(OpCode.NewFunA, funHandle));
        public void NewIter(int funHandle) => Emit(new Op(OpCode.NewIter, funHandle));
        public void Br(Label lab) => Emit(OpCode.Br, lab);
        public void Brtrue(Label lab) => Emit(OpCode.Brtrue, lab);
        public void Brfalse(Label lab) => Emit(OpCode.Brfalse, lab);
        public void Brterm(Label lab) => Emit(OpCode.Brterm, lab);
        public void Briter(Label lab) => Emit(OpCode.Briter, lab);
        public void GetMember(int nameId) => Emit(new Op(OpCode.GetMember, nameId));
        public void HasMember(int nameId) => Emit(new Op(OpCode.HasMember, nameId));
        public void RunMod(int code) => Emit(new Op(OpCode.RunMod, code));
        public void Aux(int data) => Emit(new Op(OpCode.Aux, data));
        public void Get(int index) => Emit(new Op(OpCode.GetIx, index));
        public void Set(int index) => Emit(new Op(OpCode.SetIx, index));
        public void HasField(string field) => Emit(new Op(OpCode.HasField, IndexString(field)));
        public void Start(Label lab) => Emit(OpCode.Start, lab);
        public void Fail(DyErrorCode code) => Emit(new Op(OpCode.FailSys, (int)code));
        public void NewType(int typeId) => Emit(new Op(OpCode.NewType, typeId));

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
        public void SyncPoint() => Emit(Op.SyncPoint);
        public void Fail() => Emit(Op.Fail);
        public void Term() => Emit(Op.Term);
    }
}
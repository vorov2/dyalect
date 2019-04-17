using System.Collections.Generic;
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
            this.frame = unit;
        }

        public CodeWriter(Unit frame)
        {
            this.frame = frame;
            this.ops = frame.Ops;
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

            if (ss.Counter > ss.Max)
                ss.Max = ss.Counter;

            ops.Add(op);
        }

        public void StartFrame(int init)
        {
            locals.Push(new StackSize { Counter = init, Max = init });
        }

        public int FinishFrame()
        {
            var ret = locals.Pop();
            return ret.Max;
        }

        public int Offset => ops.Count;

        public void Push(string val)
        {
            Emit(new Op(OpCode.PushStr, frame.IndexedStrings.Count));
            frame.IndexedStrings.Add(new DyString(val));
        }

        public void Push(double val)
        {
            if (val == 0D)
                Emit(Op.PushR8_0);
            else
            {
                Emit(new Op(OpCode.PushR8, frame.IndexedFloats.Count));
                frame.IndexedFloats.Add(new DyFloat(val));
            }
        }

        public void Push(long val)
        {
            if (val == 0L)
                Emit(Op.PushI8_0);
            else if (val == 1L)
                Emit(Op.PushI8_1);
            else
            {
                Emit(new Op(OpCode.PushI8, frame.IndexedIntegers.Count));
                frame.IndexedIntegers.Add(new DyInteger(val));
            }
        }

        public void Push(bool val)
        {
            if (val)
                Emit(Op.PushI1_1);
            else
                Emit(Op.PushI1_0);
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
            var idx = frame.IndexedStrings.Count;
            frame.IndexedStrings.Add(tag);
            Emit(new Op(OpCode.Tag, idx));
        }

        public void TraitG(string name)
        {
            var idx = frame.IndexedStrings.Count;
            frame.IndexedStrings.Add(name);
            Emit(new Op(OpCode.TraitG, idx));
        }

        public void Call(int args) => Emit(new Op(OpCode.Call, args), -args);
        public void NewFun(int funHandle) => Emit(new Op(OpCode.NewFun, funHandle));
        public void NewFunV(int funHandle) => Emit(new Op(OpCode.NewFunV, funHandle));
        public void Br(Label lab) => Emit(OpCode.Br, lab);
        public void Brtrue(Label lab) => Emit(OpCode.Brtrue, lab);
        public void Brfalse(Label lab) => Emit(OpCode.Brfalse, lab);
        public void Set(int type) => Emit(new Op(OpCode.TraitS, type));
        public void RunMod(int code) => Emit(new Op(OpCode.RunMod, code));

        public void Str() => Emit(Op.Str);
        public void Get() => Emit(Op.Get);
        public void Set() => Emit(Op.Set);
        public void This() => Emit(Op.This);
        public void Self() => Emit(Op.Self);
        public void Type() => Emit(Op.Type);
        public void PushNil() => Emit(Op.PushNil);
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
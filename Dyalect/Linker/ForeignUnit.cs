using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System;
using Dyalect.Runtime;

namespace Dyalect.Linker
{
    public abstract class ForeignUnit : Unit
    {
        internal List<DyObject> Values { get; } = new List<DyObject>();

        protected void RegisterGlobal(string name, DyObject value)
        {
            base.ExportList.Add(new PublishedName(name,
                new ScopeVar(0 | base.ExportList.Count << 8, VarFlags.Foreign)));
            Values.Add(value);
        }

        protected void RegisterGlobal<T1>(string name, Func<T1> value) => RegisterGlobal(name, DyFunction.Create(value, name));
        protected void RegisterGlobal<T1, T2>(string name, Func<T1, T2> value) => RegisterGlobal(name, DyFunction.Create(value, name));
        protected void RegisterGlobal<T1, T2, T3>(string name, Func<T1, T2, T3> value) => RegisterGlobal(name, DyFunction.Create(value, name));
        protected void RegisterGlobal<T1, T2, T3, T4>(string name, Func<T1, T2, T3, T4> value) => RegisterGlobal(name, DyFunction.Create(value, name));
        protected void RegisterGlobal<T1, T2, T3, T4, T5>(string name, Func<T1, T2, T3, T4, T5> value) => RegisterGlobal(name, DyFunction.Create(value, name));
        protected void RegisterGlobal<T1, T2, T3, T4, T5, T6>(string name, Func<T1, T2, T3, T4, T5, T6> value) => RegisterGlobal(name, DyFunction.Create(value, name));
        protected void RegisterGlobal<T1, T2, T3, T4, T5, T6, T7>(string name, Func<T1, T2, T3, T4, T5, T6, T7> value) => RegisterGlobal(name, DyFunction.Create(value, name));
        protected void RegisterGlobal<T1, T2, T3, T4, T5, T6, T7, T8>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8> value) => RegisterGlobal(name, DyFunction.Create(value, name));
        protected void RegisterGlobal<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9> value) => RegisterGlobal(name, DyFunction.Create(value, name));
        protected void RegisterGlobal<T1, T2>(string name, Func<T1[], T2> value) => RegisterGlobal(name, DyFunction.Create(value, name));
        protected void RegisterGlobal<T1, T2>(string name, Func<ExecutionContext, T1[], T2> value) => RegisterGlobal(name, DyFunction.Create(value, name));
    }
}

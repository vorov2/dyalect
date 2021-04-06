using System;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    public sealed class Scope
    {
        public Scope(ScopeKind kind, Scope parent)
        {
            Kind = kind;
            Parent = parent;
            Locals = new ();
            Autos = new ();
        }

        public ScopeVar GetVariable(string name)
        {
            if (!Locals.TryGetValue(name, out ScopeVar var))
                var = ScopeVar.Empty;

            return var;
        }

        public Scope Clone() => 
            new (Kind, Parent)
            {
                Locals = new (Locals),
                Autos = new (Autos)
            };

        public IEnumerable<string> EnumerateNames()
        {
            foreach (var kv in Locals)
                yield return kv.Key;
        }

        public IEnumerable<KeyValuePair<string, ScopeVar>> EnumerateVars()
        {
            foreach (var kv in Locals)
                yield return kv;
        }

        public bool LocalOrParent(string var)
        {
            if (Kind == ScopeKind.Function)
                return Locals.ContainsKey(var);

            var s = this;

            do
            {
                if (s.Locals.ContainsKey(var))
                    return true;

                s = s.Parent;
            }
            while (s != null && s.Kind != ScopeKind.Function);

            return false;
        }

        public void AddData(string name, int data)
        {
            var sv = Locals[name];
            sv = new ScopeVar(sv.Address, data);
            Locals[name] = sv;
        }

        public int TryChangeVariable(string name)
        {
            var v = default(ScopeVar);

            if (Locals.TryGetValue(name, out v))
            {
                v = new ScopeVar(v.Address, -1);
                Locals[name] = v;
                return v.Address;
            }

            return -1;
        }

        public bool IsGlobal => Parent == null;

        public Scope Parent { get; set; }

        public Queue<int> Autos { get; private set; }

        public Dictionary<string, ScopeVar> Locals { get; private set; }

        public ScopeKind Kind { get; private set; }
    }

    public enum ScopeKind
    {
        Lexical = 0,
        Function,
        Loop
    }
}

using System;

namespace Dyalect.Runtime.Types
{
    public abstract class DyObject : IEquatable<DyObject>
    {
        internal readonly int TypeId;

        internal DyObject(int typeId)
        {
            TypeId = typeId;
        }

        public override string ToString() => AsString();

        public virtual bool AsBool() => true;

        public virtual long AsInteger() => 0L;

        public virtual double AsFloat() => .0d;

        public virtual string AsString() => "[" + GetTypeInfo().TypeName + "]";

        public abstract object AsObject();

        public abstract DyTypeInfo GetTypeInfo();

        public string TypeName => GetTypeInfo().TypeName;

        protected abstract bool TestEquality(DyObject obj);
        public bool Equals(DyObject obj)
        {
            if (TypeId != obj.TypeId)
                return false;
            return TestEquality(obj);
        }

        internal protected virtual DyObject GetItem(DyObject index, ExecutionContext ctx) => null;

        internal protected virtual bool SetItem(DyObject index, DyObject value, ExecutionContext ctx) => false;
    }
}

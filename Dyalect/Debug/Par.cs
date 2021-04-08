using Dyalect.Runtime.Types;

namespace Dyalect.Debug
{
    public struct Par
    {
        public readonly string Name;
        public readonly bool IsVarArg;
        public readonly DyObject Value;

        internal Par(string name, DyObject val, bool isVarArg) => (Name, Value, IsVarArg) = (name, val, isVarArg);

        public Par(string name, bool isVarArg) => (Name, Value, IsVarArg) = (name, null, isVarArg);

        public Par(string name, DyObject value) => (Name, Value, IsVarArg) = (name, value, false);

        public Par(string name) => (Name, Value, IsVarArg) = (name, null, false);

        public override string ToString() => Name;
    }
}

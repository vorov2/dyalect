using Dyalect.Runtime.Types;

namespace Dyalect.Debug
{
    public struct Par
    {
        public readonly string Name;
        public readonly bool IsVarArg;
        public readonly DyObject Value;

        internal Par(string name, DyObject val, bool isVarArg)
        {
            Name = name;
            Value = val;
            IsVarArg = isVarArg;
        }

        public Par(string name, bool isVarArg)
        {
            Name = name;
            Value = null;
            IsVarArg = isVarArg;
        }

        public Par(string name, DyObject value)
        {
            Name = name;
            Value = value ?? DyNil.Instance;
            IsVarArg = false;
        }

        public Par(string name)
        {
            Name = name;
            Value = null;
            IsVarArg = false;
        }
    }
}

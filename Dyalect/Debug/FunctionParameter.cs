using Dyalect.Runtime.Types;

namespace Dyalect.Debug
{
    public struct FunctionParameter
    {
        public readonly string Name;
        public readonly bool IsVarArg;
        public readonly DyObject Value;

        public FunctionParameter(string name, DyObject value, bool isVarArg)
        {
            Name = name;
            Value = value;
            IsVarArg = isVarArg;
        }
    }
}

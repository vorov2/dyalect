using Dyalect.Debug;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System.Linq;

namespace Dyalect.Runtime;

public class ExecutionContext
{
    internal int RgDI; //RgDI register
    internal int RgFI; //RgFI register
    internal int CallCnt; //Call counter

    internal ExecutionContext(CallStack callStack, RuntimeContext rtx)
    {
        CallStack = callStack;
        CatchMarks = new();
        RuntimeContext = rtx;
    }

    public RuntimeContext RuntimeContext { get; }

    public bool HasErrors => Error != null;

    public ExecutionContext Clone() => new(CallStack, RuntimeContext);

    internal CallStack CallStack { get; }

    internal SectionStack CatchMarks { get; }

    internal DyFunction? EtaFunction { get; set; }

    internal DyObject InvokeEtaFunction(params DyObject[] args)
    {
        var fn = EtaFunction!;
        EtaFunction = null;
        Error = null;
        return fn.Call(this, args);
    }

    private DyVariant? _error;
    internal virtual DyVariant? Error
    {
        get => _error;
        set
        {
            if (_error is null || value is null)
                _error = value;
        }
    }

    internal Stack<StackPoint>? ErrorDump { get; set; }

    internal Stack<int>? Sections { get; set; }

    internal int UnitId;
    internal int CallerUnitId;

    public DyVariant? PopError()
    {
        var err = Error;
        Error = null;
        return err;
    }

    public void ThrowIf()
    {
        if (Error is not null)
        {
            var err = Error;
            Error = null;
            throw new DyCodeException(err);
        }
    }

    public T Type<T>() where T : DyTypeInfo => RuntimeContext.Types.OfType<T>().First();

    #region Context variables
    private readonly object syncRoot = new();
    private readonly Dictionary<string, object> contextVariables = new();
    public void SetContextVariable(string key, object val)
    {
        lock (syncRoot)
            contextVariables[key] = val;
    }

    public T? GetContextVariable<T>(string key)
    {
        if (!contextVariables.TryGetValue(key, out var val))
            return default;
        return (T)val;
    }

    public bool HasContextVariable(string key) => contextVariables.ContainsKey(key);
    #endregion

    #region ArgContainer
    private int count;
    private readonly List<ArgContainer> containers = new(2);

    internal sealed class ArgContainer
    {
        public DyObject[] Locals = null!;
        public DyObject[]? VarArgs;
        public int VarArgsSize;
        public int VarArgsIndex;
    }

    internal ArgContainer PushArguments(DyObject[] locals, int varArgsIndex, DyObject[]? varArgs = null)
    {
        if (containers.Count <= count)
            containers.Add(new());

        var ret = containers[count++];
        ret.Locals = locals;
        ret.VarArgsIndex = varArgsIndex;
        ret.VarArgs = varArgs;
        ret.VarArgsSize = 0;
        return ret;
    }

    internal ArgContainer PopArguments() => containers[--count];

    internal ArgContainer PeekArguments() => containers[count - 1];
    #endregion
}

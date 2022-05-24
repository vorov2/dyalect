using Dyalect.Debug;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
namespace Dyalect.Runtime;

public sealed class ExecutionContext
{
    internal int RgDI; //RgDI register
    internal int RgFI; //RgFI register
    internal int CallCnt; //Call counter

    internal int UnitId { get; set; }

    internal int CallerUnitId { get; set; }

    internal CallStack CallStack { get; }

    public RuntimeContext RuntimeContext { get; }

    internal ExecutionContext(CallStack callStack, RuntimeContext rtx) =>
        (CallStack, CatchMarks, RuntimeContext) = (callStack, new(), rtx);

    internal ExecutionContext(RuntimeContext rtx) : this(new(), rtx) { }

    public ExecutionContext Clone() => new(CallStack, RuntimeContext);

    #region CallBack
    internal DyFunction? CallBackFunction { get; set; }

    internal DyObject InvokeCallBackFunction(params DyObject[] args)
    {
        var fn = CallBackFunction!;
        CallBackFunction = null;
        Error = null;
        return fn.Call(this, args);
    }
    #endregion

    #region Critical sections
    internal SectionStack CatchMarks { get; }

    internal Stack<int>? Sections { get; set; }
    #endregion

    #region Errors
    public bool HasErrors => Error != null;

    private DyVariant? _error;
    public DyVariant? Error
    {
        get => _error;
        internal set
        {
            if (_error is null || value is null)
                _error = value;
        }
    }

    internal Stack<StackPoint>? ErrorDump { get; set; }

    internal CallStackTrace? Trace { get; set; }

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
    #endregion

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

using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Linq;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyByteArrayTypeInfo : DyForeignTypeInfo
{
    private const string ByteArray = nameof(ByteArray);

    public override string ReflectedTypeName => ByteArray;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Len;

    #region Operations
    public DyByteArray Create(byte[]? buffer) => new(this, buffer);

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
    {
        var buffer = ((DyByteArray)arg).GetBytes();
        var strs = buffer.Select(b => "0x" + b.ToString("X").PadLeft(2, '0')).ToArray();
        return new DyString("{" + string.Join(",", strs) + "}");
    }

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
        DyInteger.Get(((DyByteArray)arg).Count);
    #endregion

    [InstanceMethod]
    internal static DyObject Read(ExecutionContext ctx, DyByteArray self, DyTypeInfo typeInfo) => self.Read(ctx, typeInfo);

    [InstanceMethod]
    internal static void Write(ExecutionContext ctx, DyByteArray self, DyObject value) => self.Write(ctx, value);

    [InstanceMethod]
    internal static int Position(DyByteArray self) => self.Position;

    [InstanceMethod]
    internal static void Reset(DyByteArray self) => self.Reset();

    [StaticMethod]
    internal static DyObject Concat(ExecutionContext ctx, DyByteArray first, DyByteArray second)
    {
        var a1 = first.GetBytes();
        var a2 = second.GetBytes();
        var a3 = new byte[a1.Length + a2.Length];
        Array.Copy(a1, a3, a1.Length);
        Array.Copy(a2, 0, a3, a1.Length, a2.Length);
        return new DyByteArray(ctx.Type<DyByteArrayTypeInfo>(), a3);
    }

    [StaticMethod(ByteArray)]
    internal static DyObject CreateNew(ExecutionContext ctx) => new DyByteArray(ctx.Type<DyByteArrayTypeInfo>(), null);
}

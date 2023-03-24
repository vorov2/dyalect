using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.IO;
using System.Linq;

namespace Dyalect.Library.IO;

[GeneratedType]
public sealed partial class DyDriveTypeInfo : DyForeignTypeInfo<IOModule>
{
    public override string ReflectedTypeName => "Drive";

    [InstanceProperty]
    internal static string? Name(ExecutionContext ctx, DyDrive self) => ctx.Handle(() => self.Value.Name);

    [InstanceProperty]
    internal static long TotalSize(ExecutionContext ctx, DyDrive self) => ctx.Handle(() => self.Value.TotalSize);
    
    [InstanceProperty]
    internal static long TotalFreeSpace(ExecutionContext ctx, DyDrive self) => ctx.Handle(() => self.Value.TotalFreeSpace);

    [InstanceProperty]
    internal static long AvailableFreeSpace(ExecutionContext ctx, DyDrive self) => ctx.Handle(() => self.Value.AvailableFreeSpace);

    [InstanceProperty]
    internal static string? Format(ExecutionContext ctx, DyDrive self) => ctx.Handle(() => self.Value.DriveFormat);

    [InstanceProperty]
    internal static string? Root(ExecutionContext ctx, DyDrive self) => ctx.Handle(() => self.Value.RootDirectory.FullName);

    [InstanceProperty]
    internal static string? Type(ExecutionContext ctx, DyDrive self) => ctx.Handle(() => self.Value.DriveType.ToString());

    [InstanceProperty]
    internal static bool IsReady(ExecutionContext ctx, DyDrive self) => ctx.Handle(() => self.Value.IsReady);

    [InstanceProperty]
    internal static string? Label(ExecutionContext ctx, DyDrive self) => ctx.Handle(() => self.Value.VolumeLabel);

    [StaticMethod]
    internal static DyObject[]? GetDrives(ExecutionContext ctx) =>
        ctx.Handle(() => DriveInfo.GetDrives().Select(d => new DyDrive(ctx.Type<DyDriveTypeInfo>(), d)).ToArray());
}

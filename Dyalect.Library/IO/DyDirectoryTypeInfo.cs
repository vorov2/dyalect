using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.IO;

namespace Dyalect.Library.IO;

[GeneratedType]
public sealed partial class DyDirectoryTypeInfo : DyForeignTypeInfo<IOModule>
{
    public override string ReflectedTypeName => "Directory";

    [StaticMethod]
    internal static bool Exists(ExecutionContext ctx, string path) => ctx.Handle(() => Directory.Exists(path));

    [StaticMethod]
    internal static void Create(ExecutionContext ctx, string path) => ctx.Handle(() => Directory.CreateDirectory(path));

    [StaticMethod]
    internal static void Delete(ExecutionContext ctx, string path, bool recursive = false) => ctx.Handle(() => Directory.Delete(path, recursive));

    [StaticMethod]
    internal static void Move(ExecutionContext ctx, string path, string other) => ctx.Handle(() => Directory.Move(path, other));

    [StaticMethod]
    internal static void Copy(ExecutionContext ctx, string path, string other) =>
        ctx.Handle(() =>
        {
            foreach (string dirPath in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(path, other));

            foreach (string newPath in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(path, other), true);
        });
}

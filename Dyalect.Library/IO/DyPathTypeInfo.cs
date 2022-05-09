using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.IO;
using System.Linq;
namespace Dyalect.Library.IO;

[GeneratedType]
public sealed partial class DyPathTypeInfo : DyForeignTypeInfo<IOModule>
{
    public override string ReflectedTypeName => "Path";

    [StaticMethod]
    internal static string? GetFullPath(string path) => Path.GetFullPath(path);

    [StaticMethod]
    internal static string? GetDirectory(string path) => Path.GetDirectoryName(path);

    [StaticMethod]
    internal static string? GetExtension(string path) => Path.GetExtension(path);

    [StaticMethod]
    internal static string? GetFileName(string path) => Path.GetFileName(path);

    [StaticMethod]
    internal static string? GetPathRoot(string path) => Path.GetPathRoot(path);

    [StaticMethod]
    internal static bool Exists(ExecutionContext ctx, string path)
    {
        try
        {
            return Directory.Exists(path) || File.Exists(path);
        }
        catch (ArgumentException)
        {
            ctx.InvalidValue(path);
            return default;
        }
    }

    [StaticMethod]
    internal static string? GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);

    [StaticMethod]
    internal static string? Combine(ExecutionContext ctx, string path, string other)
    {
        string dir;
        
        try
        {
            dir = Path.Combine(path, other);
        }
        catch (ArgumentException)
        {
            ctx.InvalidValue();
            return default;
        }
        
        return dir;
    }

    [StaticMethod]
    internal static DyObject EnumerateFiles(ExecutionContext ctx, string path, string? mask = null)
    {
        try
        {
            var seq = mask is not null
                ? Directory.EnumerateFiles(path, mask)
                : Directory.EnumerateFiles(path);
            return DyIterator.Create(seq.Select(s => new DyString(s)));
        }
        catch (ArgumentException)
        {
            return ctx.InvalidValue();
        }
        catch (Exception)
        {
            return ctx.IOFailed();
        }
    }

    [StaticMethod]
    internal static DyObject EnumerateDirectories(ExecutionContext ctx, string path, string? mask = null)
    {
        try
        {
            var seq = mask is not null
                ? Directory.EnumerateDirectories(path, mask)
                : Directory.EnumerateDirectories(path);
            return DyIterator.Create(seq.Select(s => new DyString(s)));
        }
        catch (ArgumentException)
        {
            return ctx.InvalidValue();
        }
        catch (Exception)
        {
            return ctx.IOFailed();
        }
    }
}

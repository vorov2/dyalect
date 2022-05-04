using Dyalect.Codegen;
using Dyalect.Library.Core;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.IO;
using System.Linq;
using System.Text;
namespace Dyalect.Library.IO;

[GeneratedType]
public sealed partial class DyFileTypeInfo : DyForeignTypeInfo<IOModule>
{
    public override string ReflectedTypeName => "File";

    protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

    private static Encoding GetEncoding(ExecutionContext ctx, int? encoding)
    {
        var enc = Encoding.UTF8.CodePage;

        if (encoding is not null)
            enc = encoding.Value;

        try
        {
            return Encoding.GetEncoding(enc);
        }
        catch (Exception)
        {
            if (encoding is not null)
                ctx.InvalidValue(encoding);

            return Encoding.UTF8;
        }
    }

    [StaticMethod]
    internal static DyObject? ReadText(ExecutionContext ctx, string path, int? encoding = null)
    {
        var enc = GetEncoding(ctx, encoding);

        if (ctx.HasErrors)
            return Nil;

        return ctx.Handle(() => new DyString(File.ReadAllText(path, enc)));
    }

    [StaticMethod]
    internal static DyObject? ReadLines(ExecutionContext ctx, string path, int? encoding = null)
    {
        var enc = GetEncoding(ctx, encoding);

        if (ctx.HasErrors)
            return Nil;

        return ctx.Handle(() => new DyArray(File.ReadAllLines(path, enc).Select(l => new DyString(l)).ToArray()));
    }

    [StaticMethod]
    internal static void WriteAllText(ExecutionContext ctx, string path, string data, int? encoding = null)
    {
        var enc = GetEncoding(ctx, encoding);

        if (ctx.HasErrors)
            return;

        ctx.Handle(() => File.WriteAllText(path, data, enc));
    }

    [StaticMethod]
    internal static void WriteAllLines(ExecutionContext ctx, string path, DyObject value, int? encoding = null)
    {
        var enc = GetEncoding(ctx, encoding);
        var seq = DyIterator.ToEnumerable(ctx, value).ToArray();

        if (ctx.HasErrors)
            return;

        var strings = seq.Select(s => s.ToString(ctx).Value).ToArray();

        if (ctx.HasErrors)
            return;

        ctx.Handle(() => File.WriteAllLines(path, strings, enc));
    }

    [StaticMethod]
    internal static DyObject? ReadAllBytes(ExecutionContext ctx, string path) => ctx.Handle(() => ctx.Type<DyByteArrayTypeInfo>().Create(File.ReadAllBytes(path)));

    [StaticMethod]
    internal static void WriteAllBytes(ExecutionContext ctx, string path, DyByteArray value) => ctx.Handle(() => File.WriteAllBytes(path, value.GetBytes()));

    [StaticMethod]
    internal static bool Exists(ExecutionContext ctx, string path) => ctx.Handle(() => File.Exists(path));

    [StaticMethod]
    internal static void Create(ExecutionContext ctx, string path) => ctx.Handle(() => File.Create(path).Dispose());

    [StaticMethod]
    internal static void Delete(ExecutionContext ctx, string path) =>
        ctx.Handle(() =>
        {
            File.SetAttributes(path, FileAttributes.Normal);
            File.Delete(path);
        });

    [StaticMethod]
    internal static DyObject? GetAttributes(ExecutionContext ctx, string path) =>
        ctx.Handle(() =>
        {
            var attr = File.GetAttributes(path);
            return DyTuple.Create(
                new("readOnly", (DyBool)attr.HasFlag(FileAttributes.ReadOnly)),
                new("hidden", (DyBool)attr.HasFlag(FileAttributes.Hidden)),
                new("system", (DyBool)attr.HasFlag(FileAttributes.System)),
                new("directory", (DyBool)attr.HasFlag(FileAttributes.Directory)),
                new("archive", (DyBool)attr.HasFlag(FileAttributes.Archive)),
                new("device", (DyBool)attr.HasFlag(FileAttributes.Device)),
                new("normal", (DyBool)attr.HasFlag(FileAttributes.Normal)),
                new("temporary", (DyBool)attr.HasFlag(FileAttributes.Temporary)),
                new("sparseFile", (DyBool)attr.HasFlag(FileAttributes.SparseFile)),
                new("reparsePoint", (DyBool)attr.HasFlag(FileAttributes.ReparsePoint)),
                new("compressed", (DyBool)attr.HasFlag(FileAttributes.Compressed)),
                new("offline", (DyBool)attr.HasFlag(FileAttributes.Offline)),
                new("notContentIndexed", (DyBool)attr.HasFlag(FileAttributes.NotContentIndexed)),
                new("encrypted", (DyBool)attr.HasFlag(FileAttributes.Encrypted)),
                new("integrityStream", (DyBool)attr.HasFlag(FileAttributes.IntegrityStream)),
                new("noScrubData", (DyBool)attr.HasFlag(FileAttributes.NoScrubData))
            );
        });

    [StaticMethod]
    internal static void SetAttributes(ExecutionContext ctx, string path, string[] attributes) =>
        ctx.Handle(() =>
        {
            FileAttributes attr = default;

            foreach (var t in attributes)
            {
                if (!Enum.TryParse<FileAttributes>(t, out var fa))
                {
                    ctx.InvalidValue(t);
                    return;
                }

                attr |= fa;
            }

            if (attr != default)
                File.SetAttributes(path, attr);
        });

    [StaticMethod]
    internal static void Copy(ExecutionContext ctx, string source, string destination, bool overwrite = false) =>
        ctx.Handle(() => File.Copy(source, destination, overwrite));

    [StaticMethod]
    internal static void Move(ExecutionContext ctx, string source, string destination, bool overwrite = false) => ctx.Handle(() => File.Move(source, destination, overwrite));

    [StaticMethod]
    internal static DyObject? GetCreationTime(ExecutionContext ctx, string path) =>
        ctx.Handle(() => new DyDateTime(ctx.Type<DyDateTimeTypeInfo>(), File.GetCreationTimeUtc(path).Ticks));

    [StaticMethod]
    internal static void SetCreationTime(ExecutionContext ctx, string path, DyDateTime value) => ctx.Handle(() => File.SetCreationTimeUtc(path, GetDateTimeUtc(value)));

    [StaticMethod]
    internal static DyObject? GetLastAccessTime(ExecutionContext ctx, string path) =>
        ctx.Handle(() => new DyDateTime(ctx.Type<DyDateTimeTypeInfo>(), File.GetLastAccessTimeUtc(path).Ticks));

    [StaticMethod]
    internal static void SetLastAccessTime(ExecutionContext ctx, string path, DyDateTime value) => ctx.Handle(() => File.SetLastAccessTimeUtc(path, GetDateTimeUtc(value)));

    [StaticMethod]
    internal static DyObject? GetLastWriteTime(ExecutionContext ctx, string path) => 
        ctx.Handle(() => new DyDateTime(ctx.Type<DyDateTimeTypeInfo>(), File.GetLastWriteTimeUtc(path).Ticks));

    [StaticMethod]
    internal static void SetLastWriteTime(ExecutionContext ctx, string path, DyDateTime value) => ctx.Handle(() => File.SetLastWriteTimeUtc(path, GetDateTimeUtc(value)));

    private static DateTime GetDateTimeUtc(DyDateTime value) =>
        value is DyLocalDateTime loc ? loc.ToDateTimeOffset().ToUniversalTime().DateTime : value.ToDateTime();
}

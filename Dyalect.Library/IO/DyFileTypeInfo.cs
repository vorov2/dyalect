using Dyalect.Debug;
using Dyalect.Library.Core;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.IO;
using System.Text;
using System.Linq;

namespace Dyalect.Library.IO
{
    public sealed class DyFileTypeInfo : DyForeignTypeInfo<IOModule>
    {
        public override string ReflectedTypeName => "File";

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        private Encoding GetEncoding(ExecutionContext ctx, DyObject encoding)
        {
            var enc = Encoding.UTF8.CodePage;

            if (encoding.NotNil() && encoding.IsInteger(ctx))
                enc = (int)encoding.GetInteger();

            try
            {
                return Encoding.GetEncoding(enc);
            }
            catch (Exception)
            {
                ctx.InvalidValue(encoding);
                return Encoding.UTF8;
            }
        }

        private string? GetPath(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx))
                return null;

            var pathStr = path.GetString();

            if (string.IsNullOrWhiteSpace(pathStr))
            {
                ctx.InvalidValue(path);
                return null;
            }

            return pathStr;
        }

        private DyObject Handle(ExecutionContext ctx, Func<DyObject> action, DyObject? arg = null)
        {
            try
            {
                return action();
            }
            catch (ArgumentException)
            {
                return arg is null ? ctx.InvalidValue() : ctx.InvalidValue(arg);
            }
            catch (Exception)
            {
                return ctx.IOFailed();
            }
        }

        private DyObject ReadAllText(ExecutionContext ctx, DyObject path, DyObject encoding)
        {
            var (enc, spath) = (GetEncoding(ctx, encoding), GetPath(ctx, path));
            if (ctx.HasErrors) return Nil;
            return Handle(ctx, () => new DyString(File.ReadAllText(spath!, enc)), path);
        }

        private DyObject ReadAllLines(ExecutionContext ctx, DyObject path, DyObject encoding)
        {
            var (enc, spath) = (GetEncoding(ctx, encoding), GetPath(ctx, path));
            if (ctx.HasErrors) return Nil;
            return Handle(ctx, () => new DyArray(File.ReadAllLines(spath!, enc).Select(l => new DyString(l)).ToArray()), path);
        }

        public DyObject WriteAllText(ExecutionContext ctx, DyObject path, DyObject data, DyObject encoding)
        {
            var (enc, spath) = (GetEncoding(ctx, encoding), GetPath(ctx, path));
            if (ctx.HasErrors) return Nil;
            if (!data.IsString(ctx)) return Nil;
            return Handle(ctx, () =>
            {
                File.WriteAllText(spath!, data.GetString(), enc);
                return Nil;
            }, data);
        }

        public DyObject WriteAllLines(ExecutionContext ctx, DyObject path, DyObject data, DyObject encoding)
        {
            var (enc, spath) = (GetEncoding(ctx, encoding), GetPath(ctx, path));
            if (ctx.HasErrors) return Nil;
            var seq = DyIterator.ToEnumerable(ctx, data).ToArray();
            if (ctx.HasErrors) return Nil;
            var strings = seq.Select(s => s.ToString(ctx).GetString()).ToArray();
            if (ctx.HasErrors) return Nil;
            return Handle(ctx, () =>
            {
                File.WriteAllLines(spath!, strings, enc);
                return Nil;
            });
        }

        public DyObject ReadAllBytes(ExecutionContext ctx, DyObject path)
        {
            var spath = GetPath(ctx, path);
            if (ctx.HasErrors) return Nil;
            return Handle(ctx, () => DeclaringUnit.Core.Value.ByteArray.Create(File.ReadAllBytes(spath!)), path);
        }

        public DyObject WriteAllBytes(ExecutionContext ctx, DyObject path, DyObject arr)
        {
            var spath = GetPath(ctx, path);
            if (ctx.HasErrors) return Nil;
            if (arr.TypeId != DeclaringUnit.Core.Value.ByteArray.TypeId)
                return ctx.InvalidType(DeclaringUnit.Core.Value.ByteArray.TypeId, arr);
            return Handle(ctx, () =>
            {
                File.WriteAllBytes(spath!, ((DyByteArray)arr).GetBytes());
                return Nil;
            }, path);
        }

        private DyObject FileExists(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Nil;
            return Handle(ctx, () => File.Exists(path.GetString()) ? DyBool.True : DyBool.False, path);
        }

        private DyObject CreateFile(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Nil;
            return Handle(ctx, () =>
            {
                File.Create(path.GetString()).Dispose();
                return DyNil.Instance;
            }, path);
        }

        private DyObject DeleteFile(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Nil;
            return Handle(ctx, () =>
            {
                var p = path.GetString();
                File.SetAttributes(p, FileAttributes.Normal);
                File.Delete(p);
                return DyNil.Instance;
            }, path);
        }

        private DyObject GetAttributes(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Nil;
            return Handle(ctx, () =>
            {
                var attr = File.GetAttributes(path.GetString());
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
            }, path);
        }

        private DyObject SetAttributes(ExecutionContext ctx, DyObject path, DyObject attributes)
        {
            if (!path.IsString(ctx)) return Nil;
            var tup = (DyTuple)attributes;
            return Handle(ctx, () =>
            {
                FileAttributes attr = default;

                foreach (var t in tup)
                {
                    if (!t.IsString(ctx)) return Nil;

                    if (!Enum.TryParse<FileAttributes>(t.GetString(), out var fa))
                        return ctx.InvalidValue(t);

                    attr |= fa;
                }

                if (attr != default)
                    File.SetAttributes(path.GetString(), attr);

                return DyNil.Instance;
            }, path);
        }

        private DyObject CopyFile(ExecutionContext ctx, DyObject source, DyObject destination, DyObject overwrite)
        {
            if (!source.IsString(ctx)) return Nil;
            if (!destination.IsString(ctx)) return Nil;
            return Handle(ctx, () =>
            {
                File.Copy(source.GetString(), destination.GetString(), overwrite.IsTrue());
                return DyNil.Instance;
            });
        }

        private DyObject MoveFile(ExecutionContext ctx, DyObject source, DyObject destination, DyObject overwrite)
        {
            if (!source.IsString(ctx)) return Nil;
            if (!destination.IsString(ctx)) return Nil;
            return Handle(ctx, () =>
            {
                File.Move(source.GetString(), destination.GetString(), overwrite.IsTrue());
                return DyNil.Instance;
            });
        }

        private DyObject GetCreationTime(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Nil;
            return Handle(ctx, () => new DyDateTime(DeclaringUnit.Core.Value.DateTime, File.GetCreationTimeUtc(path.GetString()).Ticks), path);
        }

        private DyObject SetCreationTime(ExecutionContext ctx, DyObject path, DyObject date)
        {
            if (!path.IsString(ctx)) return Nil;
            if (date.TypeId != DeclaringUnit.Core.Value.DateTime.TypeId)
                return ctx.InvalidType(date);
            return Handle(ctx, () =>
            {
                File.SetCreationTimeUtc(path.GetString(), new DateTime(((DyDateTime)date).Ticks));
                return Nil;
            }, path);
        }

        private DyObject GetLastAccessTime(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Nil;
            return Handle(ctx, () => new DyDateTime(DeclaringUnit.Core.Value.DateTime, File.GetLastAccessTimeUtc(path.GetString()).Ticks), path);
        }

        private DyObject SetLastAccessTime(ExecutionContext ctx, DyObject path, DyObject date)
        {
            if (!path.IsString(ctx)) return Nil;
            if (date.TypeId != DeclaringUnit.Core.Value.DateTime.TypeId)
                return ctx.InvalidType(date);
            return Handle(ctx, () =>
            {
                File.SetLastAccessTimeUtc(path.GetString(), new DateTime(((DyDateTime)date).Ticks));
                return Nil;
            }, path);
        }

        private DyObject GetLastWriteTime(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Nil;
            return Handle(ctx, () => new DyDateTime(DeclaringUnit.Core.Value.DateTime, File.GetLastWriteTimeUtc(path.GetString()).Ticks), path);
        }

        private DyObject SetLastWriteTime(ExecutionContext ctx, DyObject path, DyObject date)
        {
            if (!path.IsString(ctx)) return Nil;
            if (date.TypeId != DeclaringUnit.Core.Value.DateTime.TypeId)
                return ctx.InvalidType(date);
            return Handle(ctx, () =>
            {
                File.SetLastWriteTimeUtc(path.GetString(), new DateTime(((DyDateTime)date).Ticks));
                return Nil;
            }, path);
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "ReadText" => Func.Static(name, ReadAllText, -1, new Par("path"), new Par("encoding", DyNil.Instance)),
                "ReadLines" => Func.Static(name, ReadAllLines, -1, new Par("path"), new Par("encoding", DyNil.Instance)),
                "WriteText" => Func.Static(name, WriteAllText, -1, new Par("path"), new Par("value"), new Par("encoding", DyNil.Instance)),
                "WriteAllLines" => Func.Static(name, WriteAllLines, -1, new Par("path"), new Par("value"), new Par("encoding", DyNil.Instance)),
                "ReadBytes" => Func.Static(name, ReadAllBytes, -1, new Par("path")),
                "WriteBytes" => Func.Static(name, WriteAllBytes, -1, new Par("path"), new Par("value")),
                "Exists" => Func.Static(name, FileExists, -1, new Par("path")),
                "Create" => Func.Static(name, CreateFile, -1, new Par("path")),
                "Delete" => Func.Static(name, DeleteFile, -1, new Par("path")),
                "GetAttributes" => Func.Static(name, GetAttributes, -1, new Par("path")),
                "SetAttributes" => Func.Static(name, SetAttributes, 1, new Par("path"), new Par("values")),
                "Move" => Func.Static(name, MoveFile, -1, new Par("source"), new Par("destination"), new Par("overwrite", DyBool.False)),
                "Copy" => Func.Static(name, CopyFile, -1, new Par("source"), new Par("destination"), new Par("overwrite", DyBool.False)),
                "GetCreationTime" => Func.Static(name, GetCreationTime, -1, new Par("path")),
                "GetLastAccessTime" => Func.Static(name, GetLastAccessTime, -1, new Par("path")),
                "GetLastWriteTime" => Func.Static(name, GetLastWriteTime, -1, new Par("path")),
                "SetCreationTime" => Func.Static(name, SetCreationTime, -1, new Par("path"), new Par("value")),
                "SetLastAccessTime" => Func.Static(name, SetLastAccessTime, -1, new Par("path"), new Par("value")),
                "SetLastWriteTime" => Func.Static(name, SetLastWriteTime, -1, new Par("path"), new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}

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
        public override string TypeName => "File";

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

        private DyObject Handle(ExecutionContext ctx, Func<DyObject> action)
        {
            try
            {
                return action();
            }
            catch (Exception)
            {
                return ctx.IOFailed();
            }
        }
        private DyObject Handle(ExecutionContext ctx, Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                ctx.IOFailed();
            }

            return Default();
        }

        private DyObject ReadAllText(ExecutionContext ctx, DyObject path, DyObject encoding)
        {
            var (enc, spath) = (GetEncoding(ctx, encoding), GetPath(ctx, path));
            if (ctx.HasErrors) return Default();
            return Handle(ctx, () => new DyString(File.ReadAllText(spath!, enc)));
        }

        private DyObject ReadAllLines(ExecutionContext ctx, DyObject path, DyObject encoding)
        {
            var (enc, spath) = (GetEncoding(ctx, encoding), GetPath(ctx, path));
            if (ctx.HasErrors) return Default();
            return Handle(ctx, () => new DyArray(File.ReadAllLines(spath!, enc).Select(l => new DyString(l)).ToArray()));
        }

        public DyObject WriteAllText(ExecutionContext ctx, DyObject path, DyObject data, DyObject encoding)
        {
            var (enc, spath) = (GetEncoding(ctx, encoding), GetPath(ctx, path));
            if (ctx.HasErrors) return Default();
            if (!data.IsString(ctx)) return Default();
            return Handle(ctx, () => File.WriteAllText(spath!, data.GetString(), enc));
        }

        public DyObject WriteAllLines(ExecutionContext ctx, DyObject path, DyObject data, DyObject encoding)
        {
            var (enc, spath) = (GetEncoding(ctx, encoding), GetPath(ctx, path));
            if (ctx.HasErrors) return Default();
            var seq = DyIterator.ToEnumerable(ctx, data).ToArray();
            if (ctx.HasErrors) return Default();
            var strings = seq.Select(s => s.ToString(ctx).GetString()).ToArray();
            if (ctx.HasErrors) return Default();
            return Handle(ctx, () => File.WriteAllLines(spath!, strings, enc));
        }

        public DyObject ReadAllBytes(ExecutionContext ctx, DyObject path)
        {
            var spath = GetPath(ctx, path);
            if (ctx.HasErrors) return Default();
            return Handle(ctx, () => DeclaringUnit.Core.Value.ByteArray.Create(File.ReadAllBytes(spath!)));
        }

        public DyObject WriteAllBytes(ExecutionContext ctx, DyObject path, DyObject arr)
        {
            var spath = GetPath(ctx, path);
            if (ctx.HasErrors) return Default();
            if (arr.TypeId != DeclaringUnit.Core.Value.ByteArray.TypeId)
                return ctx.InvalidType(DeclaringUnit.Core.Value.ByteArray.TypeId, arr);
            return Handle(ctx, () => File.WriteAllBytes(spath!, ((DyByteArray)arr).GetBytes()));
        }

        private DyObject FileExists(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();

            try
            {
                return File.Exists(path.GetString()) ? DyBool.True : DyBool.False;
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(path);
            }
        }

        private DyObject CreateFile(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();

            try
            {
                File.Create(path.GetString()).Dispose();
                return DyNil.Instance;
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(path);
            }
            catch (Exception)
            {
                return ctx.IOFailed();
            }
        }

        private DyObject DeleteFile(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();

            try
            {
                var p = path.GetString();
                File.SetAttributes(p, FileAttributes.Normal);
                File.Delete(p);
                return DyNil.Instance;
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(path);
            }
            catch (Exception)
            {
                return ctx.IOFailed();
            }
        }

        private DyObject GetAttributes(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();

            try
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
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(path);
            }
            catch (Exception)
            {
                return ctx.IOFailed();
            }
        }



        private DyObject SetAttributes(ExecutionContext ctx, DyObject path, DyObject attributes)
        {
            if (!path.IsString(ctx)) return Default();
            var tup = (DyTuple)attributes;

            try
            {
                FileAttributes attr = default;

                foreach (var t in tup)
                {
                    if (!t.IsString(ctx)) return Default();

                    if (!Enum.TryParse<FileAttributes>(t.GetString(), out var fa))
                        return ctx.InvalidValue(t);

                    attr |= fa;
                }

                if (attr != default)
                    File.SetAttributes(path.GetString(), attr);

                return DyNil.Instance;
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(path);
            }
            catch (Exception)
            {
                return ctx.IOFailed();
            }
        }

        private DyObject CopyFile(ExecutionContext ctx, DyObject source, DyObject destination, DyObject overwrite)
        {
            if (!source.IsString(ctx)) return Default();
            if (!destination.IsString(ctx)) return Default();

            try
            {
                File.Copy(source.GetString(), destination.GetString(), overwrite.IsTrue());
                return DyNil.Instance;
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(source);
            }
            catch (Exception)
            {
                return ctx.IOFailed();
            }
        }

        private DyObject MoveFile(ExecutionContext ctx, DyObject source, DyObject destination, DyObject overwrite)
        {
            if (!source.IsString(ctx)) return Default();
            if (!destination.IsString(ctx)) return Default();

            try
            {
                File.Move(source.GetString(), destination.GetString(), overwrite.IsTrue());
                return DyNil.Instance;
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(source);
            }
            catch (Exception)
            {
                return ctx.IOFailed();
            }
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
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}

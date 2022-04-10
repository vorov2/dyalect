using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.IO;

namespace Dyalect.Library.IO
{
    public sealed class DyDirectoryTypeInfo : DyForeignTypeInfo<IOModule>
    {
        public override string TypeName => "Directory";

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        private DyObject Exists(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();

            try
            {
                var str = path.GetString();
                return Directory.Exists(str) ? DyBool.True : DyBool.False;
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(path);
            }
        }

        private DyObject Create(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();

            try
            {
                Directory.CreateDirectory(path.GetString());
                return DyNil.Instance;
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

        private DyObject Delete(ExecutionContext ctx, DyObject path, DyObject recursive)
        {
            if (!path.IsString(ctx)) return Default();

            try
            {
                Directory.Delete(path.GetString(), recursive.IsTrue());
                return DyNil.Instance;
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

        private DyObject Move(ExecutionContext ctx, DyObject path, DyObject other)
        {
            if (!path.IsString(ctx)) return Default();
            if (!other.IsString(ctx)) return Default();

            try
            {
                Directory.Move(path.GetString(), other.GetString());
                return DyNil.Instance;
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

        private DyObject Copy(ExecutionContext ctx, DyObject path, DyObject other)
        {
            if (!path.IsString(ctx)) return Default();
            if (!other.IsString(ctx)) return Default();

            try
            {
                var (sourcePath, targetPath) = (path.GetString(), other.GetString());

                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                    
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);

                return DyNil.Instance;
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

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Exists" => Func.Static(name, Exists, -1, new Par("path")),
                "Create" => Func.Static(name, Create, -1, new Par("path")),
                "Delete" => Func.Static(name, Delete, -1, new Par("path"), new Par("recursive", DyBool.False)),
                "Copy" => Func.Static(name, Delete, -1, new Par("path"), new Par("other")),
                "Move" => Func.Static(name, Delete, -1, new Par("path"), new Par("other")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}

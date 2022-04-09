using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.IO;
using System.Linq;

namespace Dyalect.Library.IO
{
    public sealed class DyPathTypeInfo : DyForeignTypeInfo<IOModule>
    {
        public override string TypeName => "Path";

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        private DyObject GetFullPath(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();
            return new DyString(Path.GetFullPath(path.GetString()));
        }

        private DyObject GetParentDirectory(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();
            var dir = Path.GetDirectoryName(path.GetString());
            return dir is null ? DyNil.Instance : new DyString(dir);
        }

        private DyObject GetExtension(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();
            var dir = Path.GetExtension(path.GetString());
            return dir is null ? DyNil.Instance : new DyString(dir);
        }

        private DyObject GetFileName(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();
            var dir = Path.GetFileName(path.GetString());
            return dir is null ? DyNil.Instance : new DyString(dir);
        }

        private DyObject GetPathRoot(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();
            var dir = Path.GetPathRoot(path.GetString());
            return dir is null ? DyNil.Instance : new DyString(dir);
        }

        private DyObject PathExists(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();

            try
            {
                var str = path.GetString();
                return Directory.Exists(str) || File.Exists(str) ? DyBool.True : DyBool.False;
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(path);
            }
        }

        private DyObject GetFileNameWithoutExtension(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();
            var dir = Path.GetFileNameWithoutExtension(path.GetString());
            return dir is null ? DyNil.Instance : new DyString(dir);
        }

        private DyObject Combine(ExecutionContext ctx, DyObject path1, DyObject path2)
        {
            if (!path1.IsString(ctx)) return Default();
            if (!path2.IsString(ctx)) return Default();
            string dir;
            
            try
            {
                dir = Path.Combine(path1.GetString(), path2.GetString());
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue();
            }
            
            return new DyString(dir);
        }

        private DyObject EnumerateFiles(ExecutionContext ctx, DyObject path, DyObject mask)
        {
            if (!path.IsString(ctx)) return Default();
            if (mask.NotNil() && !mask.IsString(ctx)) return Default();
            
            try
            {
                var seq = mask.NotNil()
                    ? Directory.EnumerateFiles(path.GetString(), mask.GetString())
                    : Directory.EnumerateFiles(path.GetString());
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

        private DyObject EnumerateDirectories(ExecutionContext ctx, DyObject path, DyObject mask)
        {
            if (!path.IsString(ctx)) return Default();
            if (mask.NotNil() && !mask.IsString(ctx)) return Default();

            try
            {
                var seq = mask.NotNil()
                    ? Directory.EnumerateDirectories(path.GetString(), mask.GetString())
                    : Directory.EnumerateDirectories(path.GetString());
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

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "GetCurrentDirectory" => Func.Static(name, _ => new DyString(Environment.CurrentDirectory)),
                "GetFullPath" => Func.Static(name, GetFullPath, -1, new Par("path")),
                "GetDirectory" => Func.Static(name, GetParentDirectory, -1, new Par("path")),
                "GetExtension" => Func.Static(name, GetExtension, -1, new Par("path")),
                "GetFileName" => Func.Static(name, GetFileName, -1, new Par("path")),
                "GetPathRoot" => Func.Static(name, GetPathRoot, -1, new Par("path")),
                "Exists" => Func.Static(name, PathExists, -1, new Par("path")),
                "GetFileNameWithoutExtension" => Func.Static(name, GetFileNameWithoutExtension, -1, new Par("path")),
                "Combine" => Func.Static(name, Combine, -1, new Par("path"), new Par("other")),
                "EnumerateFiles" => Func.Static(name, EnumerateFiles, -1, new Par("path"), new Par("mask", DyNil.Instance)),
                "EnumerateDirectories" => Func.Static(name, EnumerateDirectories, -1, new Par("path"), new Par("mask", DyNil.Instance)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}

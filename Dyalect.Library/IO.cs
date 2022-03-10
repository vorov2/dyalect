using Dyalect.Library.Types;
using Dyalect.Linker;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.IO;
using System.Text;

namespace Dyalect.Library
{
    [DyUnit("io")]
    public sealed class IOModule : ForeignUnit
    {
        private readonly Reference<Core> core;

        public IOModule()
        {
            core = AddReference<Core>();
        }

        [Function("readAllText")]
        public string ReadAllText(string path, [Default(0)] int encoding)
        {
            if (encoding == 0)
                encoding = Encoding.UTF8.CodePage;

            return File.ReadAllText(path, Encoding.GetEncoding(encoding));
        }

        [Function("writeAllText")]
        public object? WriteAllText(string path, string data, [Default(0)] int encoding)
        {
            if (encoding == 0)
                encoding = Encoding.UTF8.CodePage;

            File.WriteAllText(path, data, Encoding.GetEncoding(encoding));
            return null;
        }

        [Function("readAllBytes")]
        public DyObject ReadAllBytes(ExecutionContext ctx, DyObject arg)
        {
            if (arg.TypeId != DyType.String)
                return ctx.InvalidType(arg);

            return core.Value.ByteArray.Create(File.ReadAllBytes(arg.GetString()));
        }

        [Function("writeAllBytes")]
        public object? WriteAllBytes(string path, DyByteArray arr)
        {
            File.WriteAllBytes(path, arr.GetBytes());
            return null;
        }
    }
}

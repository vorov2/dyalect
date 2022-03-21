using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyalect.Library.Strings;

namespace Dyalect.Library.Core
{
    internal class DyDateTimeTypeInfo : DyForeignTypeInfo
    {
        public override string TypeName => "DateTime";

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Add | SupportedOperations.Sub;

        private DyObject Parse(ExecutionContext ctx, DyObject format, DyObject value)
        {
            if (value.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, value);

            if (format.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, format);

            try
            {
                return new DyDateTime(this, DateTime.ParseExact(value.GetString(), format.GetString(), CI.Default));
            }
            catch (FormatException)
            {
                return ctx.Fail(nameof(Errors.ParsingFailed), Errors.ParsingFailed);
            }
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch 
            {
                "Parse" => Func.Static(name, Parse, -1, new Par("format"), new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}

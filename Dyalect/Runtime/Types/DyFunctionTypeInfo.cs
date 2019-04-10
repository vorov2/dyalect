using System;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyFunctionTypeInfo : DyTypeInfo
    {
        public static readonly DyFunctionTypeInfo Instance = new DyFunctionTypeInfo();

        private DyFunctionTypeInfo() : base(StandardType.Function)
        {

        }

        public override string TypeName => StandardType.FunctionName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);

        public override bool CanConvertFrom(Type type) => false;

        public override DyObject ConvertFrom(object obj, Type type, ExecutionContext ctx) =>
            Err.OperationNotSupported(nameof(ConvertFrom), TypeName).Set(ctx);

        public override bool CanConvertTo(Type type) => false;

        public override object ConvertTo(DyObject obj, Type type, ExecutionContext ctx) =>
            Err.OperationNotSupported(nameof(ConvertTo), TypeName).Set(ctx);
    }
}

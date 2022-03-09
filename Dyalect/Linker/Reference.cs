using Dyalect.Compiler;

namespace Dyalect.Linker
{
    public sealed class Reference<T> where T : ForeignUnit
    {
        private readonly Reference @ref;

        public Reference(Reference @ref) => this.@ref = @ref;

        public T Value
        {
            get
            {
                if (@ref.Instance is null)
                    throw new DyException($"Reference \"{@ref.ModuleName}\" not initialized.");

                return (T)@ref.Instance;
            }
        }
    }
}

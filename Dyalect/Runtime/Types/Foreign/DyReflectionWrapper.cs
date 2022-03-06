using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public sealed class DyReflectionWrapper : DyWrapper
    {
        public DyReflectionWrapper(object instance) : base(Convert(instance)) { }

        private static Dictionary<string, DyObject> Convert(object instance)
        {
            var typ = instance.GetType();
            var map = new Dictionary<string, DyObject>();

            foreach (var pi in typ.GetProperties())
            {
                var val = pi.GetValue(instance, null);
                map[pi.Name] = TypeConverter.ConvertFrom(val);
            }

            return map;
        }
    }
}

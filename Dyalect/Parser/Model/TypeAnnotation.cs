using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class TypeAnnotation : IEnumerable<Qualident>
    {
        public Qualident Qualident { get; }

        public TypeAnnotation? Next { get; }
        public TypeAnnotation(Qualident qualident, TypeAnnotation next) =>
                (Qualident, Next) = (qualident, next);

        public void ToString(StringBuilder sb)
        {
            var ta = this;

            while (ta is not null)
            {
                sb.Append(Qualident.ToString());
                sb.Append(' ');
                ta = ta.Next;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public IEnumerator<Qualident> GetEnumerator()
        {
            var ta = this;

            while (ta is not null)
            {
                yield return ta.Qualident;
                ta = ta.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

using Dyalect.Runtime.Types;
using System.Text.RegularExpressions;

namespace Dyalect.Library.Core
{
    public class DyRegexCapture : DyObject
    {
        private readonly Capture capture;

        public DyRegexCapture(Capture capture) : base(DyType.Object) => this.capture = capture;

        public override SupportedOperations Supports() => SupportedOperations.Get;

        public override int GetHashCode() => capture.GetHashCode();

        public override object ToObject() => capture;

        public override string ToString() => capture.Value;

        public override bool Equals(DyObject? other) => other is DyRegexCapture c && c.capture == capture;

        protected internal override object? GetItem(string key) =>
            key switch
            {
                "index" => capture.Index,
                "length" => capture.Length,
                "value" => capture.Value,
                _ => base.GetItem(key)
            };
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Debug
{
    public sealed class CallStackTrace : IEnumerable<CallFrame>
    {
        private readonly List<CallFrame> frames;

        internal CallStackTrace(List<CallFrame> frames) => this.frames = frames;

        public IEnumerator<CallFrame> GetEnumerator() => frames.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public CallFrame this[int index]
        {
            get => frames[index];
        }

        public int FrameCount
        {
            get => frames.Count;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var cf in this)
            {
                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append(cf.ToString());
            }

            return sb.ToString();
        }
    }
}

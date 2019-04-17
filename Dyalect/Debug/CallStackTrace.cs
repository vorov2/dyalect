using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Debug
{
    public sealed class CallStackTrace : IEnumerable<CallFrame>
    {
        private readonly List<CallFrame> frames;

        internal CallStackTrace(List<CallFrame> frames)
        {
            this.frames = frames;
        }

        public IEnumerator<CallFrame> GetEnumerator()
        {
            return frames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public CallFrame this[int index]
        {
            get { return frames[index]; }
        }

        public int FrameCount
        {
            get { return frames.Count; }
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

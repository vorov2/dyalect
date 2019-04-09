using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Debug
{
    public sealed class CallStackTrace : IEnumerable<CallFrame>
    {
        private List<CallFrame> frames;

        internal CallStackTrace(string currentFile, int currentLine, int currentCol, List<CallFrame> frames)
        {
            File = currentFile;
            Line = currentLine;
            Column = currentCol;
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

        public string File { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var cf in this)
                sb.AppendLine(cf.ToString());

            return sb.ToString();
        }
    }
}

namespace Dyalect.Parser
{
    internal sealed class StringBuffer : SourceBuffer
    {
        private readonly char[] buffer;
        private readonly int bufferLen;
        private int bufferPosition;

        public StringBuffer(string value)
        {
            buffer = value.ToCharArray();
            bufferLen = buffer.Length;
        }

        public StringBuffer(string value, string fileName) : this(value)
        {
            this.fileName = fileName.Replace('\\', '/');
        }

        protected internal override int Read() =>
            bufferPosition < bufferLen ? buffer[bufferPosition++] : EOF;

        protected internal override int Peek()
        {
            var curPos = Pos;
            var ch = Read();
            Pos = curPos;
            return ch;
        }

        protected internal override string GetString(int start, int end)
        {
            var len = 0;
            var buf = new char[end - start];
            var oldPos = Pos;
            Pos = start;

            while (Pos < end)
                buf[len++] = (char)Read();

            Pos = oldPos;
            return new string(buf, 0, len);
        }

        protected internal override int Pos
        {
            get => bufferPosition;
            set
            {
                if (value < 0 || value > bufferLen)
                    throw new DyException($"End of file, position: {value}.", null);

                bufferPosition = value;
            }
        }

        private readonly string? fileName;
        public override string? FileName => fileName;
    }
}

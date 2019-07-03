namespace Dyalect.Parser
{
    internal sealed class StringBuffer : SourceBuffer
    {
        private readonly char[] buffer;
        private int bufferPosition;
        private int bufferLen;

        public StringBuffer(string value)
        {
            this.buffer = value.ToCharArray();
            this.bufferLen = this.buffer.Length;
        }

        public StringBuffer(string value, string fileName) : this(value)
        {
            this.fileName = fileName.Replace('\\', '/');
        }

        internal protected override int Read()
        {
            if (this.bufferPosition < this.bufferLen)
                return this.buffer[this.bufferPosition++];
            else
                return EOF;
        }

        internal protected override int Peek()
        {
            var curPos = Pos;
            var ch = Read();
            Pos = curPos;
            return ch;
        }

        internal protected override string GetString(int start, int end)
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

        internal protected override int Pos
        {
            get { return this.bufferPosition; }
            set
            {
                if (value < 0 || value > bufferLen)
                    throw new DyException(string.Format("Выход за пределы диапазона в буфере парсера MScript, позиция: {0}.", value), null);

                this.bufferPosition = value;
            }
        }

        private string fileName;
        public override string FileName => fileName;
    }
}

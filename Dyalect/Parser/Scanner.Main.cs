namespace Dyalect.Parser
{
    internal sealed partial class Scanner
    {
        private const char EOL = '\n';
        private const int EOFSYM = 0;
        private static readonly Map start = null!;
        private readonly SourceBuffer buffer;
        private Token t = null!;
        private int ch;
        private int pos = -1;
        private int charPos = -1;
        private int col;
        private int line = 1;
        private int oldEols;
        private Token tokens;
        private Token pt;
        private char[] tval = new char[128];
        private int tlen;

        public SourceBuffer Buffer => buffer;

        public Scanner(SourceBuffer buffer)
        {
            this.buffer = buffer;
            NextCh();
            pt = tokens = new Token();
        }

        private void SetScannerBehindT()
        {
            buffer.Pos = t.pos;
            NextCh();
            line = t.line;
            col = t.col;
            charPos = t.charPos;

            for (int i = 0; i < tlen; i++)
                NextCh();
        }

        public Token Scan()
        {
            if (tokens.next == null)
                return NextToken();
            else
            {
                pt = tokens = tokens.next;
                return tokens;
            }
        }

        public Token Peek()
        {
            do
            {
                if (pt.next == null)
                    pt.next = NextToken();

                pt = pt.next;
            }
            while (pt.kind > maxT);

            return pt;
        }

        public void ResetPeek()
        {
            pt = tokens;
        }
    }
}

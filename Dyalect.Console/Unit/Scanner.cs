
#nullable disable
using System;
using System.IO;
using System.Collections;
using Buffer = Dyalect.Units.SourceBuffer;


namespace Dyalect.Units
{
    partial class Scanner
    {
	const int maxT = 18;
	const int noSym = 18;


        static Scanner()
        {
            start = new Map(128);
		for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 49; i <= 57; ++i) start[i] = 29;
		for (int i = 33; i <= 33; ++i) start[i] = 28;
		for (int i = 37; i <= 38; ++i) start[i] = 28;
		for (int i = 40; i <= 45; ++i) start[i] = 28;
		for (int i = 47; i <= 47; ++i) start[i] = 28;
		for (int i = 58; i <= 59; ++i) start[i] = 28;
		for (int i = 61; i <= 61; ++i) start[i] = 28;
		for (int i = 64; i <= 64; ++i) start[i] = 28;
		for (int i = 92; i <= 92; ++i) start[i] = 28;
		for (int i = 94; i <= 94; ++i) start[i] = 28;
		for (int i = 96; i <= 96; ++i) start[i] = 28;
		for (int i = 124; i <= 124; ++i) start[i] = 28;
		for (int i = 126; i <= 126; ++i) start[i] = 28;
		start[95] = 30; 
		start[35] = 2; 
		start[48] = 31; 
		start[46] = 32; 
		start[34] = 19; 
		start[39] = 22; 
		start[36] = 24; 
		start[60] = 45; 
		start[123] = 40; 
		start[125] = 41; 
		start[62] = 42; 
		start[91] = 43; 
		start[93] = 44; 
		start[Buffer.EOF] = -1;

        }

        private void NextCh()
        {
            if (oldEols > 0)
            {
                ch = EOL;
                oldEols--;
            }
            else
            {
                pos = buffer.Pos;
                ch = buffer.Read();
                col++;
                charPos++;

                if (ch == '\r' && buffer.Peek() != '\n')
                    ch = EOL;

                if (ch == EOL)
                {
                    line++;
                    col = 0;
                }
            }

        }

        private void AddCh()
        {
            if (tlen >= tval.Length)
            {
                var newBuf = new char[2 * tval.Length];
                Array.Copy(tval, 0, newBuf, 0, tval.Length);
                tval = newBuf;
            }

            if (ch != SourceBuffer.EOF)
            {
			tval[tlen++] = (char) ch;
                NextCh();
            }
        }


	bool Comment0() {
		int level = 1, pos0 = pos, line0 = line, col0 = col, charPos0 = charPos;
		NextCh();
		if (ch == '/') {
			NextCh();
			for(;;) {
				if (ch == 10) {
					level--;
					if (level == 0) { oldEols = line - line0; NextCh(); return true; }
					NextCh();
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0; charPos = charPos0;
		}
		return false;
	}

	bool Comment1() {
		int level = 1, pos0 = pos, line0 = line, col0 = col, charPos0 = charPos;
		NextCh();
		if (ch == '*') {
			NextCh();
			for(;;) {
				if (ch == '*') {
					NextCh();
					if (ch == '/') {
						level--;
						if (level == 0) { oldEols = line - line0; NextCh(); return true; }
						NextCh();
					}
				} else if (ch == '/') {
					NextCh();
					if (ch == '*') {
						level++; NextCh();
					}
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0; charPos = charPos0;
		}
		return false;
	}


        private void CheckLiteral()
        {
		switch (t.val) {
			case "test": t.kind = 10; break;
			case "init": t.kind = 11; break;
			default: break;
		}
        }

        private Token NextToken()
        {
            var eol = false;
            while (ch == ' ' ||
			ch >= 9 && ch <= 10 || ch == 13
            )
            {
                if (ch == '\r' || ch == '\n')
                    eol = true;

                NextCh();
            }
		if (ch == '/' && Comment0() ||ch == '/' && Comment1()) return NextToken();

            int recKind = noSym;
            int recEnd = pos;
            t = new Token();
            t.pos = pos;
            t.col = col;
            t.line = line;
            t.charPos = charPos;
            t.AfterEol = eol;
            var state = start.ContainsKey(ch) ? (int)start[ch] : 0;
            tlen = 0;
            AddCh();

            switch (state)
            {
                case -1:
                    t.kind = EOFSYM;
                    break;
                case 0:
                    if (recKind != noSym)
                    {
                        tlen = recEnd - t.pos;
                        SetScannerBehindT();
                    }

                    t.kind = recKind;
                    break;
    			case 1:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 2:
				if (ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 3;}
				else if (ch == '_') {AddCh(); goto case 33;}
				else {goto case 0;}
			case 3:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 3;}
				else {t.kind = 2; break;}
			case 4:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 5;}
				else {goto case 0;}
			case 5:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 5;}
				else {t.kind = 3; break;}
			case 6:
				recEnd = pos; recKind = 4;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 6;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 18;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 7;}
				else {t.kind = 4; break;}
			case 7:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 8;}
				else {goto case 0;}
			case 8:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
				else {goto case 0;}
			case 9:
				recEnd = pos; recKind = 4;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 18;}
				else {t.kind = 4; break;}
			case 10:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 11;}
				else {goto case 0;}
			case 11:
				recEnd = pos; recKind = 4;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 11;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 18;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 12;}
				else {t.kind = 4; break;}
			case 12:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 14;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 13;}
				else {goto case 0;}
			case 13:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 14;}
				else {goto case 0;}
			case 14:
				recEnd = pos; recKind = 4;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 14;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 18;}
				else {t.kind = 4; break;}
			case 15:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 16;}
				else {goto case 0;}
			case 16:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else {goto case 0;}
			case 17:
				recEnd = pos; recKind = 4;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 18;}
				else {t.kind = 4; break;}
			case 18:
				{t.kind = 4; break;}
			case 19:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 19;}
				else if (ch == '"') {AddCh(); goto case 21;}
				else if (ch == 92) {AddCh(); goto case 20;}
				else {goto case 0;}
			case 20:
				if (ch == '"' || ch >= 39 && ch <= '(' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == 92 || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 19;}
				else {goto case 0;}
			case 21:
				{t.kind = 5; break;}
			case 22:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 22;}
				else if (ch == 39) {AddCh(); goto case 23;}
				else if (ch == 92) {AddCh(); goto case 34;}
				else {goto case 0;}
			case 23:
				{t.kind = 6; break;}
			case 24:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 25;}
				else {goto case 0;}
			case 25:
				recEnd = pos; recKind = 7;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 25;}
				else {t.kind = 7; break;}
			case 26:
				if (ch <= 92 || ch >= '^' && ch <= 65535) {AddCh(); goto case 26;}
				else if (ch == ']') {AddCh(); goto case 35;}
				else {goto case 0;}
			case 27:
				if (ch == '>') {AddCh(); goto case 26;}
				else {goto case 0;}
			case 28:
				recEnd = pos; recKind = 9;
				if (ch == '!' || ch >= '%' && ch <= '&' || ch >= '(' && ch <= '/' || ch >= ':' && ch <= ';' || ch == '=' || ch == '@' || ch == 92 || ch == '^' || ch == '`' || ch == '|' || ch == '~') {AddCh(); goto case 28;}
				else {t.kind = 9; break;}
			case 29:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 29;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 18;}
				else if (ch == '.') {AddCh(); goto case 10;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 15;}
				else {t.kind = 3; break;}
			case 30:
				recEnd = pos; recKind = 1;
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 36;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 31:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 29;}
				else if (ch == 'X' || ch == 'x') {AddCh(); goto case 4;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 18;}
				else if (ch == '.') {AddCh(); goto case 10;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 15;}
				else {t.kind = 3; break;}
			case 32:
				recEnd = pos; recKind = 9;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 6;}
				else if (ch == '!' || ch >= '%' && ch <= '&' || ch >= '(' && ch <= '/' || ch >= ':' && ch <= ';' || ch == '=' || ch == '@' || ch == 92 || ch == '^' || ch == '`' || ch == '|' || ch == '~') {AddCh(); goto case 28;}
				else {t.kind = 9; break;}
			case 33:
				recEnd = pos; recKind = 2;
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 37;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 3;}
				else {t.kind = 2; break;}
			case 34:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 22;}
				else if (ch == 92) {AddCh(); goto case 34;}
				else if (ch == 39) {AddCh(); goto case 38;}
				else {goto case 0;}
			case 35:
				if (ch <= '=' || ch >= '?' && ch <= 65535) {AddCh(); goto case 26;}
				else if (ch == '>') {AddCh(); goto case 39;}
				else {goto case 0;}
			case 36:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 36;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 37:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 37;}
				else {t.kind = 2; break;}
			case 38:
				recEnd = pos; recKind = 6;
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 22;}
				else if (ch == 39) {AddCh(); goto case 23;}
				else if (ch == 92) {AddCh(); goto case 34;}
				else {t.kind = 6; break;}
			case 39:
				recEnd = pos; recKind = 8;
				if (ch == ']') {AddCh(); goto case 27;}
				else {t.kind = 8; break;}
			case 40:
				{t.kind = 12; break;}
			case 41:
				{t.kind = 13; break;}
			case 42:
				{t.kind = 15; break;}
			case 43:
				{t.kind = 16; break;}
			case 44:
				{t.kind = 17; break;}
			case 45:
				recEnd = pos; recKind = 14;
				if (ch == '[') {AddCh(); goto case 26;}
				else {t.kind = 14; break;}

            }

            t.val = new string(tval, 0, tlen);
            return t;
        }
    }
}

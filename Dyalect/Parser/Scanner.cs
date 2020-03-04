
using System;
using System.IO;
using System.Collections;
using Buffer = Dyalect.Parser.SourceBuffer;


namespace Dyalect.Parser
{
    partial class Scanner
    {
	const int maxT = 84;
	const int noSym = 84;


        static Scanner()
        {
            start = new Map(128);
		for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 49; i <= 57; ++i) start[i] = 39;
		start[95] = 40; 
		start[35] = 2; 
		start[48] = 41; 
		start[46] = 70; 
		start[34] = 19; 
		start[39] = 22; 
		start[36] = 24; 
		start[60] = 71; 
		start[61] = 72; 
		start[44] = 29; 
		start[59] = 30; 
		start[58] = 31; 
		start[40] = 32; 
		start[41] = 33; 
		start[123] = 34; 
		start[125] = 35; 
		start[91] = 36; 
		start[93] = 37; 
		start[45] = 73; 
		start[43] = 74; 
		start[33] = 75; 
		start[126] = 38; 
		start[42] = 76; 
		start[47] = 77; 
		start[37] = 78; 
		start[124] = 79; 
		start[38] = 80; 
		start[62] = 81; 
		start[94] = 82; 
		start[63] = 83; 
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
			case "var": t.kind = 9; break;
			case "const": t.kind = 10; break;
			case "func": t.kind = 11; break;
			case "return": t.kind = 12; break;
			case "continue": t.kind = 13; break;
			case "break": t.kind = 14; break;
			case "yield": t.kind = 15; break;
			case "if": t.kind = 16; break;
			case "for": t.kind = 17; break;
			case "while": t.kind = 18; break;
			case "type": t.kind = 19; break;
			case "set": t.kind = 50; break;
			case "import": t.kind = 51; break;
			case "true": t.kind = 52; break;
			case "false": t.kind = 53; break;
			case "when": t.kind = 54; break;
			case "static": t.kind = 55; break;
			case "auto": t.kind = 56; break;
			case "private": t.kind = 57; break;
			case "match": t.kind = 59; break;
			case "nil": t.kind = 64; break;
			case "else": t.kind = 65; break;
			case "do": t.kind = 66; break;
			case "in": t.kind = 67; break;
			case "throw": t.kind = 68; break;
			case "try": t.kind = 69; break;
			case "catch": t.kind = 70; break;
			case "is": t.kind = 81; break;
			case "base": t.kind = 83; break;
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
				else if (ch == '_') {AddCh(); goto case 42;}
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
				else if (ch == 92) {AddCh(); goto case 43;}
				else {goto case 0;}
			case 20:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '(' || ch >= '*' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 20;}
				else if (ch == ')') {AddCh(); goto case 44;}
				else {goto case 0;}
			case 21:
				{t.kind = 5; break;}
			case 22:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 22;}
				else if (ch == 39) {AddCh(); goto case 23;}
				else if (ch == 92) {AddCh(); goto case 45;}
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
				else if (ch == ']') {AddCh(); goto case 46;}
				else {goto case 0;}
			case 27:
				if (ch == '>') {AddCh(); goto case 26;}
				else {goto case 0;}
			case 28:
				{t.kind = 20; break;}
			case 29:
				{t.kind = 22; break;}
			case 30:
				{t.kind = 23; break;}
			case 31:
				{t.kind = 24; break;}
			case 32:
				{t.kind = 26; break;}
			case 33:
				{t.kind = 27; break;}
			case 34:
				{t.kind = 28; break;}
			case 35:
				{t.kind = 29; break;}
			case 36:
				{t.kind = 30; break;}
			case 37:
				{t.kind = 31; break;}
			case 38:
				{t.kind = 35; break;}
			case 39:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 39;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 18;}
				else if (ch == '.') {AddCh(); goto case 10;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 15;}
				else {t.kind = 3; break;}
			case 40:
				recEnd = pos; recKind = 1;
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 47;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 41:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 39;}
				else if (ch == 'X' || ch == 'x') {AddCh(); goto case 4;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 18;}
				else if (ch == '.') {AddCh(); goto case 10;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 15;}
				else {t.kind = 3; break;}
			case 42:
				recEnd = pos; recKind = 2;
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 48;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 3;}
				else {t.kind = 2; break;}
			case 43:
				if (ch >= '!' && ch <= 39 || ch >= '*' && ch <= '~') {AddCh(); goto case 19;}
				else if (ch == '(') {AddCh(); goto case 20;}
				else {goto case 0;}
			case 44:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 44;}
				else if (ch == '"') {AddCh(); goto case 49;}
				else if (ch == 92) {AddCh(); goto case 43;}
				else {goto case 0;}
			case 45:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 22;}
				else if (ch == 92) {AddCh(); goto case 45;}
				else if (ch == 39) {AddCh(); goto case 50;}
				else {goto case 0;}
			case 46:
				if (ch <= '=' || ch >= '?' && ch <= 65535) {AddCh(); goto case 26;}
				else if (ch == '>') {AddCh(); goto case 51;}
				else {goto case 0;}
			case 47:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 47;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 48:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 48;}
				else {t.kind = 2; break;}
			case 49:
				recEnd = pos; recKind = 5;
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '(' || ch >= '*' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 20;}
				else if (ch == ')') {AddCh(); goto case 44;}
				else {t.kind = 5; break;}
			case 50:
				recEnd = pos; recKind = 6;
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 22;}
				else if (ch == 39) {AddCh(); goto case 23;}
				else if (ch == 92) {AddCh(); goto case 45;}
				else {t.kind = 6; break;}
			case 51:
				recEnd = pos; recKind = 8;
				if (ch == ']') {AddCh(); goto case 27;}
				else {t.kind = 8; break;}
			case 52:
				{t.kind = 41; break;}
			case 53:
				{t.kind = 42; break;}
			case 54:
				{t.kind = 45; break;}
			case 55:
				{t.kind = 46; break;}
			case 56:
				{t.kind = 58; break;}
			case 57:
				{t.kind = 60; break;}
			case 58:
				{t.kind = 61; break;}
			case 59:
				{t.kind = 71; break;}
			case 60:
				{t.kind = 72; break;}
			case 61:
				{t.kind = 73; break;}
			case 62:
				{t.kind = 74; break;}
			case 63:
				{t.kind = 75; break;}
			case 64:
				{t.kind = 76; break;}
			case 65:
				{t.kind = 77; break;}
			case 66:
				{t.kind = 78; break;}
			case 67:
				{t.kind = 79; break;}
			case 68:
				{t.kind = 80; break;}
			case 69:
				{t.kind = 82; break;}
			case 70:
				recEnd = pos; recKind = 21;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 6;}
				else if (ch == '.') {AddCh(); goto case 84;}
				else {t.kind = 21; break;}
			case 71:
				recEnd = pos; recKind = 44;
				if (ch == '[') {AddCh(); goto case 26;}
				else if (ch == '=') {AddCh(); goto case 55;}
				else if (ch == '<') {AddCh(); goto case 85;}
				else {t.kind = 44; break;}
			case 72:
				recEnd = pos; recKind = 25;
				if (ch == '>') {AddCh(); goto case 28;}
				else if (ch == '=') {AddCh(); goto case 52;}
				else {t.kind = 25; break;}
			case 73:
				recEnd = pos; recKind = 32;
				if (ch == '=') {AddCh(); goto case 60;}
				else {t.kind = 32; break;}
			case 74:
				recEnd = pos; recKind = 33;
				if (ch == '=') {AddCh(); goto case 59;}
				else {t.kind = 33; break;}
			case 75:
				recEnd = pos; recKind = 34;
				if (ch == '=') {AddCh(); goto case 53;}
				else {t.kind = 34; break;}
			case 76:
				recEnd = pos; recKind = 36;
				if (ch == '=') {AddCh(); goto case 61;}
				else {t.kind = 36; break;}
			case 77:
				recEnd = pos; recKind = 37;
				if (ch == '=') {AddCh(); goto case 62;}
				else {t.kind = 37; break;}
			case 78:
				recEnd = pos; recKind = 38;
				if (ch == '=') {AddCh(); goto case 63;}
				else {t.kind = 38; break;}
			case 79:
				recEnd = pos; recKind = 39;
				if (ch == '|') {AddCh(); goto case 57;}
				else if (ch == '=') {AddCh(); goto case 65;}
				else {t.kind = 39; break;}
			case 80:
				recEnd = pos; recKind = 40;
				if (ch == '&') {AddCh(); goto case 58;}
				else if (ch == '=') {AddCh(); goto case 64;}
				else {t.kind = 40; break;}
			case 81:
				recEnd = pos; recKind = 43;
				if (ch == '=') {AddCh(); goto case 54;}
				else if (ch == '>') {AddCh(); goto case 86;}
				else {t.kind = 43; break;}
			case 82:
				recEnd = pos; recKind = 47;
				if (ch == '=') {AddCh(); goto case 66;}
				else {t.kind = 47; break;}
			case 83:
				recEnd = pos; recKind = 63;
				if (ch == '?') {AddCh(); goto case 69;}
				else {t.kind = 63; break;}
			case 84:
				recEnd = pos; recKind = 62;
				if (ch == '.') {AddCh(); goto case 56;}
				else {t.kind = 62; break;}
			case 85:
				recEnd = pos; recKind = 48;
				if (ch == '=') {AddCh(); goto case 67;}
				else {t.kind = 48; break;}
			case 86:
				recEnd = pos; recKind = 49;
				if (ch == '=') {AddCh(); goto case 68;}
				else {t.kind = 49; break;}

            }

            t.val = new string(tval, 0, tlen);
            return t;
        }
    }
}

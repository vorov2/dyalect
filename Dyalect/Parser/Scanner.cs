
#nullable disable
using System;
using System.IO;
using System.Collections;
using Buffer = Dyalect.Parser.SourceBuffer;


namespace Dyalect.Parser
{
    partial class Scanner
    {
	const int maxT = 101;
	const int noSym = 101;


        static Scanner()
        {
            start = new Map(128);
		for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 2;
		for (int i = 49; i <= 57; ++i) start[i] = 55;
		start[95] = 56; 
		start[64] = 3; 
		start[35] = 5; 
		start[48] = 57; 
		start[46] = 75; 
		start[34] = 22; 
		start[39] = 25; 
		start[36] = 27; 
		start[60] = 76; 
		start[61] = 77; 
		start[44] = 32; 
		start[59] = 33; 
		start[58] = 34; 
		start[40] = 35; 
		start[41] = 36; 
		start[123] = 37; 
		start[125] = 38; 
		start[91] = 39; 
		start[93] = 40; 
		start[63] = 78; 
		start[43] = 58; 
		start[45] = 59; 
		start[42] = 79; 
		start[47] = 80; 
		start[37] = 81; 
		start[38] = 82; 
		start[124] = 83; 
		start[94] = 84; 
		start[62] = 85; 
		start[33] = 86; 
		start[126] = 52; 
		start[92] = 74; 
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
			case "auto": t.kind = 11; break;
			case "var": t.kind = 12; break;
			case "let": t.kind = 13; break;
			case "lazy": t.kind = 14; break;
			case "func": t.kind = 15; break;
			case "return": t.kind = 16; break;
			case "private": t.kind = 17; break;
			case "continue": t.kind = 18; break;
			case "break": t.kind = 19; break;
			case "yield": t.kind = 20; break;
			case "if": t.kind = 21; break;
			case "for": t.kind = 22; break;
			case "while": t.kind = 23; break;
			case "type": t.kind = 24; break;
			case "in": t.kind = 25; break;
			case "do": t.kind = 26; break;
			case "#region": t.kind = 69; break;
			case "#endregion": t.kind = 70; break;
			case "import": t.kind = 71; break;
			case "or": t.kind = 72; break;
			case "when": t.kind = 75; break;
			case "true": t.kind = 76; break;
			case "false": t.kind = 77; break;
			case "static": t.kind = 78; break;
			case "and": t.kind = 79; break;
			case "get": t.kind = 80; break;
			case "set": t.kind = 81; break;
			case "as": t.kind = 82; break;
			case "match": t.kind = 83; break;
			case "not": t.kind = 85; break;
			case "nil": t.kind = 86; break;
			case "else": t.kind = 87; break;
			case "many": t.kind = 88; break;
			case "throw": t.kind = 89; break;
			case "try": t.kind = 91; break;
			case "catch": t.kind = 92; break;
			case "is": t.kind = 95; break;
			case "yields": t.kind = 99; break;
			case "base": t.kind = 100; break;
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
				else {t.kind = 1; break;}
			case 2:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 2;}
				else {t.kind = 2; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 3:
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 4;}
				else {goto case 0;}
			case 4:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 4;}
				else {t.kind = 3; break;}
			case 5:
				if (ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 6;}
				else if (ch == '_') {AddCh(); goto case 60;}
				else {goto case 0;}
			case 6:
				recEnd = pos; recKind = 4;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 6;}
				else {t.kind = 4; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 7:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 8;}
				else {goto case 0;}
			case 8:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 8;}
				else {t.kind = 5; break;}
			case 9:
				recEnd = pos; recKind = 6;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 21;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 10;}
				else {t.kind = 6; break;}
			case 10:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 11;}
				else {goto case 0;}
			case 11:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
				else {goto case 0;}
			case 12:
				recEnd = pos; recKind = 6;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 21;}
				else {t.kind = 6; break;}
			case 13:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 14;}
				else {goto case 0;}
			case 14:
				recEnd = pos; recKind = 6;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 14;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 21;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 15;}
				else {t.kind = 6; break;}
			case 15:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 16;}
				else {goto case 0;}
			case 16:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else {goto case 0;}
			case 17:
				recEnd = pos; recKind = 6;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 21;}
				else {t.kind = 6; break;}
			case 18:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 20;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 19;}
				else {goto case 0;}
			case 19:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 20;}
				else {goto case 0;}
			case 20:
				recEnd = pos; recKind = 6;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 20;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 21;}
				else {t.kind = 6; break;}
			case 21:
				{t.kind = 6; break;}
			case 22:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 22;}
				else if (ch == '"') {AddCh(); goto case 24;}
				else if (ch == 92) {AddCh(); goto case 23;}
				else {goto case 0;}
			case 23:
				if (ch == '"' || ch >= 39 && ch <= '(' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == 92 || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 22;}
				else {goto case 0;}
			case 24:
				{t.kind = 7; break;}
			case 25:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 25;}
				else if (ch == 39) {AddCh(); goto case 26;}
				else if (ch == 92) {AddCh(); goto case 61;}
				else {goto case 0;}
			case 26:
				{t.kind = 8; break;}
			case 27:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 28;}
				else {goto case 0;}
			case 28:
				recEnd = pos; recKind = 9;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 28;}
				else {t.kind = 9; break;}
			case 29:
				if (ch <= 92 || ch >= '^' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch == ']') {AddCh(); goto case 62;}
				else {goto case 0;}
			case 30:
				if (ch == '>') {AddCh(); goto case 29;}
				else {goto case 0;}
			case 31:
				{t.kind = 27; break;}
			case 32:
				{t.kind = 29; break;}
			case 33:
				{t.kind = 30; break;}
			case 34:
				{t.kind = 31; break;}
			case 35:
				{t.kind = 33; break;}
			case 36:
				{t.kind = 34; break;}
			case 37:
				{t.kind = 35; break;}
			case 38:
				{t.kind = 36; break;}
			case 39:
				{t.kind = 37; break;}
			case 40:
				{t.kind = 38; break;}
			case 41:
				{t.kind = 39; break;}
			case 42:
				{t.kind = 40; break;}
			case 43:
				{t.kind = 41; break;}
			case 44:
				{t.kind = 42; break;}
			case 45:
				{t.kind = 43; break;}
			case 46:
				{t.kind = 44; break;}
			case 47:
				{t.kind = 45; break;}
			case 48:
				{t.kind = 46; break;}
			case 49:
				{t.kind = 47; break;}
			case 50:
				{t.kind = 48; break;}
			case 51:
				{t.kind = 49; break;}
			case 52:
				if (ch == '~') {AddCh(); goto case 53;}
				else {goto case 0;}
			case 53:
				if (ch == '~') {AddCh(); goto case 54;}
				else {goto case 0;}
			case 54:
				{t.kind = 53; break;}
			case 55:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 55;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 21;}
				else if (ch == '.') {AddCh(); goto case 13;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 18;}
				else {t.kind = 5; break;}
			case 56:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z') {AddCh(); goto case 2;}
				else if (ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 63;}
				else {t.kind = 2; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 57:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 55;}
				else if (ch == 'X' || ch == 'x') {AddCh(); goto case 7;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 21;}
				else if (ch == '.') {AddCh(); goto case 13;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 18;}
				else {t.kind = 5; break;}
			case 58:
				recEnd = pos; recKind = 51;
				if (ch == '=') {AddCh(); goto case 42;}
				else {t.kind = 51; break;}
			case 59:
				recEnd = pos; recKind = 50;
				if (ch == '=') {AddCh(); goto case 43;}
				else {t.kind = 50; break;}
			case 60:
				recEnd = pos; recKind = 4;
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 65;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 6;}
				else {t.kind = 4; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 61:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 25;}
				else if (ch == 92) {AddCh(); goto case 61;}
				else if (ch == 39) {AddCh(); goto case 66;}
				else {goto case 0;}
			case 62:
				if (ch <= '=' || ch >= '?' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch == '>') {AddCh(); goto case 67;}
				else {goto case 0;}
			case 63:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 63;}
				else {t.kind = 2; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 64:
				recEnd = pos; recKind = 54;
				if (ch == '=') {AddCh(); goto case 41;}
				else {t.kind = 54; break;}
			case 65:
				recEnd = pos; recKind = 4;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 65;}
				else {t.kind = 4; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 66:
				recEnd = pos; recKind = 8;
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 25;}
				else if (ch == 39) {AddCh(); goto case 26;}
				else if (ch == 92) {AddCh(); goto case 61;}
				else {t.kind = 8; break;}
			case 67:
				recEnd = pos; recKind = 10;
				if (ch == ']') {AddCh(); goto case 30;}
				else {t.kind = 10; break;}
			case 68:
				{t.kind = 60; break;}
			case 69:
				{t.kind = 61; break;}
			case 70:
				{t.kind = 64; break;}
			case 71:
				{t.kind = 65; break;}
			case 72:
				{t.kind = 74; break;}
			case 73:
				{t.kind = 97; break;}
			case 74:
				{t.kind = 98; break;}
			case 75:
				recEnd = pos; recKind = 28;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
				else if (ch == '.') {AddCh(); goto case 87;}
				else {t.kind = 28; break;}
			case 76:
				recEnd = pos; recKind = 63;
				if (ch == '[') {AddCh(); goto case 29;}
				else if (ch == '<') {AddCh(); goto case 88;}
				else if (ch == '=') {AddCh(); goto case 71;}
				else {t.kind = 63; break;}
			case 77:
				recEnd = pos; recKind = 32;
				if (ch == '>') {AddCh(); goto case 31;}
				else if (ch == '=') {AddCh(); goto case 68;}
				else {t.kind = 32; break;}
			case 78:
				recEnd = pos; recKind = 90;
				if (ch == '?') {AddCh(); goto case 64;}
				else {t.kind = 90; break;}
			case 79:
				recEnd = pos; recKind = 55;
				if (ch == '=') {AddCh(); goto case 44;}
				else {t.kind = 55; break;}
			case 80:
				recEnd = pos; recKind = 56;
				if (ch == '=') {AddCh(); goto case 45;}
				else {t.kind = 56; break;}
			case 81:
				recEnd = pos; recKind = 57;
				if (ch == '=') {AddCh(); goto case 46;}
				else {t.kind = 57; break;}
			case 82:
				if (ch == '&') {AddCh(); goto case 89;}
				else {goto case 0;}
			case 83:
				recEnd = pos; recKind = 73;
				if (ch == '|') {AddCh(); goto case 90;}
				else {t.kind = 73; break;}
			case 84:
				recEnd = pos; recKind = 96;
				if (ch == '^') {AddCh(); goto case 91;}
				else {t.kind = 96; break;}
			case 85:
				recEnd = pos; recKind = 62;
				if (ch == '>') {AddCh(); goto case 92;}
				else if (ch == '=') {AddCh(); goto case 70;}
				else {t.kind = 62; break;}
			case 86:
				recEnd = pos; recKind = 52;
				if (ch == '=') {AddCh(); goto case 69;}
				else {t.kind = 52; break;}
			case 87:
				recEnd = pos; recKind = 84;
				if (ch == '.') {AddCh(); goto case 72;}
				else if (ch == '<') {AddCh(); goto case 73;}
				else {t.kind = 84; break;}
			case 88:
				if (ch == '<') {AddCh(); goto case 93;}
				else {goto case 0;}
			case 89:
				recEnd = pos; recKind = 94;
				if (ch == '&') {AddCh(); goto case 94;}
				else {t.kind = 94; break;}
			case 90:
				recEnd = pos; recKind = 93;
				if (ch == '|') {AddCh(); goto case 95;}
				else {t.kind = 93; break;}
			case 91:
				if (ch == '^') {AddCh(); goto case 96;}
				else {goto case 0;}
			case 92:
				if (ch == '>') {AddCh(); goto case 97;}
				else {goto case 0;}
			case 93:
				recEnd = pos; recKind = 67;
				if (ch == '=') {AddCh(); goto case 50;}
				else {t.kind = 67; break;}
			case 94:
				recEnd = pos; recKind = 59;
				if (ch == '=') {AddCh(); goto case 47;}
				else {t.kind = 59; break;}
			case 95:
				recEnd = pos; recKind = 58;
				if (ch == '=') {AddCh(); goto case 48;}
				else {t.kind = 58; break;}
			case 96:
				recEnd = pos; recKind = 66;
				if (ch == '=') {AddCh(); goto case 49;}
				else {t.kind = 66; break;}
			case 97:
				recEnd = pos; recKind = 68;
				if (ch == '=') {AddCh(); goto case 51;}
				else {t.kind = 68; break;}

            }

            t.val = new string(tval, 0, tlen);
            return t;
        }
    }
}

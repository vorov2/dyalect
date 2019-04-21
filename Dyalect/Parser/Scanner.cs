
using System;
using System.IO;
using System.Collections;
using Buffer = Dyalect.Parser.SourceBuffer;


namespace Dyalect.Parser 
{
    partial class Scanner 
    {
	const int maxT = 70;
	const int noSym = 70;

    
        static Scanner() 
        {
            start = new Map(128);
		for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 49; i <= 57; ++i) start[i] = 35;
		start[95] = 36; 
		start[48] = 37; 
		start[46] = 60; 
		start[34] = 16; 
		start[39] = 22; 
		start[36] = 23; 
		start[61] = 61; 
		start[44] = 26; 
		start[59] = 27; 
		start[58] = 28; 
		start[40] = 29; 
		start[41] = 30; 
		start[123] = 31; 
		start[125] = 32; 
		start[91] = 33; 
		start[93] = 34; 
		start[43] = 62; 
		start[45] = 63; 
		start[42] = 64; 
		start[47] = 65; 
		start[37] = 66; 
		start[124] = 67; 
		start[38] = 68; 
		start[33] = 69; 
		start[35] = 41; 
		start[62] = 70; 
		start[60] = 71; 
		start[94] = 72; 
		start[126] = 73; 
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
			case "var": t.kind = 7; break;
			case "const": t.kind = 8; break;
			case "func": t.kind = 9; break;
			case "return": t.kind = 10; break;
			case "continue": t.kind = 11; break;
			case "break": t.kind = 12; break;
			case "yield": t.kind = 13; break;
			case "if": t.kind = 14; break;
			case "for": t.kind = 15; break;
			case "while": t.kind = 16; break;
			case "type": t.kind = 17; break;
			case "import": t.kind = 49; break;
			case "else": t.kind = 51; break;
			case "in": t.kind = 52; break;
			case "when": t.kind = 53; break;
			case "nil": t.kind = 67; break;
			case "true": t.kind = 68; break;
			case "false": t.kind = 69; break;
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
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 3;}
				else {goto case 0;}
			case 3:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 3;}
				else {t.kind = 2; break;}
			case 4:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 4;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 5;}
				else {t.kind = 3; break;}
			case 5:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 7;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 6;}
				else {goto case 0;}
			case 6:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 7;}
				else {goto case 0;}
			case 7:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 7;}
				else {t.kind = 3; break;}
			case 8:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
				else {goto case 0;}
			case 9:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 10;}
				else {t.kind = 3; break;}
			case 10:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 11;}
				else {goto case 0;}
			case 11:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
				else {goto case 0;}
			case 12:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
				else {t.kind = 3; break;}
			case 13:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 15;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 14;}
				else {goto case 0;}
			case 14:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 15;}
				else {goto case 0;}
			case 15:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 15;}
				else {t.kind = 3; break;}
			case 16:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 16;}
				else if (ch == '"') {AddCh(); goto case 21;}
				else if (ch == 92) {AddCh(); goto case 38;}
				else {goto case 0;}
			case 17:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 18;}
				else {goto case 0;}
			case 18:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 19;}
				else {goto case 0;}
			case 19:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 20;}
				else {goto case 0;}
			case 20:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 16;}
				else {goto case 0;}
			case 21:
				{t.kind = 4; break;}
			case 22:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= 65535) {AddCh(); goto case 22;}
				else if (ch == 39) {AddCh(); goto case 39;}
				else {goto case 0;}
			case 23:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 24;}
				else {goto case 0;}
			case 24:
				recEnd = pos; recKind = 6;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 24;}
				else {t.kind = 6; break;}
			case 25:
				{t.kind = 18; break;}
			case 26:
				{t.kind = 20; break;}
			case 27:
				{t.kind = 21; break;}
			case 28:
				{t.kind = 22; break;}
			case 29:
				{t.kind = 24; break;}
			case 30:
				{t.kind = 25; break;}
			case 31:
				{t.kind = 26; break;}
			case 32:
				{t.kind = 27; break;}
			case 33:
				{t.kind = 28; break;}
			case 34:
				{t.kind = 29; break;}
			case 35:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 35;}
				else if (ch == '.') {AddCh(); goto case 8;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 13;}
				else {t.kind = 2; break;}
			case 36:
				recEnd = pos; recKind = 1;
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 40;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 37:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 35;}
				else if (ch == 'X' || ch == 'x') {AddCh(); goto case 2;}
				else if (ch == '.') {AddCh(); goto case 8;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 13;}
				else {t.kind = 2; break;}
			case 38:
				if (ch == '"' || ch == 39 || ch == '0' || ch == 92 || ch == 'b' || ch == 'n' || ch == 'r' || ch == 't') {AddCh(); goto case 16;}
				else if (ch == 'u') {AddCh(); goto case 17;}
				else {goto case 0;}
			case 39:
				recEnd = pos; recKind = 5;
				if (ch == 39) {AddCh(); goto case 22;}
				else {t.kind = 5; break;}
			case 40:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 40;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 41:
				{t.kind = 38; break;}
			case 42:
				{t.kind = 39; break;}
			case 43:
				{t.kind = 40; break;}
			case 44:
				{t.kind = 43; break;}
			case 45:
				{t.kind = 44; break;}
			case 46:
				if (ch == '~') {AddCh(); goto case 47;}
				else {goto case 0;}
			case 47:
				{t.kind = 48; break;}
			case 48:
				if (ch == '.') {AddCh(); goto case 49;}
				else {goto case 0;}
			case 49:
				{t.kind = 50; break;}
			case 50:
				{t.kind = 54; break;}
			case 51:
				{t.kind = 55; break;}
			case 52:
				{t.kind = 56; break;}
			case 53:
				{t.kind = 57; break;}
			case 54:
				{t.kind = 58; break;}
			case 55:
				{t.kind = 59; break;}
			case 56:
				{t.kind = 60; break;}
			case 57:
				{t.kind = 61; break;}
			case 58:
				{t.kind = 62; break;}
			case 59:
				{t.kind = 63; break;}
			case 60:
				recEnd = pos; recKind = 19;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 4;}
				else if (ch == '.') {AddCh(); goto case 48;}
				else {t.kind = 19; break;}
			case 61:
				recEnd = pos; recKind = 23;
				if (ch == '>') {AddCh(); goto case 25;}
				else if (ch == '=') {AddCh(); goto case 42;}
				else {t.kind = 23; break;}
			case 62:
				recEnd = pos; recKind = 30;
				if (ch == '=') {AddCh(); goto case 50;}
				else {t.kind = 30; break;}
			case 63:
				recEnd = pos; recKind = 31;
				if (ch == '=') {AddCh(); goto case 51;}
				else {t.kind = 31; break;}
			case 64:
				recEnd = pos; recKind = 32;
				if (ch == '=') {AddCh(); goto case 52;}
				else {t.kind = 32; break;}
			case 65:
				recEnd = pos; recKind = 33;
				if (ch == '=') {AddCh(); goto case 53;}
				else {t.kind = 33; break;}
			case 66:
				recEnd = pos; recKind = 34;
				if (ch == '=') {AddCh(); goto case 54;}
				else {t.kind = 34; break;}
			case 67:
				if (ch == '|') {AddCh(); goto case 74;}
				else {goto case 0;}
			case 68:
				if (ch == '&') {AddCh(); goto case 75;}
				else {goto case 0;}
			case 69:
				recEnd = pos; recKind = 37;
				if (ch == '=') {AddCh(); goto case 43;}
				else {t.kind = 37; break;}
			case 70:
				recEnd = pos; recKind = 41;
				if (ch == '=') {AddCh(); goto case 44;}
				else if (ch == '>') {AddCh(); goto case 76;}
				else {t.kind = 41; break;}
			case 71:
				recEnd = pos; recKind = 42;
				if (ch == '=') {AddCh(); goto case 45;}
				else if (ch == '<') {AddCh(); goto case 77;}
				else {t.kind = 42; break;}
			case 72:
				if (ch == '^') {AddCh(); goto case 78;}
				else {goto case 0;}
			case 73:
				recEnd = pos; recKind = 66;
				if (ch == '~') {AddCh(); goto case 46;}
				else {t.kind = 66; break;}
			case 74:
				recEnd = pos; recKind = 64;
				if (ch == '|') {AddCh(); goto case 79;}
				else {t.kind = 64; break;}
			case 75:
				recEnd = pos; recKind = 65;
				if (ch == '&') {AddCh(); goto case 80;}
				else {t.kind = 65; break;}
			case 76:
				if (ch == '>') {AddCh(); goto case 81;}
				else {goto case 0;}
			case 77:
				if (ch == '<') {AddCh(); goto case 82;}
				else {goto case 0;}
			case 78:
				if (ch == '^') {AddCh(); goto case 83;}
				else {goto case 0;}
			case 79:
				recEnd = pos; recKind = 35;
				if (ch == '=') {AddCh(); goto case 56;}
				else {t.kind = 35; break;}
			case 80:
				recEnd = pos; recKind = 36;
				if (ch == '=') {AddCh(); goto case 55;}
				else {t.kind = 36; break;}
			case 81:
				recEnd = pos; recKind = 47;
				if (ch == '=') {AddCh(); goto case 59;}
				else {t.kind = 47; break;}
			case 82:
				recEnd = pos; recKind = 46;
				if (ch == '=') {AddCh(); goto case 58;}
				else {t.kind = 46; break;}
			case 83:
				recEnd = pos; recKind = 45;
				if (ch == '=') {AddCh(); goto case 57;}
				else {t.kind = 45; break;}

            }

            t.val = new string(tval, 0, tlen);
            return t;
        }
    }
}

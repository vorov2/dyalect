
#nullable disable
using System;
using System.Linq;
using System.Collections.Generic;
using Dyalect.Parser.Model;


namespace Dyalect.Units
{
    partial class InternalParser
    {
	public const int _EOF = 0;
	public const int _identToken = 1;
	public const int _directive = 2;
	public const int _intToken = 3;
	public const int _floatToken = 4;
	public const int _stringToken = 5;
	public const int _charToken = 6;
	public const int _implicitToken = 7;
	public const int _verbatimStringToken = 8;
	public const int _operatorToken = 9;
	public const int maxT = 18;




        private void Get()
        {
            for (;;)
            {
                t = la;
                la = scanner.Scan();

                if (la.kind <= maxT)
                {
                    ++errDist;
                    break;
                }

                la = t;
            }
        }

	void Test() {
		string name = null; 
		Expect(10);
		if (la.kind == 5) {
			Get();
			name = ParseString(); 
		} else if (la.kind == 11) {
			Get();
		} else SynErr(19);
		Expect(12);
		Console.WriteLine(name + " start " + t.line + ":" + t.col); 
		while (StartOf(1)) {
			Content();
		}
		Console.WriteLine(name + " end " + t.line + ":" + t.col); 
		Expect(13);
	}

	void Content() {
		switch (la.kind) {
		case 1: {
			Get();
			break;
		}
		case 11: {
			Get();
			break;
		}
		case 10: {
			Get();
			break;
		}
		case 14: {
			Get();
			break;
		}
		case 15: {
			Get();
			break;
		}
		case 16: {
			Get();
			break;
		}
		case 17: {
			Get();
			break;
		}
		case 2: {
			Get();
			break;
		}
		case 3: {
			Get();
			break;
		}
		case 4: {
			Get();
			break;
		}
		case 5: {
			Get();
			break;
		}
		case 6: {
			Get();
			break;
		}
		case 7: {
			Get();
			break;
		}
		case 8: {
			Get();
			break;
		}
		case 9: {
			Get();
			break;
		}
		case 12: {
			Parens();
			break;
		}
		default: SynErr(20); break;
		}
	}

	void Parens() {
		Expect(12);
		while (StartOf(1)) {
			Content();
		}
		Expect(13);
	}

	void Unit() {
		Test();
		while (la.kind == 10) {
			Test();
		}
	}



        public void Parse()
        {
            la = new Token();
            la.val = "";
            Get();
		Unit();
		Expect(0);

        }

        static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _T,_T,_x,_x}

        };

        private void SynErr(int line, int col, int n)
        {
            string s;

            switch (n)
            {
			case 0: s = "EOF expected"; break;
			case 1: s = "identToken expected"; break;
			case 2: s = "directive expected"; break;
			case 3: s = "intToken expected"; break;
			case 4: s = "floatToken expected"; break;
			case 5: s = "stringToken expected"; break;
			case 6: s = "charToken expected"; break;
			case 7: s = "implicitToken expected"; break;
			case 8: s = "verbatimStringToken expected"; break;
			case 9: s = "operatorToken expected"; break;
			case 10: s = "\"test\" expected"; break;
			case 11: s = "\"init\" expected"; break;
			case 12: s = "\"{\" expected"; break;
			case 13: s = "\"}\" expected"; break;
			case 14: s = "\"<\" expected"; break;
			case 15: s = "\">\" expected"; break;
			case 16: s = "\"[\" expected"; break;
			case 17: s = "\"]\" expected"; break;
			case 18: s = "??? expected"; break;
			case 19: s = "invalid Test"; break;
			case 20: s = "invalid Content"; break;

                default:
                    s = "unknown " + n;
                    break;
            }

            AddError(s, line, col);
        }
    }
}

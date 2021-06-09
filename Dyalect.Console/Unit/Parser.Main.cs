using Dyalect.Parser.Model;
using Dyalect.Strings;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dyalect.Units
{
    internal sealed partial class InternalParser
    {
        private const bool _T = true;
        private const bool _x = false;
        private const int minErrDist = 2;

        private readonly Scanner scanner;

        public Token t = null!;
        public Token la = null!;
        private int errDist = minErrDist;

        public string FileName { get; init; }

        public List<(string message, int line, int col)> Errors { get; } = new();
       
        public InternalParser(string filename, Scanner scanner)
        {
            FileName = filename;
            this.scanner = scanner;
        }

        private void AddError(string message, int line, int col)
        {
            Errors.Add((message, line, col));
        }

        private void SynErr(int n)
        {
            if (errDist >= minErrDist)
                SynErr(la.line, la.col, n);

            errDist = 0;
        }

        private void Expect(int n)
        {
            if (la.kind == n)
                Get();
            else
            {
                SynErr(n);
            }
        }

        private bool StartOf(int s)
        {
            return set[s, la.kind];
        }

        private void ExpectWeak(int n, int follow)
        {
            if (la.kind == n)
                Get();
            else
            {
                SynErr(n);

                while (!StartOf(follow))
                    Get();
            }
        }

        private bool WeakSeparator(int n, int syFol, int repFol)
        {
            int kind = la.kind;
            if (kind == n) { Get(); return true; }
            else if (StartOf(repFol)) { return false; }
            else
            {
                SynErr(n);
                while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind]))
                {
                    Get();
                    kind = la.kind;
                }
                return StartOf(syFol);
            }
        }

        private string? ParseString()
        {
            if (!EscapeCodeParser.Parse(scanner.Buffer.FileName, t, t.val,out var result))
                return null;

            return result;
        }
    }
}

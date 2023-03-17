using Dyalect.Parser.Model;
using Dyalect.Strings;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace Dyalect.Parser;

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

    public List<DImport> Imports { get; } = new();

    public DBlock Root { get; } = new(default);

    public List<BuildMessage> Errors { get; } = new();

    private readonly Stack<DFunctionDeclaration> functions = new();

    public InternalParser(string filename, Scanner scanner)
    {
        FileName = filename;
        this.scanner = scanner;
    }

    private void AddError(ParserError error, Location loc, params object[] args)
    {
        var str = string.Format(ParserErrors.ResourceManager.GetString(error.ToString()) ?? error.ToString(), args);
        AddError(new BuildMessage(str, BuildMessageType.Error, (int)error, loc.Line, loc.Column, this.scanner.Buffer.FileName));
    }

    private void Deprecated(string exp)
    {
        var detail = string.Format(ParserErrors.Deprecated, exp);
        AddError(new BuildMessage(detail, BuildMessageType.Error, (int)ParserError.Deprecated,
            t.line, t.col, this.scanner.Buffer.FileName));
    }

    private void AddError(string message, int line, int col)
    {
        ErrorProcessor.ProcessError(message, out var detail, out var code);
        AddError(new BuildMessage(detail, BuildMessageType.Error, (int)code, line, col, this.scanner.Buffer.FileName));
    }

    private void AddError(BuildMessage msg)
    {
        Errors.Add(msg);
    }

    private void SynErr(int n)
    {
        if (errDist >= minErrDist)
            SynErr(la.line, la.col, n);

        errDist = 0;
    }

    private void SemErr(string msg)
    {
        if (errDist >= minErrDist)
            SemErr(t.line, t.col, msg);
        errDist = 0;
    }

    private void SemErr(int line, int col, string s)
    {
        AddError(ParserError.SemanticError, new Location(line, col), s);
    }

    private void Expect(int n)
    {
        if (la.kind == n)
            Get();
        else
        {
            if (n == _semicolonToken
                && (t.kind == _curlyLeftToken || la.kind == 0 || la.kind == _curlyRightToken || la.AfterEol))
                return;

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

    private bool IsTypeName()
    {
        if (la.kind == _ucaseToken)
            return true;

        if (la.kind != _lcaseToken)
            return false;

        var xa = scanner.Peek();
        var res = xa.kind == _dotToken;
        scanner.ResetPeek();
        return res;
    }

    private bool IsPrivateScope()
    {
        if (la.kind != _privateToken)
            return false;

        var xa = scanner.Peek();
        var res = xa.kind == _curlyLeftToken;
        scanner.ResetPeek();
        return res;
    }

    private bool IsLabel()
    {
        if (la.kind == _varToken || la.kind == _letToken)
        {
            var xa = scanner.Peek();
            if (xa.kind != _lcaseToken && xa.kind != _ucaseToken && xa.kind != _stringToken)
            {
                scanner.ResetPeek();
                return false;
            }
        }
        else if (la.kind != _lcaseToken && la.kind != _ucaseToken && la.kind != _stringToken)
            return false;

        var na = scanner.Peek();
        scanner.ResetPeek();
        return na.kind == _colonToken;
    }

    private bool IsLabelPattern()
    {
        if (la.kind != _lcaseToken && la.kind != _ucaseToken && la.kind != _stringToken)
            return false;

        scanner.ResetPeek();
        return scanner.Peek().kind == _colonToken;
    }

    private bool IsTuple(bool allowFields = true)
    {
        if (la.kind != _parenLeftToken)
            return false;

        scanner.ResetPeek();

        if (scanner.Peek().kind is _varToken or _letToken)
            return true;

        scanner.ResetPeek();

        if (allowFields)
        {
            Token xt;
            if (((xt = scanner.Peek()).kind == _lcaseToken || xt.kind == _ucaseToken|| xt.kind == _stringToken)
                && scanner.Peek().kind == _colonToken)
                return true;
            scanner.ResetPeek();
        }

        var x = la;
        var balance = 0;

        while (true)
        {
            if (x.kind == _commaToken && balance == 1)
                return true;

            if (x.kind == _parenLeftToken || x.kind == _curlyLeftToken || x.kind == _squareLeftToken)
                balance++;
            else if (x.kind == _parenRightToken || x.kind == _curlyRightToken || x.kind == _squareRightToken)
            {
                balance--;
                if (balance == 0)
                    break;
            }

            x = scanner.Peek();
        }

        return false;
    }

    private bool IsFunction()
    {
        if (la.kind != _parenLeftToken && la.kind != _lcaseToken && la.kind != _ucaseToken)
            return false;

        scanner.ResetPeek();
        var x = la;

        if (la.kind == _lcaseToken || la.kind == _ucaseToken)
        {
            x = scanner.Peek();
            return x.kind == _arrowToken;
        }

        var balance = 0;

        while (x.kind != _arrowToken)
        {
            if (x.kind == _parenLeftToken)
                balance++;
            else if (x.kind == _parenRightToken)
            {
                balance--;
                if (balance == 0)
                    return scanner.Peek().kind == _arrowToken;
            }

            x = scanner.Peek();
        }

        return false;
    }

    private DStringLiteral? ParseString()
    {
        if (!EscapeCodeParser.Parse(scanner.Buffer.FileName, t, t.val, Errors, out var result, out var chunks))
            return null;

        return new DStringLiteral(t) { Value = result, Chunks = chunks };
    }

    private void ParseStringChunk(DStringLiteral lit)
    {
        if (lit is null || !EscapeCodeParser.Parse(scanner.Buffer.FileName, t, t.val, Errors, out var result, out var chunks))
            return;

        if (lit.Chunks is null) 
        {
            lit.Chunks = new();
            lit.Chunks.Add(new PlainStringChunk(lit.Value!));
            lit.Value = null;
        }

        if (result is not null)
            lit.Chunks.Add(new PlainStringChunk(result));
        else
            lit.Chunks.AddRange(chunks!);
    }

    private DStringLiteral ParseVerbatimString()
    {
        return new DStringLiteral(t) { Value = t.val[2..^2].Replace("]>]>", "]>") };
    }

    private string? ParseSimpleString()
    {
        if (!EscapeCodeParser.Parse(scanner.Buffer.FileName, t, t.val, Errors, out var result, out var chunks))
            return null;

        if (chunks is not null)
        {
            AddError(ParserError.CodeIslandsNotAllowed, t);
            return null;
        }

        return result;
    }

    private char ParseChar()
    {
        var str = ParseSimpleString();

        if (str == null)
            return '\0';

        if (str.Length > 1)
            AddError(ParserError.InvalidCharLiteral, new Location(t.line, t.col));

        return str[0];
    }

    private long ParseInteger()
    {
        var val = t.val;

        if (val.IndexOf('_') != -1)
            val = val.Replace("_", "");

        if (val.Length > 2 && val[0] == '0' && char.ToUpper(val[1]) == 'X')
            return long.Parse(val[2..], NumberStyles.HexNumber);

        return long.Parse(val);
    }

    private double ParseFloat()
    {
        var val = t.val;

        if (val.IndexOf('_') != -1)
            val = val.Replace("_", "");

        var c = val[^1];

        if (c == 'f' || c == 'F')
            return double.Parse(val[0..^1], InvariantCulture.NumberFormat);

        return double.Parse(val, InvariantCulture.NumberFormat);
    }
}

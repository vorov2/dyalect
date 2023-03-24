namespace Dyalect;

public class BuildMessage
{
    private const string ErrorFormat = "{0}({1},{2}): {3} D{4}: {5}";

    public string? File { get; internal set; }

    public string Message { get; protected set; }

    public BuildMessageType Type { get; }

    public int Code { get; protected set; }

    public int Line { get; protected set; }

    public int Column { get; protected set; }

    public BuildMessage(string message, BuildMessageType type, int code, int line, int col, string? file)
    {
        Message = message;
        Type = type;
        Code = code;
        Line = line;
        Column = col;
        File = file;
    }

    public override string ToString()
    {
        var stype = Type == BuildMessageType.Error ? "Error"
            : Type == BuildMessageType.Warning ? "Warning"
            : Type == BuildMessageType.Hint ? "Information"
            : "";
        var scode = Code.ToString().PadLeft(3, '0');
        return string.Format(ErrorFormat, GetFileName(), Line, Column, stype, scode, Message);
    }

    protected string GetFileName() => File ?? "<memory>";
}

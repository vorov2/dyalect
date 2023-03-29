using System.IO;

namespace Dyalect.Parser;

public abstract class SourceBuffer
{
    public abstract string? FileName { get; }

    protected internal abstract int Pos { get; set; }

    protected internal abstract string GetString(int start, int end);

    protected internal abstract int Peek();

    protected internal abstract int Read();

    public static SourceBuffer FromFile(string file)
    {
        using var sr = new StreamReader(File.OpenRead(file));
        return new StringBuffer(sr.ReadToEnd(), file);
    }

    public static SourceBuffer FromString(string str, string? file = null) =>
        new StringBuffer(str, file ?? "<memory>");
}
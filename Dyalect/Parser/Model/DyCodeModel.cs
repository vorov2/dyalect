using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DyCodeModel
{
    public DyCodeModel(DBlock root, DImport[] imports, string fileName) =>
        (Root, Imports, FileName) = (root, imports, fileName);

    public DImport[] Imports { get; }

    public DBlock Root { get; }

    public string FileName { get; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var i in Imports)
            sb.AppendLine(i.ToString());

        sb.AppendLine();
        Root.ToString(sb);
        return sb.ToString();
    }
}

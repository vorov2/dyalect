namespace Dyalect.Compiler;

public struct Label
{
    public static readonly Label Empty = new(EmptyLabel);
    internal const int EmptyLabel = -1;
    private readonly int index;

    public Label(int index) => this.index = index;

    public override string ToString() => "label:" + index.ToString();

    public bool IsEmpty() => index == EmptyLabel;

    public int GetIndex() => index;
}

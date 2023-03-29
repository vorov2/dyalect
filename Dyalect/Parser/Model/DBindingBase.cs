using System.Text;

namespace Dyalect.Parser.Model;

public abstract class DBindingBase : DNode
{
    protected DBindingBase(NodeType type, Location loc) : base(type, loc) { }

    public DPattern Pattern { get; internal set; } = null!;

    public DNode Init { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DTryCatch : DNode
{
    public DTryCatch(Location loc) : base(NodeType.TryCatch, loc) { }

    public DNode Expression { get; set; } = null!;

    public DNode Catch { get; set; } = null!;

    public DName? BindVariable { get; set; }

    internal override void ToString(StringBuilder sb)
    {
        sb.Append("try ");
        Expression.ToString(sb);
        sb.Append("catch ");

        if (BindVariable is not null)
        {
            BindVariable.ToString(sb);
            sb.Append(' ');
        }

        Catch.ToString(sb);
    }
}

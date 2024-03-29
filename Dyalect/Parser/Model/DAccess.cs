﻿using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DAccess : DNode, INamedNode
{
    public DAccess(Location loc) : base(NodeType.Access, loc) { }

    public DNode Target { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string NodeName => Name;

    public bool SpecialName { get; set; }

    public bool PrivateAccess { get; set; }

    internal override void ToString(StringBuilder sb)
    {
        Target.ToString(sb);

        if (PrivateAccess)
            sb.Append('!');
        else
            sb.Append('.');

        if (SpecialName)
            sb.Append("[" + Name + "]");
        else
            sb.Append(Name);
    }
}

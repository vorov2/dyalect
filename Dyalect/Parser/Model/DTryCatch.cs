﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTryCatch : DNode
    {
        public DTryCatch(Location loc) : base(NodeType.TryCatch, loc)
        {

        }

        public DNode Expression { get; set; }

        public DNode Catch { get; set; }

        public string BindVariable { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("try ");
            Expression.ToString(sb);
            sb.Append("catch ");

            if (BindVariable != null)
            {
                sb.Append(BindVariable);
                sb.Append(' ');
            }

            Catch.ToString(sb);
        }
    }
}

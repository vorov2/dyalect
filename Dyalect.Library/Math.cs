using Dyalect.Linker;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Text;
using M = System.Math;

namespace Dyalect.Library
{
    public sealed class Math : ForeignUnit
    {
        [Function("pow")]
        public double Pow(double x, double y) => M.Pow(x, y);
    }
}

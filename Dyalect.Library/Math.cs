using Dyalect.Linker;
using M = System.Math;

namespace Dyalect.Library
{
    [DyUnit("Math")]
    public sealed class Math : ForeignUnit
    {
        [Function("pow")]
        public double Pow(double x, double y) => M.Pow(x, y);
    }
}

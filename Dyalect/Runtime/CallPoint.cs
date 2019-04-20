using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    internal struct CallPoint
    {
        public CallPoint(int returnAddress, int unitId)
        {
            ReturnAddress = returnAddress;
            UnitId = unitId;
        }

        public readonly int ReturnAddress;

        public readonly int UnitId;
    }
}

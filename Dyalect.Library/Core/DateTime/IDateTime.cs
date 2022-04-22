using Dyalect.Runtime.Types;
namespace Dyalect.Library.Core;

public interface IDateTime : IDate, ITime
{
    DyObject GetDate(DyDateTypeInfo typeInfo);

    DyObject GetTime(DyTimeTypeInfo typeInfo);

    void AddHours(double value);

    void AddMinutes(double value);

    void AddSeconds(double value);

    void AddMilliseconds(double value);

    void AddTicks(long value);
}

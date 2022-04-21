namespace Dyalect.Library.Core;

public interface IZonedDateTime : IDateTime
{
    IInterval Interval { get; }
}

namespace Dyalect.Library.Core;

public interface ILocalDateTime : IDateTime
{
    IInterval Interval { get; }
}

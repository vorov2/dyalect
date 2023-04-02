using Dyalect.Runtime;

namespace Dyalect.Library.IO;

internal static class Extensions
{
    public static T? Handle<T>(this ExecutionContext ctx, Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            ctx.IOFailed(ex.Message);
            return default;
        }
    }

    public static void Handle(this ExecutionContext ctx, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            ctx.IOFailed(ex.Message);
        }
    }
}

using Dyalect.Runtime.Types;
using System.IO;
namespace Dyalect.Linker;

internal static class ObjectSerializer
{
    public static void Serialize(this DyObject obj, BinaryWriter writer)
    {
        switch (obj.TypeId)
        {
            case Dy.String:
                writer.Write(obj.TypeId);
                writer.Write(((DyString)obj).Value);
                break;
            case Dy.Nil:
                writer.Write(obj.TypeId);
                break;
            case Dy.Integer:
                writer.Write(obj.TypeId);
                writer.Write(((DyInteger)obj).Value);
                break;
            case Dy.Float:
                writer.Write(obj.TypeId);
                writer.Write(((DyFloat)obj).Value);
                break;
            case Dy.Char:
                writer.Write(obj.TypeId);
                writer.Write(((DyChar)obj).Value);
                break;
            case Dy.Bool:
                writer.Write(obj.TypeId);
                writer.Write(ReferenceEquals(obj, DyBool.True));
                break;
            default:
                throw new NotSupportedException();
        }
    }
}

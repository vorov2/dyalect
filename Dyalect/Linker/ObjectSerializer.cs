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
                writer.Write(obj.GetString());
                break;
            case Dy.Nil:
                writer.Write(obj.TypeId);
                break;
            case Dy.Integer:
                writer.Write(obj.TypeId);
                writer.Write(obj.GetInteger());
                break;
            case Dy.Float:
                writer.Write(obj.TypeId);
                writer.Write(obj.GetFloat());
                break;
            case Dy.Char:
                writer.Write(obj.TypeId);
                writer.Write(obj.GetChar());
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

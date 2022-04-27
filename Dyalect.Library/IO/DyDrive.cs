using Dyalect.Runtime.Types;
using System.IO;
namespace Dyalect.Library.IO;

public sealed class DyDrive : DyForeignObject
{
    internal readonly DriveInfo Value;

    public DyDrive(DyDriveTypeInfo typeInfo, DriveInfo value) : base(typeInfo) => Value = value;

    public override int GetHashCode() => Value.GetHashCode();

    public override object ToObject() => Value;

    public override string ToString() => Value.ToString();

    public override DyObject Clone() => this;

    public override bool Equals(DyObject? other) => other is DyDrive g && g.Value == Value;
}

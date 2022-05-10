namespace Dyalect.Linker;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class DyUnitAttribute : Attribute
{
    public DyUnitAttribute(string name) => Name = name;

    public string Name { get; }
}

namespace Dyalect.Codegen;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class DefaultAttribute : Attribute
{
    public DefaultAttribute() { }

    public DefaultAttribute(int _) { }

    public DefaultAttribute(long _) { }

    public DefaultAttribute(char _) { }

    public DefaultAttribute(string _) { }

    public DefaultAttribute(bool _) { }

    public DefaultAttribute(double _) { }
}

namespace Dyalect.Command
{
    public interface IOptionBag
    {
        string StartupPath { get; set; }

        string DefaultArgument { get; set; }
    }
}

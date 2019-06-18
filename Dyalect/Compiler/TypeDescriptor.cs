namespace Dyalect.Compiler
{
    public sealed class TypeDescriptor
    {
        public TypeDescriptor(string name, int id, bool autoGenCons)
        {
            Name = name;
            Id = id;
            AutoGenConstructors = autoGenCons;
        }

        public string Name { get; }
        public int Id { get; internal set; }
        public bool AutoGenConstructors { get; }
    }
}

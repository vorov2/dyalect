using Dyalect.Runtime.Types;

namespace Dyalect.Runtime;

public record struct RuntimeVar(string Name, DyObject Value);

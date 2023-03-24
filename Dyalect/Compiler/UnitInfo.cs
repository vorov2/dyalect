using System.Collections.Generic;

namespace Dyalect.Compiler;

internal record UnitInfo(int Handle, Dictionary<HashString, ScopeVar> ExportList);

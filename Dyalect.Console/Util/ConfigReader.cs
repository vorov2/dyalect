using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
namespace Dyalect.Util;

public static class ConfigReader
{
    public static IDictionary<string, object>? Read(string path)
    {
        if (!File.Exists(path))
        {
            Printer.Error($"Config file \"{path}\" not found.");
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));

            try
            {
                var dict = doc.RootElement.EnumerateObject().ToDictionary(e => e.Name,
                    e => e.Value.ValueKind == JsonValueKind.True ? true
                        : e.Value.ValueKind == JsonValueKind.False ? false
                        : e.Value.ValueKind == JsonValueKind.Number ? e.Value.GetDouble()
                        : (object)(e.Value.GetString() ?? ""));
                return dict;
            }
            catch (ArgumentException)
            {
                Printer.Error("Duplicate key in configuration file.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Printer.Error($"Error reading configuration file: {ex.Message}");
            return null;
        }
    }
}

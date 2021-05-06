using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dyalect.Library.Json;

namespace Dyalect.Util
{
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
                var json = new JsonParser(File.ReadAllText(path));
                var dict = json.Parse() as IDictionary<string, object>;

                if (!json.IsSuccess)
                {
                    Printer.PrintErrors(json.Errors!);
                    return null;
                }

                if (dict is not null)
                    return dict;
                
                Printer.Error("Invalid configuration file format.");
                return null;
            }
            catch (Exception ex)
            {
                Printer.Error($"Error reading configuration file: {ex.Message}");
                return null;
            }
        }
    }
}

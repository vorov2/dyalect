using System;
using System.Collections.Generic;
using System.IO;

namespace Dyalect.Util
{
    public static class ConfigReader
    {
        public static IDictionary<string, object> Read(string path)
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

                if (!json.Success)
                {
                    Printer.PrintErrors(json.Errors);
                    return null;
                }

                if (dict == null)
                {
                    Printer.Error("Invalid configuration file format.");
                    return null;
                }

                return dict;
            }
            catch (Exception ex)
            {
                Printer.Error($"Error reading configuration file: {ex.Message}");
                return null;
            }
        }
    }
}

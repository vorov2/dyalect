using System;
using System.Collections.Generic;
using System.IO;

namespace Dyalect.Util
{
    public static class ConfigReader
    {
        private const string FILENAME = "config.json";

        public static IDictionary<string, object> Read()
        {
            var path = Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), FILENAME);

            if (!File.Exists(path))
            {
                Config.SetDefault();
                Printer.Error($"Config file \"{FILENAME}\" not found.");
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
                Config.SetDefault();
                Printer.Error($"Error reading configuration file: {ex.Message}");
                return null;
            }
        }
    }
}

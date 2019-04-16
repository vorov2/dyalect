using System.Collections.Generic;

namespace Dyalect.Command
{
    public static class CommandLineParser
    {
        public static List<Option> Parse(string[] args)
        {
            var options = new List<Option>();
            string opt = null;
            string def = null;

            void AddOption(string key, string val) => options.Add(new Option(key, val?.Trim('"')));

            for (var i = 0; i < args.Length; i++)
            {
                var str = args[i].Trim(' ');
                var iswitch = str[0] == '-';

                if (!iswitch && opt == null)
                {
                    if (def != null)
                        throw new CommandException($"A default command line argument is already specifid: {def}");

                    AddOption(null, def = str);
                    continue;
                }

                if (str.Length > 0 && iswitch)
                {
                    if (opt != null)
                        AddOption(opt, null);
                    opt = str.Substring(1);
                    continue;
                }

                if (opt != null)
                {
                    AddOption(opt, str);
                    opt = null;
                    continue;
                }
            }

            if (opt != null)
                options.Add(new Option(opt));

            return options;
        }
    }
}

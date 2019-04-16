using System;

namespace Dyalect.Command
{
    public static class CommandLineParser
    {
        public static CommandLine Parse()
        {
            var args = Environment.GetCommandLineArgs();
            var cl = new CommandLine();
            string opt = null;

            for (var i = 0; i < args.Length; i++)
            {
                var str = args[i].Trim(' ');

                if (i == 0)
                {
                    cl.StartupPath = str;
                    continue;
                }

                if (str.Length > 0 && str[0] == '-')
                {
                    if (opt != null)
                        cl.Options.Add(new Option(opt));
                    opt = str.Substring(1);
                    continue;
                }

                if (opt != null)
                {
                    cl.Options.Add(new Option(opt,
                          int.TryParse(str, out var i4) ? i4
                        : bool.TryParse(str, out var i1) ? (object)i1
                        : str.Trim('"')));
                    opt = null;
                    continue;
                }

                if (cl.DefaultArgument != null)
                    throw new CommandException("Аргумент командной строки по умолчанию уже задан.");

                cl.DefaultArgument = str.Trim('"');
            }

            if (opt != null)
                cl.Options.Add(new Option(opt));

            return cl;
        }
    }
}

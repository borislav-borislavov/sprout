using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Common
{
    public static class AppArgs
    {
        public static Dictionary<string, string?> Args { get; private set; } = new();

        public static string SeedPath { get; set; }

        public static void Parse()
        {
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            
            Args = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.StartsWith("--"))
                {
                    var key = arg[2..];
                    var hasValue = i + 1 < args.Length && !args[i + 1].StartsWith("-");
                    Args[key] = hasValue ? args[++i] : null;
                }
                else
                {
                    Args[arg] = null;
                }
            }

            InterpretArgs();
        }

        private static void InterpretArgs()
        {
            if (Args.TryGetValue("seed", out var seedPath))
            {
                SeedPath = seedPath;
            }
        }
    }
}

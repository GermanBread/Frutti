// System
using System;
using System.IO;
using System.Collections.Generic;

namespace FruttiReborn
{
    public static class CmdlineParser {
        public record CmdArgs(FruttiFlags Flags, List<string> Directories, List<string> Files);
        public static CmdArgs ParseCmdline(string[] Args) {
            FruttiFlags _flags = 0;
            List<string> _dirs = new();
            List<string> _files = new();

            Array.ForEach(Args, x => {
                if (x.StartsWith("--") && Enum.TryParse(typeof(FruttiFlags), x[2..], true, out var y))
                    _flags |= (FruttiFlags)y;
                else if (Directory.Exists(x))
                    _dirs.Add(x);
                else if (File.Exists(x))
                    _files.Add(x);
            });

            return new CmdArgs(_flags, _dirs, _files);
        }
    }
}
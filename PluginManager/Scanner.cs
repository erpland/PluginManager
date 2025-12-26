using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PluginManager
{
    public class Scanner
    {
        public List<PluginFile> ScanPaths(List<string> searchPaths, List<string> validExtensions)
        {
            var results = new List<PluginFile>();
            var extSet = new HashSet<string>(validExtensions.Select(e => e.ToLower().Trim()));

            foreach (var path in searchPaths)
            {
                if (!Directory.Exists(path)) continue;
                try
                {
                    var dir = new DirectoryInfo(path);
                    var files = dir.EnumerateFiles("*.*", SearchOption.AllDirectories)
                                   .Where(f => extSet.Contains(f.Extension.ToLower()));

                    foreach (var f in files)
                    {
                        results.Add(new PluginFile
                        {
                            Name = Path.GetFileNameWithoutExtension(f.Name),
                            Extension = f.Extension.ToLower(),
                            Format = DetectFormat(f.FullName),
                            FullPath = f.FullName,
                            LastWriteTime = f.LastWriteTime
                        });
                    }
                }
                catch { }
            }
            return results;
        }

        private string DetectFormat(string path)
        {
            if (path.EndsWith(".vst3", StringComparison.OrdinalIgnoreCase)) return "VST3";
            if (path.EndsWith(".clap", StringComparison.OrdinalIgnoreCase)) return "CLAP";
            if (path.EndsWith(".aaxplugin", StringComparison.OrdinalIgnoreCase)) return "AAX";
            if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                return path.IndexOf("x86", StringComparison.OrdinalIgnoreCase) >= 0 ? "VST2 (32)" : "VST2 (64)";

            return path.Split('.').Last().ToUpper();
        }
    }
}
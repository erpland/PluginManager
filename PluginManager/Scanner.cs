using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager
{
    public class Scanner
    {
        private readonly HashSet<string> _extensions = new HashSet<string> { ".vst3", ".dll", ".clap", ".aaxplugin" };

        public List<PluginItem> ScanPaths(IEnumerable<string> paths)
        {
            var rawFiles = new List<FileInfo>();

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    try
                    {
                        var dir = new DirectoryInfo(path);
                        // Scan for files recursively
                        // NOTE: effectively ignores .vst3 folders, grabs the binary inside, or the bundle file
                        var files = dir.EnumerateFiles("*.*", SearchOption.AllDirectories)
                                       .Where(f => _extensions.Contains(f.Extension.ToLower()));
                        rawFiles.AddRange(files);
                    }
                    catch { /* Skip permission errors */ }
                }
            }

            return GroupPlugins(rawFiles);
        }

        private List<PluginItem> GroupPlugins(List<FileInfo> files)
        {
            // Group by filename without extension
            var grouped = files.GroupBy(f => Path.GetFileNameWithoutExtension(f.Name));
            var results = new List<PluginItem>();

            foreach (var group in grouped)
            {
                var formats = new HashSet<string>();
                DateTime lastMod = DateTime.MinValue;

                foreach (var f in group)
                {
                    string ext = f.Extension.ToLower();

                    if (ext == ".vst3") formats.Add("VST3");
                    else if (ext == ".clap") formats.Add("CLAP");
                    else if (ext == ".aaxplugin") formats.Add("AAX");
                    else if (ext == ".dll")
                    {
                        if (f.FullName.Contains("x86")) formats.Add("VST2 (32)");
                        else formats.Add("VST2 (64)");
                    }

                    if (f.LastWriteTime > lastMod) lastMod = f.LastWriteTime;
                }

                results.Add(new PluginItem
                {
                    Name = group.Key,
                    Formats = string.Join(", ", formats.OrderBy(x => x)),
                    LastUpdated = lastMod
                });
            }

            return results.OrderBy(x => x.Name).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PluginManager.Exporters
{
    public class MarkdownExporter : IPluginExporter
    {
        public string FileFilter => "Markdown File|*.md";

        public void Export(List<PluginItem> plugins, string filePath)
        {
            var sb = new StringBuilder();
            // Check if ANY item has a location (if false, we are in Unique Mode)
            bool includeLocation = plugins.Any(p => !string.IsNullOrEmpty(p.Location));

            // 1. Headers
            sb.Append("| Name | Formats | Updated");
            if (includeLocation) sb.Append(" | Location");
            sb.AppendLine(" |");

            // 2. Separator Line
            sb.Append("|---|---|---");
            if (includeLocation) sb.Append("|---");
            sb.AppendLine("|");

            // 3. Rows
            foreach (var p in plugins)
            {
                sb.Append($"| {p.Name} | {p.Formats} | {p.LastUpdatedStr}");

                if (includeLocation)
                {
                    string cleanLoc = p.Location?.Replace(Environment.NewLine, "<br>") ?? "";
                    sb.Append($" | {cleanLoc}");
                }

                sb.AppendLine(" |");
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
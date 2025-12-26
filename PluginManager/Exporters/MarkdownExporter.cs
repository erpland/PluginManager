using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PluginManager.Exporters
{
    public class MarkdownExporter : IPluginExporter
    {
        public string FileFilter => "Markdown File|*.md";

        public void Export(List<PluginItem> plugins, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Plugins System Report");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();
            sb.AppendLine("| Name | Formats | Last Updated |");
            sb.AppendLine("|---|---|---|");

            foreach (var p in plugins)
            {
                sb.AppendLine($"| {p.Name} | {p.Formats} | {p.LastUpdatedStr} |");
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
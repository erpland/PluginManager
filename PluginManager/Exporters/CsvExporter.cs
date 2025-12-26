using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PluginManager.Exporters
{
    public class CsvExporter : IPluginExporter
    {
        public string FileFilter => "CSV File|*.csv";

        public void Export(List<PluginItem> plugins, string filePath)
        {
            var sb = new StringBuilder();
            bool includeLocation = plugins.Any(p => !string.IsNullOrEmpty(p.Location));

            // 1. Headers
            sb.Append("Name,Formats,LastUpdated");
            if (includeLocation) sb.Append(",Location");
            sb.AppendLine();

            // 2. Rows
            foreach (var p in plugins)
            {
                // Escape quotes for CSV safety
                string safeName = $"\"{p.Name.Replace("\"", "\"\"")}\"";
                string safeFormats = $"\"{p.Formats}\"";

                sb.Append($"{safeName},{safeFormats},{p.LastUpdatedStr}");

                if (includeLocation)
                {
                    string safeLoc = $"\"{p.Location?.Replace("\"", "\"\"")}\"";
                    sb.Append($",{safeLoc}");
                }

                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
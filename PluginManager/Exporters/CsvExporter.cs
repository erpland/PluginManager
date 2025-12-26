using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PluginManager.Exporters
{
    public class CsvExporter : IPluginExporter
    {
        public string FileFilter => "CSV File|*.csv";

        public void Export(List<PluginItem> plugins, string filePath)
        {
            var sb = new StringBuilder();
            // Header
            sb.AppendLine("Name,Formats,LastUpdated");

            foreach (var p in plugins)
            {
                // Escape quotes if necessary, though simple plugin names rarely have them
                sb.AppendLine($"\"{p.Name}\",\"{p.Formats}\",{p.LastUpdatedStr}");
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
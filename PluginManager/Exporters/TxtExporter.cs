using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PluginManager.Exporters
{
    public class TxtExporter : IPluginExporter
    {
        public string FileFilter => "Text File|*.txt";

        public void Export(List<PluginItem> plugins, string filePath)
        {
            var sb = new StringBuilder();
            foreach (var p in plugins)
            {
                sb.AppendLine(p.Name);
            }
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
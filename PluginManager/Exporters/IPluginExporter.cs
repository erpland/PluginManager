using System.Collections.Generic;

namespace PluginManager.Exporters
{
    public interface IPluginExporter
    {
        // Property to get the file filter (e.g. "Markdown File|*.md")
        string FileFilter { get; }

        // The action
        void Export(List<PluginItem> plugins, string filePath);
    }
}
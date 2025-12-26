using System.Collections.Generic;

namespace PluginManager.Exporters
{
    public interface IPluginExporter
    {
        string FileFilter { get; }
        void Export(List<PluginItem> plugins, string filePath);
    }
}
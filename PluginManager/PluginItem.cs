using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager
{
    public class PluginItem
    {
        public string Name { get; set; }
        public string Formats { get; set; } // e.g. "VST3, AAX"
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedStr => LastUpdated.ToString("yyyy-MM-dd");
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager
{
    public class PluginItem
    {
        public string Name { get; set; }
        public string Formats { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedStr => LastUpdated.ToString("yyyy-MM-dd");

        // "Unique" Mode: Multiple paths separated by NewLine
        // "Details" Mode: Single path
        public string Location { get; set; }
    }
}

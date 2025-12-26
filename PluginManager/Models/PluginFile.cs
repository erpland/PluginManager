using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager
{
    public class PluginFile
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Format { get; set; }    // e.g. "VST3"
        public string FullPath { get; set; }
        public DateTime LastWriteTime { get; set; }
    }
}

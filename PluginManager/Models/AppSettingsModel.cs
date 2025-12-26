using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager
{
    public class AppSettingsModel
    {
        public List<string> SearchPaths { get; set; } = new List<string>();
        public List<string> Extensions { get; set; } = new List<string>();

        public static AppSettingsModel Default()
        {
            return new AppSettingsModel
            {
                SearchPaths = new List<string>
                {
                    @"C:\Program Files\Common Files\VST3",
                    @"C:\Program Files\Common Files\CLAP",
                    @"C:\Program Files\VstPlugins",
                    @"C:\Program Files\Steinberg\VstPlugins"
                },
                Extensions = new List<string> { ".vst3", ".dll", ".clap", ".aaxplugin" }
            };
        }
    }
}

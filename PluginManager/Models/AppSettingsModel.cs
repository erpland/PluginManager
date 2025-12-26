using System.Collections.Generic;

namespace PluginManager
{
    public class AppSettingsModel
    {
        public List<string> SearchPaths { get; set; } = new List<string>();
        public List<string> Extensions { get; set; } = new List<string>();

        // NEW: Exclusion Lists
        public List<string> ExcludedNames { get; set; } = new List<string>();
        public List<string> ExcludedPaths { get; set; } = new List<string>();

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
                Extensions = new List<string> { ".vst3", ".dll", ".clap", ".aaxplugin" },
                ExcludedNames = new List<string> { "Microsoft.Web.WebView2.Core" },
                ExcludedPaths = new List<string>()
            };
        }
    }
}
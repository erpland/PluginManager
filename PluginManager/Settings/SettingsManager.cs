using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace PluginManager
{
    public static class SettingsManager
    {
        private static readonly string FilePath = "settings.json";

        public static AppSettingsModel Load()
        {
            if (!File.Exists(FilePath)) return AppSettingsModel.Default();
            try
            {
                string json = File.ReadAllText(FilePath);
                var settings = JsonSerializer.Deserialize<AppSettingsModel>(json);

                // Safety check for null lists
                if (settings.ExcludedNames == null) settings.ExcludedNames = new List<string>();
                if (settings.ExcludedPaths == null) settings.ExcludedPaths = new List<string>();

                return settings;
            }
            catch { return AppSettingsModel.Default(); }
        }

        public static void Save(AppSettingsModel settings)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(FilePath, json);
        }
    }
}
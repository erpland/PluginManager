using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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
                return JsonSerializer.Deserialize<AppSettingsModel>(json);
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
using Microsoft.Win32;
using PluginManager.Exporters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PluginManager
{
    public partial class MainWindow : Window
    {
        private readonly Scanner _scanner = new Scanner();

        // 1. Raw Data (File based)
        private List<PluginFile> _rawFiles = new List<PluginFile>();

        // 2. View Data (Item based)
        private List<PluginItem> _viewData = new List<PluginItem>();
        private AppSettingsModel _settings;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            ToggleLocationColumn(false); // Default hidden
        }

        private async void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            BtnScan.IsEnabled = false;
            LblStatus.Text = "Scanning...";
            SaveCurrentConfig();

            // Run Scan
            _rawFiles = await Task.Run(() => _scanner.ScanPaths(_settings.SearchPaths, _settings.Extensions));

            LblStatus.Text = $"Found {_rawFiles.Count} files.";
            BtnScan.IsEnabled = true;

            // Process Display
            UpdateListDisplay();
        }

        private void UpdateListDisplay()
        {
            if (_rawFiles == null) return;

            string query = TxtSearch.Text.Trim().ToLower();
            bool showDetails = ChkDetails.IsChecked == true;

            // A. Filter
            var filtered = string.IsNullOrEmpty(query)
                ? _rawFiles
                : _rawFiles.Where(f => f.Name.ToLower().Contains(query)).ToList();

            _viewData.Clear();

            // B. Convert to PluginItem
            if (showDetails)
            {
                // DETAILED MODE: 1 File = 1 Row. Location is VISIBLE.
                foreach (var f in filtered.OrderBy(x => x.Name))
                {
                    _viewData.Add(new PluginItem
                    {
                        Name = f.Name,
                        Formats = f.Format,
                        LastUpdated = f.LastWriteTime,
                        Location = f.FullPath // Show path
                    });
                }
            }
            else
            {
                // UNIQUE MODE: Group by Name. Location is HIDDEN/EMPTY.
                var grouped = filtered.GroupBy(f => f.Name);

                foreach (var g in grouped.OrderBy(x => x.Key))
                {
                    _viewData.Add(new PluginItem
                    {
                        Name = g.Key,
                        Formats = string.Join(", ", g.Select(x => x.Format).Distinct().OrderBy(x => x)),
                        LastUpdated = g.Max(x => x.LastWriteTime),
                        Location = "" // Keep empty so Export is clean
                    });
                }
            }

            GridPlugins.ItemsSource = null;
            GridPlugins.ItemsSource = _viewData;

            ToggleLocationColumn(showDetails);
        }

        private void ToggleLocationColumn(bool isVisible)
        {
            if (GridPlugins.Columns.Count > 3)
                GridPlugins.Columns[3].Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        // --- Interaction Events ---

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateListDisplay();
        private void ChkDetails_Changed(object sender, RoutedEventArgs e) => UpdateListDisplay();

        private void Location_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && !string.IsNullOrEmpty(tb.Text))
            {
                try
                {
                    Process.Start("explorer.exe", $"/select,\"{tb.Text}\"");
                }
                catch { }
            }
        }

        // --- Config & Export ---

        private void LoadConfig()
        {
            _settings = SettingsManager.Load();
            TxtPaths.Text = string.Join(Environment.NewLine, _settings.SearchPaths);
            TxtExtensions.Text = string.Join(", ", _settings.Extensions);
        }

        private void SaveCurrentConfig()
        {
            _settings.SearchPaths = TxtPaths.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            _settings.Extensions = TxtExtensions.Text.Split(',').Select(e => e.Trim().ToLower()).Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
            SettingsManager.Save(_settings);
        }

        private void BtnExportMD_Click(object sender, RoutedEventArgs e) => RunExport(new MarkdownExporter());
        private void BtnExportCSV_Click(object sender, RoutedEventArgs e) => RunExport(new CsvExporter());
        private void BtnExportTXT_Click(object sender, RoutedEventArgs e) => RunExport(new TxtExporter());

        private void RunExport(IPluginExporter exporter)
        {
            if (_viewData.Count == 0) return;
            var dlg = new SaveFileDialog { Filter = exporter.FileFilter, FileName = "MyPlugins" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    exporter.Export(_viewData, dlg.FileName);
                    LblStatus.Text = "Export Saved!";
                    LblStatus.Foreground = Brushes.LightGreen;
                    Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
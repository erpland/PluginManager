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
        private List<PluginFile> _rawFiles = new List<PluginFile>();
        private List<PluginItem> _viewData = new List<PluginItem>();
        private AppSettingsModel _settings;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            ToggleLocationColumn(false);
        }

        private async void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            await ScanAndDisplay();
        }

        private async Task ScanAndDisplay()
        {
            BtnScan.IsEnabled = false;
            LblStatus.Text = "Scanning...";
            SaveCurrentConfig();

            _rawFiles = await Task.Run(() => _scanner.ScanPaths(_settings.SearchPaths, _settings.Extensions));

            UpdateTotalCount();
            BtnScan.IsEnabled = true;

            UpdateListDisplay();
        }

        private void UpdateListDisplay()
        {
            if (_rawFiles == null) return;
            string query = TxtSearch.Text.Trim().ToLower();
            bool showDetails = ChkDetails.IsChecked == true;

            var filtered = string.IsNullOrEmpty(query) ? _rawFiles : _rawFiles.Where(f => f.Name.ToLower().Contains(query));

            if (_settings.ExcludedNames != null)
                filtered = filtered.Where(f => !_settings.ExcludedNames.Contains(f.Name));

            if (_settings.ExcludedPaths != null)
                filtered = filtered.Where(f => !_settings.ExcludedPaths.Contains(f.FullPath));

            var resultList = filtered.ToList();
            _viewData.Clear();

            if (showDetails)
            {
                foreach (var f in resultList.OrderBy(x => x.Name))
                {
                    _viewData.Add(new PluginItem { Name = f.Name, Formats = f.Format, LastUpdated = f.LastWriteTime, Location = f.FullPath });
                }
            }
            else
            {
                var grouped = resultList.GroupBy(f => f.Name);
                foreach (var g in grouped.OrderBy(x => x.Key))
                {
                    _viewData.Add(new PluginItem
                    {
                        Name = g.Key,
                        Formats = string.Join(", ", g.Select(x => x.Format).Distinct().OrderBy(x => x)),
                        LastUpdated = g.Max(x => x.LastWriteTime),
                        Location = ""
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


        private void GridPlugins_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var contextMenu = new ContextMenu();

            // 1. Global Exclude (Always Enabled)
            var item1 = new MenuItem { Header = "Exclude Name (Global)" };
            item1.Click += CtxExcludeName_Click;
            contextMenu.Items.Add(item1);

            // 2. Specific Exclude (Disabled in Unique Mode)
            var item2 = new MenuItem { Header = "Exclude This Path (Specific)" };
            item2.Click += CtxExcludePath_Click;

            // IF we are NOT showing details, disable this option
            if (ChkDetails.IsChecked == false)
            {
                item2.IsEnabled = false;
                item2.Header += " (Requires Details View)"; // Optional: explains why
            }

            contextMenu.Items.Add(item2);

            e.Row.ContextMenu = contextMenu;
        }

        private void CtxExcludeName_Click(object sender, RoutedEventArgs e)
        {
            // Get the Item from the MenuItem's DataContext
            if (sender is MenuItem menuItem && menuItem.DataContext is PluginItem item)
            {
                if (!_settings.ExcludedNames.Contains(item.Name))
                {
                    _settings.ExcludedNames.Add(item.Name);
                    SettingsManager.Save(_settings);
                    UpdateTotalCount();
                    UpdateListDisplay();
                }
            }
        }

        private void CtxExcludePath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is PluginItem item)
            {
                if (!string.IsNullOrEmpty(item.Location))
                {
                    if (!_settings.ExcludedPaths.Contains(item.Location))
                    {
                        _settings.ExcludedPaths.Add(item.Location);
                        SettingsManager.Save(_settings);
                        UpdateListDisplay();
                        UpdateListDisplay();
                    }
                }
                else
                {
                    MessageBox.Show("Please switch to 'Show File Details' to exclude a specific path.", "Mode Warning");
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateListDisplay();
        private void ChkDetails_Changed(object sender, RoutedEventArgs e) => UpdateListDisplay();
        private void Location_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && !string.IsNullOrEmpty(tb.Text))
                try { Process.Start("explorer.exe", $"/select,\"{tb.Text}\""); } catch { }
        }

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
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void UpdateTotalCount()
        {
            if (_rawFiles == null) return;
            // Count only files that are NOT in the exclude lists
            int validCount = _rawFiles.Count(f =>
                !_settings.ExcludedNames.Contains(f.Name) &&
                !_settings.ExcludedPaths.Contains(f.FullPath));

            LblStatus.Text = $"Found {validCount} files.";
        }
    }
}
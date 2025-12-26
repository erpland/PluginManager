using Microsoft.Win32; // For SaveFileDialog
using PluginManager.Exporters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PluginManager
{
    public partial class MainWindow : Window
    {
        private readonly Scanner _scanner = new Scanner();
        private List<PluginItem> _currentData = new List<PluginItem>();

        public MainWindow()
        {
            InitializeComponent();
            LoadDefaultPaths();
        }

        private void LoadDefaultPaths()
        {
            var defaults = new List<string>
            {
                @"C:\Program Files\Common Files\VST3",
                @"C:\Program Files\Common Files\CLAP",
                @"C:\Program Files\Common Files\Avid\Audio\Plug-Ins", // AAX
                @"C:\Program Files\VstPlugins",
                @"C:\Program Files\Steinberg\VstPlugins",
                @"C:\Program Files\Native Instruments",
                @"C:\Program Files (x86)\VstPlugins"
            };

            TxtPaths.Text = string.Join(Environment.NewLine, defaults);
        }

        private async void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            await UpadateGUI();
        }

        private async Task UpadateGUI()
        {
            BtnScan.IsEnabled = false;
            LblStatus.Text = "Scanning...";

            // Get paths from TextBox (split by new line)
            var paths = TxtPaths.Text
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();

            // Run on background thread to keep UI responsive
            _currentData = await Task.Run(() => _scanner.ScanPaths(paths));

            // Update UI
            GridPlugins.ItemsSource = _currentData;
            LblStatus.Text = $"Found {_currentData.Count} plugins.";
            BtnScan.IsEnabled = true;
        }

        private void BtnExportMD_Click(object sender, RoutedEventArgs e)
        {
            RunExport(new MarkdownExporter());
        }

        private void BtnExportCSV_Click(object sender, RoutedEventArgs e)
        {
            RunExport(new CsvExporter());
        }

        private void BtnExportTXT_Click(object sender, RoutedEventArgs e)
        {
            RunExport(new TxtExporter());
        }


        private void RunExport(IPluginExporter exporter)
        {
            if (_currentData.Count == 0)
            {
                MessageBox.Show("Scan first!");
                return;
            }

            var dlg = new SaveFileDialog { Filter = exporter.FileFilter, FileName = "MyPlugins" };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    // 1. Attempt Export
                    exporter.Export(_currentData, dlg.FileName);

                    // 2. Show Success on GUI (Simple text update for now)
                    LblStatus.Text = "Export Saved!";
                    LblStatus.Foreground = System.Windows.Media.Brushes.LightGreen;

                    // 3. Try to open the file (Safe call)
                    TryOpenFile(dlg.FileName);
                }
                catch (Exception ex)
                {
                    // This only catches EXPORT errors (permissions, disk space), not open errors.
                    MessageBox.Show($"Error exporting: {ex.Message}");
                    LblStatus.Text = "Export Failed";
                    LblStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        }

        private void TryOpenFile(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch
            {
                // If opening fails (e.g. no app associated), we just ignore it.
                // The file is already saved safely, so we don't want to alarm the user.
            }
        }

    }
}
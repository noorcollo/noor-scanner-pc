using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using NAPS2.Scan;
using NoorScanner.Core;

namespace NoorScanner.App;

public partial class MainWindow : Window
{
    private readonly ScannerService _scannerService = new();
    private CancellationTokenSource? _scanCancellation;

    public MainWindow()
    {
        InitializeComponent();
        DpiSelector.SelectedIndex = 2;
        PaperSourceSelector.SelectedIndex = 0;
        Loaded += Window_Loaded;
        Closing += (_, _) => _scanCancellation?.Cancel();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await RefreshDevicesAsync();

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await RefreshDevicesAsync();

    private async Task RefreshDevicesAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            DeviceHint.Text = "Scanner discovery is available in the Windows desktop app.";
            StatusText.Text = "Windows is required to connect to local scanners.";
            return;
        }

        RefreshButton.IsEnabled = false;
        DeviceSelector.IsEnabled = false;
        StatusText.Text = "Looking for connected scanners…";
        try
        {
            var selectedId = (DeviceSelector.SelectedItem as ScanDevice)?.ID;
            var devices = await _scannerService.GetDevicesAsync();
            DeviceSelector.ItemsSource = devices;
            DeviceSelector.SelectedItem = devices.FirstOrDefault(d => d.ID == selectedId) ?? devices.FirstOrDefault();
            DeviceSelector.IsEnabled = devices.Count > 0;
            DeviceHint.Text = devices.Count == 0
                ? "No scanner found. Connect a WIA or TWAIN scanner, then refresh."
                : $"{devices.Count} scanner{(devices.Count == 1 ? "" : "s")} available.";
            StatusText.Text = devices.Count == 0 ? "No scanner detected" : "Ready · Choose a scanner and start a scan";
        }
        catch (Exception ex)
        {
            DeviceHint.Text = "Scanner discovery failed. Check the scanner connection and driver, then try again.";
            StatusText.Text = "Could not connect to scanner";
            MessageBox.Show(this, ex.Message, "Scanner discovery failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            RefreshButton.IsEnabled = true;
        }
    }

    private async void Scan_Click(object sender, RoutedEventArgs e)
    {
        if (DeviceSelector.SelectedItem is not ScanDevice device)
        {
            MessageBox.Show(this, "Connect a scanner and choose it from the scanner list first.",
                "No scanner selected", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Save scanned document",
            Filter = "PDF document (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = $"NoorScan_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };
        if (dialog.ShowDialog(this) != true) return;

        var selectedDpi = (DpiSelector.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        if (!int.TryParse(selectedDpi, out var dpi)) dpi = ScanSettings.DefaultDpi;
        var selectedSource = (PaperSourceSelector.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        var source = Enum.TryParse<PaperSource>(selectedSource, ignoreCase: true, out var parsedSource)
            ? parsedSource
            : PaperSource.Auto;
        var settings = new ScanSettings
        {
            Dpi = dpi,
            PaperSource = source,
            AutoDeskew = DeskewCheckBox.IsChecked == true
        };

        _scanCancellation = new CancellationTokenSource();
        SetScanBusy(true);
        StatusText.Text = "Scanning pages… Keep the document feeder clear until the scan completes.";
        try
        {
            var pageCount = await _scannerService.ScanToPdfAsync(device, settings, dialog.FileName, _scanCancellation.Token);
            StatusText.Text = $"Saved {pageCount} page{(pageCount == 1 ? "" : "s")} · {System.IO.Path.GetFileName(dialog.FileName)}";
            MessageBox.Show(this, $"Your {pageCount}-page PDF is ready:\n{dialog.FileName}",
                "Scan complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Scan cancelled";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Scan did not complete";
            MessageBox.Show(this, ex.Message, "Scan failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _scanCancellation.Dispose();
            _scanCancellation = null;
            SetScanBusy(false);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => _scanCancellation?.Cancel();

    private void SetScanBusy(bool isBusy)
    {
        ScanButton.IsEnabled = !isBusy;
        CancelButton.IsEnabled = isBusy;
        RefreshButton.IsEnabled = !isBusy;
        DeviceSelector.IsEnabled = !isBusy;
        DpiSelector.IsEnabled = !isBusy;
        PaperSourceSelector.IsEnabled = !isBusy;
        DeskewCheckBox.IsEnabled = !isBusy;
    }
}

using Microsoft.Win32;
using NAPS2.Scan;
using NoorScanner.Core;
using System.Windows;
using System.Windows.Controls;

namespace NoorScanner.App;

public partial class MainWindow : Window
{
    private readonly ScannerService _scannerService = new();
    private readonly DocumentWorkspaceService _documentWorkspace = new();
    private CancellationTokenSource? _scanCancellation;
    private CancellationTokenSource? _workspaceImportCancellation;
    private bool _workspaceOperationInProgress;
    private bool _windowClosing;

    public MainWindow()
    {
        InitializeComponent();
        DpiSelector.SelectedIndex = 2;
        PaperSourceSelector.SelectedIndex = 0;
        Loaded += Window_Loaded;
        Closing += (_, _) =>
        {
            _windowClosing = true;
            _scanCancellation?.Cancel();
            _workspaceImportCancellation?.Cancel();
            if (!_workspaceOperationInProgress)
            {
                _documentWorkspace.Dispose();
            }
        };
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

    private async void ImportFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Add photos or PDF documents",
            Filter = "Supported documents and images (*.pdf;*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff)|*.pdf;*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|PDF documents (*.pdf)|*.pdf|Image files (*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff)|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff",
            Multiselect = true,
            CheckFileExists = true,
            CheckPathExists = true
        };
        if (dialog.ShowDialog(this) != true) return;

        _workspaceOperationInProgress = true;
        _workspaceImportCancellation = new CancellationTokenSource();
        SetWorkspaceBusy(true);
        StatusText.Text = "Importing selected files locally…";
        try
        {
            var addedPages = await _documentWorkspace.ImportFilesAsync(
                dialog.FileNames,
                _workspaceImportCancellation.Token);
            if (_windowClosing) return;
            RefreshWorkspacePages();
            StatusText.Text = $"Added {addedPages} page{(addedPages == 1 ? "" : "s")} to the workspace";
        }
        catch (OperationCanceledException)
        {
            if (!_windowClosing)
            {
                RefreshWorkspacePages();
                StatusText.Text = "Import cancelled";
            }
        }
        catch (Exception ex)
        {
            if (!_windowClosing)
            {
                RefreshWorkspacePages();
                StatusText.Text = "One or more files could not be imported";
                MessageBox.Show(this, ex.Message, "Import failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        finally
        {
            _workspaceImportCancellation?.Dispose();
            _workspaceImportCancellation = null;
            _workspaceOperationInProgress = false;
            if (_windowClosing)
            {
                _documentWorkspace.Dispose();
            }
            else
            {
                SetWorkspaceBusy(false);
            }
        }
    }

    private async void ExportWorkspace_Click(object sender, RoutedEventArgs e)
    {
        if (_documentWorkspace.Pages.Count == 0)
        {
            MessageBox.Show(this, "Add at least one document or image before saving a PDF.",
                "Workspace is empty", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Save combined document",
            Filter = "PDF document (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = $"NoorDocument_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };
        if (dialog.ShowDialog(this) != true) return;

        _workspaceOperationInProgress = true;
        SetWorkspaceBusy(true);
        StatusText.Text = "Creating your combined PDF locally…";
        try
        {
            await _documentWorkspace.ExportToPdfAsync(dialog.FileName);
            if (_windowClosing) return;
            StatusText.Text = $"Saved {_documentWorkspace.Pages.Count} pages · {System.IO.Path.GetFileName(dialog.FileName)}";
            MessageBox.Show(this, $"Your {_documentWorkspace.Pages.Count}-page PDF is ready:\n{dialog.FileName}",
                "Export complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            if (!_windowClosing)
            {
                StatusText.Text = "PDF export did not complete";
                MessageBox.Show(this, ex.Message, "Export failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _workspaceOperationInProgress = false;
            if (_windowClosing)
            {
                _documentWorkspace.Dispose();
            }
            else
            {
                SetWorkspaceBusy(false);
            }
        }
    }

    private void RefreshWorkspacePages()
    {
        var pages = _documentWorkspace.Pages;
        WorkspacePagesList.ItemsSource = null;
        WorkspacePagesList.ItemsSource = pages;
        WorkspaceCountText.Text = $"{pages.Count} page{(pages.Count == 1 ? "" : "s")}";
        WorkspacePagesList.Visibility = pages.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        WorkspaceEmptyState.Visibility = pages.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ExportWorkspaceButton.IsEnabled = pages.Count > 0;
    }

    private void SetWorkspaceBusy(bool isBusy)
    {
        ImportFilesButton.IsEnabled = !isBusy;
        ExportWorkspaceButton.IsEnabled = !isBusy && _documentWorkspace.Pages.Count > 0;
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
            if (_windowClosing) return;
            StatusText.Text = $"Saved {pageCount} page{(pageCount == 1 ? "" : "s")} · {System.IO.Path.GetFileName(dialog.FileName)}";
            MessageBox.Show(this, $"Your {pageCount}-page PDF is ready:\n{dialog.FileName}",
                "Scan complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (OperationCanceledException)
        {
            if (!_windowClosing) StatusText.Text = "Scan cancelled";
        }
        catch (Exception ex)
        {
            if (!_windowClosing)
            {
                StatusText.Text = "Scan did not complete";
                MessageBox.Show(this, ex.Message, "Scan failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _scanCancellation.Dispose();
            _scanCancellation = null;
            if (!_windowClosing) SetScanBusy(false);
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

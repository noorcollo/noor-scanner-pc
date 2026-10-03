using Microsoft.Win32;
using NAPS2.Images;
using NAPS2.Scan;
using NoorScanner.Core;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NoorScanner.App;

public sealed record PageQueueItem(string PositionLabel, string SourceFileName, BitmapSource Thumbnail);

public partial class MainWindow : Window
{
    private readonly ScannerService _scannerService = new();
    private readonly DocumentWorkspaceService _documentWorkspace = new();
    private readonly Dictionary<ProcessedImage, BitmapSource> _thumbnailCache = new(ReferenceEqualityComparer.Instance);
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
                _thumbnailCache.Clear();
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
            await RefreshWorkspacePagesAsync(_documentWorkspace.Pages.Count - 1);
            StatusText.Text = $"Added {addedPages} page{(addedPages == 1 ? "" : "s")} to the workspace";
        }
        catch (OperationCanceledException)
        {
            if (!_windowClosing)
            {
                await RefreshWorkspacePagesAsync(_documentWorkspace.Pages.Count - 1);
                StatusText.Text = "Import cancelled";
            }
        }
        catch (Exception ex)
        {
            if (!_windowClosing)
            {
                await RefreshWorkspacePagesAsync(_documentWorkspace.Pages.Count - 1);
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
                _thumbnailCache.Clear();
            }
            else
            {
                SetWorkspaceBusy(false);
            }
        }
    }

    private async void MovePageUp_Click(object sender, RoutedEventArgs e) => await MoveSelectedPageAsync(-1);

    private async void MovePageDown_Click(object sender, RoutedEventArgs e) => await MoveSelectedPageAsync(1);

    private async Task MoveSelectedPageAsync(int offset)
    {
        var sourceIndex = WorkspacePagesList.SelectedIndex;
        var destinationIndex = sourceIndex + offset;
        if (sourceIndex < 0 || !_documentWorkspace.MovePage(sourceIndex, destinationIndex)) return;

        await RefreshWorkspacePagesAsync(destinationIndex);
        StatusText.Text = "Page order updated · the saved PDF will follow this list";
    }

    private async void RemovePage_Click(object sender, RoutedEventArgs e)
    {
        var index = WorkspacePagesList.SelectedIndex;
        if (index < 0) return;

        var removed = _documentWorkspace.RemovePageAt(index);
        if (removed is null) return;
        _thumbnailCache.Remove(removed.Image);
        var nextSelection = _documentWorkspace.Pages.Count == 0 ? -1 : Math.Min(index, _documentWorkspace.Pages.Count - 1);
        await RefreshWorkspacePagesAsync(nextSelection);
        StatusText.Text = $"Removed {removed.SourceFileName} from the workspace";
    }

    private void WorkspacePagesList_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePageQueueButtons();

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
                _thumbnailCache.Clear();
            }
            else
            {
                SetWorkspaceBusy(false);
            }
        }
    }

    private async Task RefreshWorkspacePagesAsync(int? selectedIndex = null)
    {
        var previousSelection = selectedIndex ?? WorkspacePagesList.SelectedIndex;
        var pages = _documentWorkspace.Pages.ToArray();
        var missingThumbnails = pages.Where(page => !_thumbnailCache.ContainsKey(page.Image)).ToArray();
        if (missingThumbnails.Length > 0)
        {
            var renderedThumbnails = await Task.Run(async () =>
            {
                var results = new List<(ProcessedImage Image, byte[] PngBytes)>(missingThumbnails.Length);
                foreach (var page in missingThumbnails)
                {
                    using var thumbnail = await new ThumbnailRenderer(page.Image.ImageContext)
                        .Render(page.Image, 160)
                        .ConfigureAwait(false);
                    using var stream = new MemoryStream();
                    thumbnail.Save(stream, ImageFileFormat.Png);
                    results.Add((page.Image, stream.ToArray()));
                }

                return results;
            });

            foreach (var rendered in renderedThumbnails)
            {
                _thumbnailCache[rendered.Image] = CreateBitmapSource(rendered.PngBytes);
            }
        }

        if (_windowClosing) return;
        var rows = pages.Select((page, index) => new PageQueueItem(
            $"Page {index + 1}", page.SourceFileName, _thumbnailCache[page.Image])).ToArray();
        WorkspacePagesList.ItemsSource = null;
        WorkspacePagesList.ItemsSource = rows;
        WorkspaceCountText.Text = $"{pages.Length} page{(pages.Length == 1 ? "" : "s")}";
        WorkspacePagesList.Visibility = pages.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        WorkspaceEmptyState.Visibility = pages.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        ExportWorkspaceButton.IsEnabled = pages.Length > 0;
        WorkspacePagesList.SelectedIndex = pages.Length == 0 || previousSelection < 0
            ? -1
            : Math.Min(previousSelection, pages.Length - 1);
        UpdatePageQueueButtons();
    }

    private static BitmapSource CreateBitmapSource(byte[] pngBytes)
    {
        using var stream = new MemoryStream(pngBytes, writable: false);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }

    private void UpdatePageQueueButtons()
    {
        var selectedIndex = WorkspacePagesList.SelectedIndex;
        var pageCount = _documentWorkspace.Pages.Count;
        MovePageUpButton.IsEnabled = !_workspaceOperationInProgress && selectedIndex > 0;
        MovePageDownButton.IsEnabled = !_workspaceOperationInProgress && selectedIndex >= 0 && selectedIndex < pageCount - 1;
        RemovePageButton.IsEnabled = !_workspaceOperationInProgress && selectedIndex >= 0;
    }

    private void SetWorkspaceBusy(bool isBusy)
    {
        ImportFilesButton.IsEnabled = !isBusy;
        ExportWorkspaceButton.IsEnabled = !isBusy && _documentWorkspace.Pages.Count > 0;
        UpdatePageQueueButtons();
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

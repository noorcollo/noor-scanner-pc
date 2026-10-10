using NAPS2.Images.Gdi;
using NAPS2.Pdf;
using NAPS2.Scan;
using System.Runtime.Versioning;

namespace NoorScanner.Core;

/// <summary>Local scanner operations. Scan images are never uploaded by this service.</summary>
[SupportedOSPlatform("windows7.0")]
public sealed class ScannerService
{
    public async Task<IReadOnlyList<ScanDevice>> GetDevicesAsync()
    {
        using var context = new ScanningContext(new GdiImageContext());
        var controller = new ScanController(context);
        return await controller.GetDeviceList().ConfigureAwait(false);
    }

    public async Task<int> ScanToPdfAsync(
        ScanDevice device,
        ScanSettings settings,
        string outputPath,
        CancellationToken cancellationToken = default,
        SearchablePdfOcrOptions? ocrOptions = null)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        settings.Validate();

        var fullPath = Path.GetFullPath(outputPath);
        if (!string.Equals(Path.GetExtension(fullPath), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The output file must use the .pdf extension.", nameof(outputPath));
        }

        var outputDirectory = Path.GetDirectoryName(fullPath)
            ?? throw new ArgumentException("The output path must include a directory.", nameof(outputPath));
        Directory.CreateDirectory(outputDirectory);

        using var context = new ScanningContext(new GdiImageContext());
        var controller = new ScanController(context);
        var pages = new List<NAPS2.Images.ProcessedImage>();

        await foreach (var page in controller.Scan(settings.ToScanOptions(device), cancellationToken)
                           .WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            pages.Add(page);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (pages.Count == 0)
        {
            throw new InvalidOperationException("The scanner returned no pages. Check the scanner and paper source, then try again.");
        }

        var temporaryPath = Path.Combine(
            outputDirectory,
            $".{Path.GetFileNameWithoutExtension(fullPath)}.{Guid.NewGuid():N}.tmp.pdf");
        try
        {
            var exported = await SearchablePdfExporter.ExportAsync(context, temporaryPath, pages, ocrOptions)
                .ConfigureAwait(false);
            if (!exported)
            {
                throw new IOException("The PDF exporter did not complete successfully.");
            }

            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }

        return pages.Count;
    }
}

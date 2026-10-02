using System.Runtime.Versioning;
using NAPS2.Images;
using NAPS2.Images.Gdi;
using NAPS2.ImportExport;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NoorScanner.Core;

/// <summary>A page imported from a local file into the current document workspace.</summary>
public sealed record WorkspacePage(ProcessedImage Image, string SourceFileName, int PageNumber)
{
    public string DisplayLabel => $"Page {PageNumber} · {SourceFileName}";
}

/// <summary>
/// Holds imported local document pages for the lifetime of the app window. Intermediate page data is
/// kept in a private, per-session temporary folder so large PDFs do not need to stay entirely in RAM.
/// </summary>
[SupportedOSPlatform("windows7.0")]
public sealed class DocumentWorkspaceService : IDisposable
{
    private readonly string _storageDirectory;
    private readonly ScanningContext _context;
    private readonly FileImporter _fileImporter;
    private readonly List<WorkspacePage> _pages = [];
    private bool _disposed;

    public DocumentWorkspaceService()
    {
        _storageDirectory = Path.Combine(Path.GetTempPath(), "NoorScannerPc", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_storageDirectory);
        _context = new ScanningContext(new GdiImageContext())
        {
            FileStorageManager = new FileStorageManager(_storageDirectory)
        };
        _fileImporter = new FileImporter(_context);
    }

    public IReadOnlyList<WorkspacePage> Pages => _pages.AsReadOnly();

    public async Task<int> ImportFilesAsync(IEnumerable<string> paths, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var validatedPaths = LocalImportPolicy.ValidateSelection(paths);
        var addedCount = 0;

        foreach (var path in validatedPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var importedImages = new List<ProcessedImage>();
            try
            {
                await foreach (var image in _fileImporter.Import(path)
                                   .WithCancellation(cancellationToken)
                                   .ConfigureAwait(false))
                {
                    importedImages.Add(image);
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (importedImages.Count == 0)
                {
                    throw new InvalidDataException(
                        $"No readable pages were found in '{Path.GetFileName(path)}'. The file may be empty, damaged, or password-protected.");
                }
            }
            catch
            {
                foreach (var image in importedImages)
                {
                    image.Dispose();
                }

                throw;
            }

            foreach (var image in importedImages)
            {
                _pages.Add(new WorkspacePage(image, Path.GetFileName(path), _pages.Count + 1));
                addedCount++;
            }
        }

        return addedCount;
    }

    public async Task ExportToPdfAsync(string outputPath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        if (_pages.Count == 0)
        {
            throw new InvalidOperationException("Import at least one page before exporting a PDF.");
        }

        var fullPath = Path.GetFullPath(outputPath);
        if (!string.Equals(Path.GetExtension(fullPath), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The output file must use the .pdf extension.", nameof(outputPath));
        }

        var outputDirectory = Path.GetDirectoryName(fullPath)
            ?? throw new ArgumentException("The output path must include a directory.", nameof(outputPath));
        Directory.CreateDirectory(outputDirectory);
        var temporaryPath = Path.Combine(
            outputDirectory,
            $".{Path.GetFileNameWithoutExtension(fullPath)}.{Guid.NewGuid():N}.tmp.pdf");

        try
        {
            var exported = await new PdfExporter(_context)
                .Export(temporaryPath, _pages.Select(page => page.Image).ToList())
                .ConfigureAwait(false);
            if (!exported)
            {
                throw new IOException("The PDF exporter did not complete successfully.");
            }

            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _context.Dispose();
        try
        {
            if (Directory.Exists(_storageDirectory))
            {
                Directory.Delete(_storageDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
            // Context disposal already removed its owned page data; leave cleanup to the OS if a
            // transient handle prevents removing this now-private temporary directory.
        }
        catch (UnauthorizedAccessException)
        {
            // Avoid making application shutdown fail if the OS temporarily blocks temp cleanup.
        }
    }
}

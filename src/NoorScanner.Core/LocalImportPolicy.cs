namespace NoorScanner.Core;

/// <summary>Validates user-selected local files before passing them to the image/PDF importer.</summary>
public static class LocalImportPolicy
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff"
    };

    public static IReadOnlyList<string> ValidateSelection(IEnumerable<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var validated = new List<string>();
        foreach (var path in paths)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            var fullPath = Path.GetFullPath(path);
            var extension = Path.GetExtension(fullPath);
            if (!SupportedExtensions.Contains(extension))
            {
                throw new NotSupportedException(
                    $"'{extension}' is not supported. Choose a PDF, JPEG, PNG, BMP, or TIFF file.");
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The selected document could not be found.", fullPath);
            }

            validated.Add(fullPath);
        }

        if (validated.Count == 0)
        {
            throw new ArgumentException("Choose at least one document or image to import.", nameof(paths));
        }

        return validated;
    }
}

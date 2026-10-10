using NAPS2.Ocr;

namespace NoorScanner.Core;

public sealed record OcrLanguageChoice(string DisplayName, string Code);

/// <summary>Validates local OCR dependencies and holds one searchable-PDF export configuration.</summary>
public sealed class SearchablePdfOcrOptions
{
    private static readonly Func<string, TesseractOcrEngine> DefaultEngineFactory = TesseractOcrEngine.Bundled;

    public static IReadOnlyList<OcrLanguageChoice> SupportedLanguages { get; } = Array.AsReadOnly(
    [
        new OcrLanguageChoice("English", "eng"),
        new OcrLanguageChoice("French", "fra"),
        new OcrLanguageChoice("Arabic", "ara")
    ]);

    private SearchablePdfOcrOptions(string languageCode, string languageDataDirectory, TesseractOcrEngine engine)
    {
        LanguageCode = languageCode;
        LanguageDataDirectory = languageDataDirectory;
        Engine = engine;
        Parameters = new OcrParams(languageCode, OcrMode.Fast, 120);
    }

    public string LanguageCode { get; }
    public string LanguageDataDirectory { get; }
    public TesseractOcrEngine Engine { get; }
    public OcrParams Parameters { get; }

    public static string DefaultLanguageDataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NoorScannerPC",
        "tessdata");

    public static string GetLanguageDataFilePath(string languageCode, string languageDataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageDataDirectory);
        if (!SupportedLanguages.Any(language => language.Code == languageCode))
        {
            throw new ArgumentOutOfRangeException(nameof(languageCode), languageCode,
                "Choose one of the supported OCR languages: eng, fra, or ara.");
        }

        return Path.Combine(Path.GetFullPath(languageDataDirectory), $"{languageCode}.traineddata");
    }

    public static SearchablePdfOcrOptions Create(
        string languageCode,
        string? languageDataDirectory = null,
        Func<string, TesseractOcrEngine>? engineFactory = null)
    {
        var directory = Path.GetFullPath(languageDataDirectory ?? DefaultLanguageDataDirectory);
        var modelPath = GetLanguageDataFilePath(languageCode, directory);
        if (!File.Exists(modelPath) || new FileInfo(modelPath).Length == 0)
        {
            throw new InvalidOperationException(
                $"Searchable PDF OCR needs '{Path.GetFileName(modelPath)}'. It was not found or is empty.\n" +
                $"Expected file: {modelPath}\n" +
                "Download the matching model from https://github.com/tesseract-ocr/tessdata_fast and place it at that path. " +
                "No PDF was created.");
        }

        try
        {
            var engine = (engineFactory ?? DefaultEngineFactory)(directory);
            return new SearchablePdfOcrOptions(languageCode, directory, engine);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException)
        {
            throw new InvalidOperationException(
                "The bundled Tesseract OCR engine is unavailable. Reinstall Noor Scanner PC or turn off searchable-PDF OCR. " +
                "No PDF was created.", exception);
        }
    }
}

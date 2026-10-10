using NAPS2.Ocr;
using NoorScanner.Core;
using Xunit;

namespace NoorScanner.Core.Tests;

public sealed class SearchablePdfOcrOptionsTests
{
    [Fact]
    public void SupportedLanguages_AreEnglishFrenchAndArabic()
    {
        Assert.Equal(
            new[] { "eng", "fra", "ara" },
            SearchablePdfOcrOptions.SupportedLanguages.Select(language => language.Code));
    }

    [Theory]
    [InlineData("eng")]
    [InlineData("fra")]
    [InlineData("ara")]
    public void GetLanguageDataFilePath_UsesTesseractCode(string code)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"ocr-models-{Guid.NewGuid():N}");

        var path = SearchablePdfOcrOptions.GetLanguageDataFilePath(code, directory);

        Assert.Equal(Path.Combine(directory, $"{code}.traineddata"), path);
    }

    [Fact]
    public void Create_ReportsMissingLanguageDataAndExpectedPath()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"ocr-models-{Guid.NewGuid():N}");

        var exception = Assert.Throws<InvalidOperationException>(
            () => SearchablePdfOcrOptions.Create("eng", directory));

        Assert.Contains("eng.traineddata", exception.Message);
        Assert.Contains(SearchablePdfOcrOptions.GetLanguageDataFilePath("eng", directory), exception.Message);
        Assert.Contains("tessdata_fast", exception.Message);
        Assert.Contains("No PDF was created", exception.Message);
    }

    [Fact]
    public void Create_RejectsUnsupportedLanguageBeforeEngineCreation()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SearchablePdfOcrOptions.Create("xx", Path.GetTempPath()));

        Assert.Contains("eng, fra, or ara", exception.Message);
    }

    [Fact]
    public void Create_ReportsUnavailableEngineClearly()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"ocr-models-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "eng.traineddata"), "test model placeholder");

        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                SearchablePdfOcrOptions.Create("eng", directory,
                    _ => throw new FileNotFoundException("Tesseract runtime missing.")));

            Assert.Contains("OCR engine is unavailable", exception.Message);
            Assert.Contains("No PDF was created", exception.Message);
            Assert.IsType<FileNotFoundException>(exception.InnerException);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}

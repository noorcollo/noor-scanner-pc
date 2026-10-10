using NoorScanner.Core;
using Xunit;

namespace NoorScanner.Core.Tests;

public sealed class LocalImportPolicyTests
{
    [Theory]
    [InlineData(".pdf")]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".png")]
    [InlineData(".bmp")]
    [InlineData(".tif")]
    [InlineData(".tiff")]
    public void ValidateSelection_AcceptsExistingSupportedFiles(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"noor-import-{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(path, [0x01]);
        try
        {
            var result = LocalImportPolicy.ValidateSelection([path]);
            Assert.Equal(Path.GetFullPath(path), Assert.Single(result));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ValidateSelection_RejectsUnsupportedTypes()
    {
        var path = Path.Combine(Path.GetTempPath(), $"noor-import-{Guid.NewGuid():N}.docx");
        File.WriteAllBytes(path, [0x01]);
        try
        {
            Assert.Throws<NotSupportedException>(() => LocalImportPolicy.ValidateSelection([path]));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ValidateSelection_RejectsMissingFiles()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.pdf");
        Assert.Throws<FileNotFoundException>(() => LocalImportPolicy.ValidateSelection([path]));
    }

    [Fact]
    public void ValidateSelection_RejectsEmptySelection()
    {
        Assert.Throws<ArgumentException>(() => LocalImportPolicy.ValidateSelection([]));
    }

    [Fact]
    public void ValidateSelection_RejectsBlankPath()
    {
        Assert.Throws<ArgumentException>(() => LocalImportPolicy.ValidateSelection([" "]));
    }
}

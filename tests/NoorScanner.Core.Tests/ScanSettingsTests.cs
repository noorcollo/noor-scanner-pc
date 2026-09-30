using NAPS2.Scan;
using NoorScanner.Core;
using Xunit;

namespace NoorScanner.Core.Tests;

public sealed class ScanSettingsTests
{
    [Theory]
    [InlineData(75)]
    [InlineData(150)]
    [InlineData(300)]
    [InlineData(600)]
    [InlineData(1200)]
    public void Validate_AcceptsSupportedResolutionRange(int dpi)
    {
        var settings = new ScanSettings { Dpi = dpi };
        settings.Validate();
    }

    [Theory]
    [InlineData(74)]
    [InlineData(1201)]
    public void Validate_RejectsOutOfRangeResolution(int dpi)
    {
        var settings = new ScanSettings { Dpi = dpi };
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
    }

    [Theory]
    [InlineData(PaperSource.Auto)]
    [InlineData(PaperSource.Flatbed)]
    [InlineData(PaperSource.Feeder)]
    [InlineData(PaperSource.Duplex)]
    public void ToScanOptions_MapsTheSelectedPaperSource(PaperSource source)
    {
        var device = new ScanDevice(Driver.Wia, "test-device", "Test scanner");
        var options = new ScanSettings { PaperSource = source }.ToScanOptions(device);
        Assert.Equal(source, options.PaperSource);
        Assert.Equal(ScanSettings.DefaultDpi, options.Dpi);
        Assert.True(options.AutoDeskew);
        Assert.True(options.MaxQuality);
        Assert.Equal(device, options.Device);
    }

    [Fact]
    public void Validate_RejectsUndefinedPaperSource()
    {
        var settings = new ScanSettings { PaperSource = (PaperSource)999 };
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
    }
}

using NAPS2.Scan;

namespace NoorScanner.Core;

/// <summary>Validated settings used for one scan job.</summary>
public sealed record ScanSettings
{
    public const int DefaultDpi = 300;
    public const int MinimumDpi = 75;
    public const int MaximumDpi = 1200;

    public int Dpi { get; init; } = DefaultDpi;
    public PaperSource PaperSource { get; init; } = PaperSource.Auto;
    public bool AutoDeskew { get; init; } = true;

    public void Validate()
    {
        if (Dpi is < MinimumDpi or > MaximumDpi)
        {
            throw new ArgumentOutOfRangeException(nameof(Dpi), Dpi,
                $"Resolution must be between {MinimumDpi} and {MaximumDpi} DPI.");
        }

        if (!Enum.IsDefined(PaperSource))
        {
            throw new ArgumentOutOfRangeException(nameof(PaperSource), PaperSource,
                "Choose a supported scanner paper source.");
        }
    }

    public ScanOptions ToScanOptions(ScanDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);
        Validate();
        return new ScanOptions
        {
            Device = device,
            Dpi = Dpi,
            PaperSource = PaperSource,
            AutoDeskew = AutoDeskew,
            MaxQuality = true
        };
    }
}

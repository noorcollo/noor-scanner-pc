using NAPS2.Images;
using NAPS2.Images.Transforms;
using NoorScanner.Core;
using Xunit;

namespace NoorScanner.Core.Tests;

public sealed class PageCropOperationsTests
{
    [Fact]
    public void AddMargin_UsesReferenceThumbnailDimensions_AndPreservesOtherEdits()
    {
        var original = TransformState.Empty
            .AddOrSimplify(new RotationTransform(90))
            .AddOrSimplify(new CorrectionTransform(CorrectionMode.Document));

        var cropped = PageCropOperations.AddMargin(original, referenceWidth: 160, referenceHeight: 128, marginPercent: 5);
        var crop = Assert.IsType<CropTransform>(cropped.Transforms[^1]);

        Assert.Equal(8, crop.Left);
        Assert.Equal(8, crop.Right);
        Assert.Equal(6, crop.Top);
        Assert.Equal(6, crop.Bottom);
        Assert.Equal(160, crop.OriginalWidth);
        Assert.Equal(128, crop.OriginalHeight);
        Assert.Contains(cropped.Transforms, transform => transform is RotationTransform { Angle: 90 });
        Assert.Contains(cropped.Transforms,
            transform => transform is CorrectionTransform { Mode: CorrectionMode.Document });
        Assert.True(PageCropOperations.HasCrop(cropped));
    }

    [Fact]
    public void AddMargin_AgainAfterCrop_ComposesIntoOneCropTransform()
    {
        var firstCrop = PageCropOperations.AddMargin(TransformState.Empty, 100, 100, 5);

        var twiceCropped = PageCropOperations.AddMargin(firstCrop, 90, 90, 5);
        var crop = Assert.IsType<CropTransform>(Assert.Single(twiceCropped.Transforms));

        Assert.Equal(10, crop.Left);
        Assert.Equal(10, crop.Right);
        Assert.Equal(10, crop.Top);
        Assert.Equal(10, crop.Bottom);
        Assert.Equal(100, crop.OriginalWidth);
        Assert.Equal(100, crop.OriginalHeight);
    }

    [Fact]
    public void Reset_RemovesOnlyCropTransforms()
    {
        var original = TransformState.Empty
            .AddOrSimplify(new RotationTransform(90))
            .AddOrSimplify(new CorrectionTransform(CorrectionMode.Document));
        var cropped = PageCropOperations.AddMargin(original, 160, 128, 5);

        var reset = PageCropOperations.Reset(cropped);

        Assert.False(PageCropOperations.HasCrop(reset));
        Assert.Contains(reset.Transforms, transform => transform is RotationTransform { Angle: 90 });
        Assert.Contains(reset.Transforms,
            transform => transform is CorrectionTransform { Mode: CorrectionMode.Document });
        Assert.DoesNotContain(reset.Transforms, transform => transform is CropTransform);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(11)]
    public void AddMargin_RejectsUnsupportedPercentages(int percent)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PageCropOperations.AddMargin(TransformState.Empty, 100, 100, percent));
    }

    [Fact]
    public void AddMargin_RejectsDimensionsTooSmallToCropSafely()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PageCropOperations.AddMargin(TransformState.Empty, 2, 100, 2));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PageCropOperations.AddMargin(TransformState.Empty, 100, 2, 2));
    }
}

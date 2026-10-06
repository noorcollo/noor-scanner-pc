using System.Collections.Immutable;
using NAPS2.Images;
using NAPS2.Images.Transforms;

namespace NoorScanner.Core;

/// <summary>Builds proportional crop transforms while keeping other page edits intact.</summary>
public static class PageCropOperations
{
    public static bool HasCrop(TransformState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.Transforms.OfType<CropTransform>().Any(transform => !transform.IsNull);
    }

    public static TransformState AddMargin(TransformState state, int referenceWidth, int referenceHeight, int marginPercent)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (referenceWidth < 3) throw new ArgumentOutOfRangeException(nameof(referenceWidth));
        if (referenceHeight < 3) throw new ArgumentOutOfRangeException(nameof(referenceHeight));
        if (marginPercent is not (2 or 5 or 10)) throw new ArgumentOutOfRangeException(nameof(marginPercent));

        var horizontalMargin = Math.Max(1,
            (int)Math.Round(referenceWidth * marginPercent / 100d, MidpointRounding.AwayFromZero));
        var verticalMargin = Math.Max(1,
            (int)Math.Round(referenceHeight * marginPercent / 100d, MidpointRounding.AwayFromZero));
        if (horizontalMargin * 2 >= referenceWidth)
        {
            throw new ArgumentOutOfRangeException(nameof(referenceWidth), "The selected crop would leave no page width.");
        }
        if (verticalMargin * 2 >= referenceHeight)
        {
            throw new ArgumentOutOfRangeException(nameof(referenceHeight), "The selected crop would leave no page height.");
        }

        var crop = new CropTransform(
            horizontalMargin,
            horizontalMargin,
            verticalMargin,
            verticalMargin,
            referenceWidth,
            referenceHeight);
        return state.AddOrSimplify(crop);
    }

    public static TransformState Reset(TransformState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var remainingTransforms = state.Transforms
            .Where(transform => transform is not CropTransform)
            .ToImmutableList();
        return remainingTransforms.Count == 0
            ? TransformState.Empty
            : new TransformState(remainingTransforms);
    }
}

using NAPS2.Images;
using NAPS2.Images.Transforms;
using NoorScanner.Core;
using Xunit;

namespace NoorScanner.Core.Tests;

public sealed class DocumentCorrectionOperationsTests
{
    [Fact]
    public void EnableDocumentCorrection_IsIdempotent_AndPreservesRotation()
    {
        var original = TransformState.Empty.AddOrSimplify(new RotationTransform(90));

        var enhanced = DocumentCorrectionOperations.SetDocumentCorrection(original, enabled: true);
        var enabledAgain = DocumentCorrectionOperations.SetDocumentCorrection(enhanced, enabled: true);

        Assert.True(DocumentCorrectionOperations.IsDocumentCorrectionEnabled(enhanced));
        Assert.Single(enhanced.Transforms.OfType<CorrectionTransform>());
        Assert.Contains(enhanced.Transforms, transform => transform is RotationTransform { Angle: 90 });
        Assert.Equal(enhanced, enabledAgain);
    }

    [Fact]
    public void ResetDocumentCorrection_RemovesOnlyDocumentPreset()
    {
        var original = TransformState.Empty
            .AddOrSimplify(new RotationTransform(90))
            .AddOrSimplify(new CorrectionTransform(CorrectionMode.Document))
            .AddOrSimplify(new CorrectionTransform(CorrectionMode.Photo));

        var reset = DocumentCorrectionOperations.SetDocumentCorrection(original, enabled: false);

        Assert.False(DocumentCorrectionOperations.IsDocumentCorrectionEnabled(reset));
        Assert.Contains(reset.Transforms, transform => transform is RotationTransform { Angle: 90 });
        Assert.Contains(reset.Transforms,
            transform => transform is CorrectionTransform { Mode: CorrectionMode.Photo });
        Assert.DoesNotContain(reset.Transforms,
            transform => transform is CorrectionTransform { Mode: CorrectionMode.Document });
    }

    [Fact]
    public void DisableDocumentCorrection_OnEmptyState_LeavesItEmpty()
    {
        var reset = DocumentCorrectionOperations.SetDocumentCorrection(TransformState.Empty, enabled: false);

        Assert.True(reset.IsEmpty);
    }
}

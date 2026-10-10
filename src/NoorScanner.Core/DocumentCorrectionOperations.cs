using System.Collections.Immutable;
using NAPS2.Images;
using NAPS2.Images.Transforms;

namespace NoorScanner.Core;

/// <summary>Applies or removes the document-enhancement transform without disturbing other edits.</summary>
public static class DocumentCorrectionOperations
{
    public static bool IsDocumentCorrectionEnabled(TransformState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.Transforms.OfType<CorrectionTransform>()
            .Any(transform => transform.Mode == CorrectionMode.Document);
    }

    public static TransformState SetDocumentCorrection(TransformState state, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(state);

        var transforms = state.Transforms
            .Where(transform => transform is not CorrectionTransform correction ||
                                correction.Mode != CorrectionMode.Document)
            .ToImmutableList();
        var nextState = transforms.Count == 0 ? TransformState.Empty : new TransformState(transforms);
        return enabled
            ? nextState.AddOrSimplify(new CorrectionTransform(CorrectionMode.Document))
            : nextState;
    }
}

using NoorScanner.Core;
using NAPS2.Images.Transforms;
using System.Runtime.Versioning;
using Xunit;

namespace NoorScanner.Core.Tests;

public sealed class DocumentWorkspaceIntegrationTests
{
    [Fact]
    [SupportedOSPlatform("windows7.0")]
    public async Task Workspace_ImportsImage_EnhancesAndResetsCorrection_ExportsPdf()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(7)) return;

        var folder = Path.Combine(Path.GetTempPath(), $"noor-workspace-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        var imagePath = Path.Combine(folder, "one-page.bmp");
        var enhancedPdfPath = Path.Combine(folder, "enhanced.pdf");
        var pdfPath = Path.Combine(folder, "roundtrip.pdf");
        File.WriteAllBytes(imagePath, CreateTestBmp());

        try
        {
            using (var imageWorkspace = new DocumentWorkspaceService())
            {
                var imported = await imageWorkspace.ImportFilesAsync([imagePath]);
                Assert.Equal(1, imported);
                var importedPage = Assert.Single(imageWorkspace.Pages);
                Assert.Equal("one-page.bmp", importedPage.SourceFileName);
                Assert.True(importedPage.Image.TransformState.IsEmpty);
                Assert.False(imageWorkspace.RotatePage(-1, clockwise: true));

                Assert.True(imageWorkspace.RotatePage(0, clockwise: true));
                var clockwiseTransform = Assert.IsType<RotationTransform>(
                    Assert.Single(imageWorkspace.Pages[0].Image.TransformState.Transforms));
                Assert.Equal(90, clockwiseTransform.Angle);
                Assert.True(imageWorkspace.RotatePage(0, clockwise: false));
                Assert.True(imageWorkspace.Pages[0].Image.TransformState.IsEmpty);
                Assert.True(imageWorkspace.RotatePage(0, clockwise: true));

                Assert.True(imageWorkspace.SetDocumentCorrection(0, enabled: true));
                Assert.True(imageWorkspace.IsDocumentCorrectionEnabled(0));
                Assert.Contains(imageWorkspace.Pages[0].Image.TransformState.Transforms,
                    transform => transform is CorrectionTransform { Mode: CorrectionMode.Document });
                await imageWorkspace.ExportToPdfAsync(enhancedPdfPath);
                Assert.True(new FileInfo(enhancedPdfPath).Length > 100);

                Assert.True(imageWorkspace.SetDocumentCorrection(0, enabled: false));
                Assert.False(imageWorkspace.IsDocumentCorrectionEnabled(0));
                Assert.Contains(imageWorkspace.Pages[0].Image.TransformState.Transforms,
                    transform => transform is RotationTransform { Angle: 90 });

                await imageWorkspace.ExportToPdfAsync(pdfPath);
                Assert.True(new FileInfo(pdfPath).Length > 100);
            }

            using var pdfWorkspace = new DocumentWorkspaceService();
            var importedPdfPages = await pdfWorkspace.ImportFilesAsync([pdfPath]);
            Assert.Equal(1, importedPdfPages);
            Assert.Equal("roundtrip.pdf", Assert.Single(pdfWorkspace.Pages).SourceFileName);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    private static byte[] CreateTestBmp()
    {
        const int width = 64;
        const int height = 64;
        const int rowSize = width * 3;
        const int pixelSize = rowSize * height;
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            writer.Write((byte)'B');
            writer.Write((byte)'M');
            writer.Write((uint)(54 + pixelSize));
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((uint)54);
            writer.Write((uint)40);
            writer.Write(width);
            writer.Write(height);
            writer.Write((ushort)1);
            writer.Write((ushort)24);
            writer.Write((uint)0);
            writer.Write((uint)pixelSize);
            writer.Write(2835);
            writer.Write(2835);
            writer.Write((uint)0);
            writer.Write((uint)0);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = x % 16 < 2 || y % 16 < 2 ? (byte)0 : (byte)255;
                    writer.Write(value);
                    writer.Write(value);
                    writer.Write(value);
                }
            }
        }

        return stream.ToArray();
    }
}

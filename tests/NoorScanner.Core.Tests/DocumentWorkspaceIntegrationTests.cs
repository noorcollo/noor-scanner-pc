using NoorScanner.Core;
using System.Runtime.Versioning;
using Xunit;

namespace NoorScanner.Core.Tests;

public sealed class DocumentWorkspaceIntegrationTests
{
    [Fact]
    [SupportedOSPlatform("windows7.0")]
    public async Task Workspace_ImportsImage_ExportsPdf_AndImportsPdf()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(7)) return;

        var folder = Path.Combine(Path.GetTempPath(), $"noor-workspace-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        var imagePath = Path.Combine(folder, "one-page.bmp");
        var pdfPath = Path.Combine(folder, "roundtrip.pdf");
        File.WriteAllBytes(imagePath, CreateOnePixelBmp());

        try
        {
            using (var imageWorkspace = new DocumentWorkspaceService())
            {
                var imported = await imageWorkspace.ImportFilesAsync([imagePath]);
                Assert.Equal(1, imported);
                var importedPage = Assert.Single(imageWorkspace.Pages);
                Assert.Equal("one-page.bmp", importedPage.SourceFileName);

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

    private static byte[] CreateOnePixelBmp()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            writer.Write((byte)'B');
            writer.Write((byte)'M');
            writer.Write((uint)58);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((uint)54);
            writer.Write((uint)40);
            writer.Write(1);
            writer.Write(1);
            writer.Write((ushort)1);
            writer.Write((ushort)24);
            writer.Write((uint)0);
            writer.Write((uint)4);
            writer.Write(2835);
            writer.Write(2835);
            writer.Write((uint)0);
            writer.Write((uint)0);
            writer.Write((byte)255);
            writer.Write((byte)255);
            writer.Write((byte)255);
            writer.Write((byte)0);
        }

        return stream.ToArray();
    }
}

using NAPS2.Images;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Scan;
using System.Threading;

namespace NoorScanner.Core;

public static class SearchablePdfExporter
{
    public static async Task<bool> ExportAsync(
        ScanningContext context,
        string outputPath,
        ICollection<ProcessedImage> pages,
        SearchablePdfOcrOptions? ocrOptions = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(pages);

        var exporter = new PdfExporter(context);
        if (ocrOptions is null)
        {
            return await exporter.Export(outputPath, pages).ConfigureAwait(false);
        }

        var guardedEngine = new RequiredOcrEngine(ocrOptions.Engine);
        context.OcrEngine = guardedEngine;
        bool exported;
        try
        {
            exported = await exporter.Export(
                outputPath,
                pages,
                ocrParams: ocrOptions.Parameters).ConfigureAwait(false);
        }
        catch (Exception exception) when (guardedEngine.Failed)
        {
            throw CreateOcrFailure(exception);
        }

        if (!exported) return false;
        if (guardedEngine.Failed) throw CreateOcrFailure();
        return true;
    }

    private static InvalidOperationException CreateOcrFailure(Exception? innerException = null) =>
        new("OCR failed or timed out on one or more pages. Check that the selected language model is valid and try again. No PDF was saved.",
            innerException);

    private sealed class RequiredOcrEngine(TesseractOcrEngine innerEngine) : IOcrEngine
    {
        private int _failed;

        public bool Failed => Volatile.Read(ref _failed) != 0;

        public async Task<OcrResult?> ProcessImage(
            ScanningContext scanningContext,
            string imagePath,
            OcrParams ocrParams,
            CancellationToken cancelToken)
        {
            try
            {
                var result = await innerEngine.ProcessImage(scanningContext, imagePath, ocrParams, cancelToken)
                    .ConfigureAwait(false);
                if (result is null) Interlocked.Exchange(ref _failed, 1);
                return result;
            }
            catch
            {
                Interlocked.Exchange(ref _failed, 1);
                throw;
            }
        }
    }
}

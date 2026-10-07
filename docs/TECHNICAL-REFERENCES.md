# Technical references

Research checked on 2026-10-02 for the local document import milestone:

- [NAPS2 SDK FileImporter API](https://www.naps2.com/sdk/doc/api/NAPS2.ImportExport.FileImporter.html) — `FileImporter(ScanningContext)` and `Import(string)` expose an asynchronous PDF/image page importer.
- [NAPS2 SDK FileStorageManager API](https://www.naps2.com/sdk/doc/api/NAPS2.Images.FileStorageManager.html) — `FileStorageManager(string folderPath)` provides a per-context on-disk page storage location.
- [NAPS2 PDF import sample](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/PdfImportSample.cs) — documents that PDF import requires the optional `NAPS2.Pdfium.Binaries` NuGet package and shows `PdfImporter` with `ScanningContext`.
- [NAPS2 file storage sample](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/FileStorageSample.cs) — demonstrates setting `ScanningContext.FileStorageManager` to reduce memory use for page sets.
- [NAPS2.Pdfium.Binaries 1.3.0 on NuGet](https://www.nuget.org/packages/NAPS2.Pdfium.Binaries/1.3.0) — lists the package purpose and supported runtime platforms. Its NuGet package metadata declares Apache-2.0 and links to [cyanfish/naps2-pdfium](https://github.com/cyanfish/naps2-pdfium).

This page records source material, not a guarantee that every possible user file or scanner is supported. File compatibility and physical scanner behavior still require Windows/device testing.


Research checked on 2026-10-03 for the page-queue preview milestone:

- [NAPS2 SDK ProcessedImage API](https://www.naps2.com/sdk/doc/api/NAPS2.Images.ProcessedImage.html) — `ProcessedImage` is immutable and reference-counted; clones must be disposed before the parent context is disposed.
- [NAPS2 SDK ThumbnailRenderer API](https://www.naps2.com/sdk/doc/api/NAPS2.Images.ThumbnailRenderer.html) and [upstream implementation](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk/Images/ThumbnailRenderer.cs) — `Render(ProcessedImage, int)` asynchronously returns an `IMemoryImage`, suitable for a small preview instead of rendering the full-resolution source.
- [NAPS2 WPF image API](https://www.naps2.com/sdk/doc/api/NAPS2.Images.Wpf.WpfImage.html) — image memory objects can save to a stream with an explicit `ImageFileFormat`, allowing a frozen WPF bitmap to be created from a small PNG thumbnail.


Research checked on 2026-10-04 for non-destructive page rotation:

- [NAPS2 SDK RotationTransform API](https://www.naps2.com/sdk/doc/api/NAPS2.Images.Transforms.RotationTransform.html) — accepts an angle in degrees and normalizes rotations; [ProcessedImage.WithTransform](https://www.naps2.com/sdk/doc/api/NAPS2.Images.ProcessedImage.html) returns a new immutable image reference with the transform in its state.
- [NAPS2 Windows GDI transformer source](https://github.com/cyanfish/naps2/blob/master/NAPS2.Images.Gdi/GdiImageTransformer.cs) routes a 90-degree rotation to `RotateFlipType.Rotate90FlipNone`.
- [Microsoft RotateFlipType documentation](https://learn.microsoft.com/en-us/dotnet/api/system.drawing.rotatefliptype?view=net-11.0-pp) defines `Rotate90FlipNone` as a 90-degree clockwise rotation without flipping; the UI can safely label the corresponding actions Rotate right and Rotate left.
- [NAPS2 TransformState API](https://www.naps2.com/sdk/doc/api/NAPS2.Images.TransformState.html) exposes its transform sequence and `IsEmpty`; a Windows integration test can assert the rotation state and that opposite quarter-turns simplify back to the original orientation.


Research checked on 2026-10-05 for the optional document-enhancement preset:

- [NAPS2 CorrectionTransform API](https://www.naps2.com/sdk/doc/api/NAPS2.Images.Transforms.CorrectionTransform.html) accepts `CorrectionMode`; [CorrectionMode](https://www.naps2.com/sdk/doc/api/NAPS2.Images.Transforms.CorrectionMode.html) includes `Document`, `Photo`, and `None`.
- [NAPS2 correction-transform source](https://github.com/cyanfish/naps2/blob/master/NAPS2.Images/Transforms/CorrectionTransform.cs) treats repeated same-mode correction as idempotent, which supports a simple apply/reset UI.
- [NAPS2 transform application source](https://github.com/cyanfish/naps2/blob/master/NAPS2.Images/Transforms/AbstractImageTransformer.cs) shows Document mode applying black/white-point correction and a bilateral filter; [ProcessedImage.WithTransform](https://www.naps2.com/sdk/doc/api/NAPS2.Images.ProcessedImage.html) retains the operation as immutable transform state so the source image data is not rewritten.


Research checked on 2026-10-06 for proportional page cropping:

- [NAPS2 CropTransform API](https://www.naps2.com/sdk/doc/api/NAPS2.Images.Transforms.CropTransform.html) defines left/right/top/bottom pixel margins and optional original width/height references.
- [NAPS2 crop transformer source](https://github.com/cyanfish/naps2/blob/master/NAPS2.Images/Transforms/AbstractImageTransformer.cs) scales crop margins by the rendered image’s size relative to those reference dimensions. Using the existing small thumbnail’s dimensions therefore keeps percentage presets proportional without loading a full-resolution page just to measure it.
- [NAPS2 CropTransform source](https://github.com/cyanfish/naps2/blob/master/NAPS2.Images/Transforms/CropTransform.cs) supports safe composition of successive crop transforms when both have reference dimensions; removing only CropTransform entries preserves other immutable edits.


Research checked on 2026-10-07 for optional searchable-PDF OCR:

- [NAPS2 PdfExporter API](https://www.naps2.com/sdk/doc/api/NAPS2.Pdf.PdfExporter.html) accepts optional `OcrParams` during PDF export; [ScanningContext](https://www.naps2.com/sdk/doc/api/NAPS2.Scan.ScanningContext.html) exposes `OcrEngine` for OCR requests.
- [NAPS2 OcrParams API](https://www.naps2.com/sdk/doc/api/NAPS2.Ocr.OcrParams.html) configures the language code, OCR mode, and timeout. `OcrMode.Fast` is used for the `tessdata_fast` model family.
- [NAPS2 TesseractOcrEngine API](https://www.naps2.com/sdk/doc/api/NAPS2.Ocr.TesseractOcrEngine.html) documents `Bundled(languageDataPath)` and `System()` engine creation; [NAPS2.Tesseract.Binaries 1.3.0](https://www.nuget.org/packages/NAPS2.Tesseract.Binaries/1.3.0) supplies cross-platform Tesseract executables and its NuGet metadata declares Apache-2.0.
- [NAPS2 OCR setup documentation](https://www.naps2.com/doc/ocr) describes language model installation and searchable PDF behavior. This app does not auto-download models: it asks the user to place the selected model in `%LOCALAPPDATA%\\NoorScannerPC\\tessdata`.
- [Tesseract trained-data guide](https://tesseract-ocr.github.io/tessdoc/Data-Files.html) lists supported data sets and language identifiers; [tessdata_fast](https://github.com/tesseract-ocr/tessdata_fast) provides the fast models and states its license is Apache-2.0. The selected codes are English `eng`, French `fra`, and Arabic `ara`.

The release build compiles the OCR path and the Linux tests cover model/engine preflight messages. A Windows integration test exercises runtime engine failure handling. Real OCR accuracy remains subject to validation with installed models and representative Windows documents.

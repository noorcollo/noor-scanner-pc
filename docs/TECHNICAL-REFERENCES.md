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

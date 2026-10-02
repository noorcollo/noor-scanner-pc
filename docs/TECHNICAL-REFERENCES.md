# Technical references

Research checked on 2026-10-02 for the local document import milestone:

- [NAPS2 SDK FileImporter API](https://www.naps2.com/sdk/doc/api/NAPS2.ImportExport.FileImporter.html) — `FileImporter(ScanningContext)` and `Import(string)` expose an asynchronous PDF/image page importer.
- [NAPS2 SDK FileStorageManager API](https://www.naps2.com/sdk/doc/api/NAPS2.Images.FileStorageManager.html) — `FileStorageManager(string folderPath)` provides a per-context on-disk page storage location.
- [NAPS2 PDF import sample](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/PdfImportSample.cs) — documents that PDF import requires the optional `NAPS2.Pdfium.Binaries` NuGet package and shows `PdfImporter` with `ScanningContext`.
- [NAPS2 file storage sample](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/FileStorageSample.cs) — demonstrates setting `ScanningContext.FileStorageManager` to reduce memory use for page sets.
- [NAPS2.Pdfium.Binaries 1.3.0 on NuGet](https://www.nuget.org/packages/NAPS2.Pdfium.Binaries/1.3.0) — lists the package purpose and supported runtime platforms. Its NuGet package metadata declares Apache-2.0 and links to [cyanfish/naps2-pdfium](https://github.com/cyanfish/naps2-pdfium).

This page records source material, not a guarantee that every possible user file or scanner is supported. File compatibility and physical scanner behavior still require Windows/device testing.

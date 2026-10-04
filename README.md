# Noor Scanner PC

A Noor-branded Windows desktop app for scanning paper documents and assembling local photos, images, and PDFs into a combined document. The app is still an early staged project; see the roadmap for what remains planned.

## Current milestone

- Discover connected scanners supported by the NAPS2 SDK on Windows (including WIA/TWAIN devices).
- Choose automatic, flatbed, feeder, or duplex paper source.
- Scan at 150, 200, 300, or 600 DPI with optional automatic deskew.
- Save feeder pages together as a multi-page PDF.
- Import one or more local PDF, JPEG, PNG, BMP, or TIFF files into a page workspace.
- Preview each imported page with a small thumbnail.
- Move the selected page earlier or later in the queue, or remove it from the workspace.
- Rotate the selected page 90° clockwise or counterclockwise; the source image remains unchanged, while the preview and exported PDF use the new orientation.
- Export imported pages together as one PDF in the displayed queue order.
- Keep scanning, import, and PDF export local; Noor Scanner does not upload document contents.
- Cancel a physical scan; closing the app cancels an in-progress file import before workspace cleanup.

Page previews, page reordering, removal, queue-ordered PDF export, and non-destructive quarter-turn rotation are available. Cropping and image enhancement remain planned features and are not yet available.

## Requirements

- Windows 10 or later (64-bit recommended).
- .NET 8 Desktop Runtime for running a framework-dependent build.
- A scanner with a supported Windows WIA or TWAIN driver for hardware scanning.

The app uses the [NAPS2 SDK](https://www.naps2.com/sdk) for scanner integration, image import, and PDF output. PDF import uses the optional [NAPS2.Pdfium.Binaries](https://www.nuget.org/packages/NAPS2.Pdfium.Binaries/1.3.0) runtime package. Actual device discovery and scan quality depend on the scanner model, its Windows driver, and the selected paper source. No physical scanner is available in the development sandbox, so hardware behavior must be verified on a Windows PC with a scanner attached.

## Build and test

Install the .NET 8 SDK, then run from the repository root:

```powershell
dotnet restore NoorScanner.sln
dotnet test NoorScanner.sln --configuration Release
dotnet build src/NoorScanner.App/NoorScanner.App.csproj --configuration Release
```

The core unit tests do not require a scanner. The GitHub Actions workflow builds the Windows desktop app and runs tests on a Windows runner.

## Privacy

Scanned pages and imported document pages are processed locally. The app does not upload file contents or modify the originals. During the current session, imported page data is held in a per-session folder under the user's system temporary directory to limit memory use; the workspace removes that temporary data on shutdown on a best-effort basis. Temporary storage is not secure erasure, so users handling highly sensitive documents should follow their organization's local-data handling procedures.

## Roadmap

See [ROADMAP.md](ROADMAP.md) for the staged upgrade plan. Features marked planned are not yet available in the current milestone.

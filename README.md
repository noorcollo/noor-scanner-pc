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
- Apply or reset NAPS2's Document enhancement preset on a selected page; reset preserves other edits such as rotation.
- Trim 2%, 5%, or 10% from each current page edge; applying another preset trims the remaining page again, and Reset crop preserves rotation and enhancement.
- Export imported pages together as one PDF in the displayed queue order.
- Optionally add searchable OCR text to scanned or workspace PDFs using the local Tesseract engine; English, French, and Arabic are selectable when their language models are installed.
- Keep scanning, import, and PDF export local; Noor Scanner does not upload document contents.
- Cancel a physical scan; closing the app cancels an in-progress file import before workspace cleanup.

Page previews, page reordering, removal, queue-ordered PDF export, non-destructive quarter-turn rotation, an optional Document enhancement preset, proportional symmetric crop presets, and opt-in local OCR are available on the staging branch. The crop tool is preset-based rather than freehand; the Document preset is intended for text pages and may change photo tones. OCR is off by default and depends on a separately installed language model. Reset actions preserve other page edits. Freehand crop selection and photo-specific adjustment remain planned.

## Requirements

- Windows 10 or later (64-bit recommended).
- .NET 8 Desktop Runtime for running a framework-dependent build.
- A scanner with a supported Windows WIA or TWAIN driver for hardware scanning.

The app uses the [NAPS2 SDK](https://www.naps2.com/sdk) for scanner integration, image import, and PDF output. PDF import uses [NAPS2.Pdfium.Binaries](https://www.nuget.org/packages/NAPS2.Pdfium.Binaries/1.3.0); OCR uses [NAPS2.Tesseract.Binaries](https://www.nuget.org/packages/NAPS2.Tesseract.Binaries/1.3.0). Actual device discovery and scan quality depend on the scanner model, its Windows driver, and the selected paper source. No physical scanner is available in the development sandbox, so hardware behavior must be verified on a Windows PC with a scanner attached.

## Optional searchable PDFs (OCR)

OCR is opt-in and runs locally for either a new scan or the document workspace export. Choose English (`eng`), French (`fra`), or Arabic (`ara`). The Tesseract executable is supplied by the NAPS2 runtime package, but language model files are not bundled or automatically downloaded. To enable OCR, place the selected file (`eng.traineddata`, `fra.traineddata`, or `ara.traineddata`) in `%LOCALAPPDATA%\NoorScannerPC\tessdata`; the official [Tesseract tessdata_fast repository](https://github.com/tesseract-ocr/tessdata_fast) provides the fast models. Noor Scanner checks for the selected model and OCR engine before scanning/exporting. If OCR fails or times out while processing pages, the temporary PDF is discarded rather than reported as a successful searchable PDF. One language is selected per export; recognition accuracy depends on scan quality and still needs validation with representative documents.

## Build and test

Install the .NET 8 SDK, then run from the repository root:

```powershell
dotnet restore NoorScanner.sln
dotnet test NoorScanner.sln --configuration Release
dotnet build src/NoorScanner.App/NoorScanner.App.csproj --configuration Release
```

The core unit tests do not require a scanner. The GitHub Actions workflow builds the Windows desktop app and runs tests on a Windows runner.

## Privacy

Scanned pages, OCR, and imported document pages are processed locally. The app does not upload file contents or modify the originals; OCR language models must be installed locally by the user and are not included in this repository. During the current session, imported page data is held in a per-session folder under the user's system temporary directory to limit memory use; the workspace removes that temporary data on shutdown on a best-effort basis. Temporary storage is not secure erasure, so users handling highly sensitive documents should follow their organization's local-data handling procedures.

## Roadmap

See [ROADMAP.md](ROADMAP.md) for the staged upgrade plan. Features marked planned are not yet available in the current milestone.

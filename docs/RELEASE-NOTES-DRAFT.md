# Release notes draft — not published

**Status:** Draft for owner review. No version, tag, binary release, or public GitHub promotion has been created.

## Current staging features

- Discover Windows scanners supported through WIA/TWAIN and scan multi-page PDFs with selectable resolution, paper source, and automatic deskew.
- Import local PDF and image files into a page workspace; preview, reorder, remove, rotate, apply document correction, and use proportional crop presets before combined PDF export.
- Optionally create searchable PDFs using local Tesseract OCR for English (`eng`), French (`fra`), or Arabic (`ara`) on scanner and workspace exports. OCR is off by default; missing language data or engine failures are reported, and failed OCR output is discarded.
- Process document content locally; the app does not upload scanner pages or imported documents.

## Requirements and OCR setup

- Windows 10 or later, .NET 8 Desktop Runtime, and a compatible scanner driver for physical scanning.
- The NAPS2 Tesseract executable is supplied through its runtime package. The user must separately install the chosen Tesseract `.traineddata` file into `%LOCALAPPDATA%\NoorScannerPC\tessdata`; the app does not download language models.

## Verification to date

The development branch passed 52/52 tests locally and in Windows GitHub Actions. The Windows app build succeeded with 0 warnings and 0 errors. Windows CI exercised the OCR engine-failure path. The dependency audit reported no known vulnerable NuGet packages from the configured source.

## Outstanding before a release recommendation

- Roadmap step 5, reusable scan settings, is not implemented.
- Roadmap step 6, document organization and broader recovery/accessibility improvements, is not implemented.
- No physical scanner was available; scanner discovery, capture, and real device compatibility need validation.
- OCR was not tested with real language packs or representative documents; accuracy and multilingual needs remain unverified.
- The repository owner has not selected a license for the application's own source.

This draft describes the private `automation/daily-upgrades` branch only. The repository remains private; `main` has not been changed; no release has been published. Do not distribute this draft as final release notes until the owner reviews the blockers and approves the exact publication target.

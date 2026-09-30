# Noor Scanner PC

A Noor-branded Windows desktop app for scanning paper documents into clear, combined PDF files. The first milestone focuses on reliable scanner discovery and capture; the scheduled upgrades will add document-review and CamScanner-style enhancement features.

## Current milestone

- Discover connected scanners supported by the NAPS2 SDK on Windows (including WIA/TWAIN devices).
- Choose automatic, flatbed, feeder, or duplex paper source.
- Scan at 150, 200, 300, or 600 DPI with optional automatic deskew.
- Save feeder pages together as a multi-page PDF.
- Keep scanning and PDF export local; Noor Scanner does not upload document contents.
- Cancel a scan from the app.

## Requirements

- Windows 10 or later (64-bit recommended).
- .NET 8 Desktop Runtime for running a framework-dependent build.
- A scanner with a supported Windows WIA or TWAIN driver.

The app uses the [NAPS2 SDK](https://www.naps2.com/sdk) for scanner-driver integration and PDF output. Actual device discovery and scan quality depend on the scanner model, its Windows driver, and the selected paper source. No physical scanner is available in the development sandbox, so hardware behavior must be verified on a Windows PC with a scanner attached.

## Build and test

Install the .NET 8 SDK, then run from the repository root:

```powershell
dotnet restore NoorScanner.sln
dotnet test NoorScanner.sln --configuration Release
dotnet build src/NoorScanner.App/NoorScanner.App.csproj --configuration Release
```

The core unit tests do not require a scanner. The GitHub Actions workflow builds the Windows desktop app and runs tests on a Windows runner.

## Privacy

Scanned pages and generated PDFs are processed locally and saved to a location you choose. This app has no cloud upload or telemetry implementation. Keep private documents in a suitable local folder and review the chosen destination before each scan.

## Roadmap

See [ROADMAP.md](ROADMAP.md) for the staged upgrade plan. Features marked planned are not yet available in the current milestone.

# Daily upgrade log

## Starter milestone — 2026-09-30

The initial Windows WPF app shell discovers connected scanners, exposes resolution and paper-source controls, supports cancellation, and saves feeder scans as multi-page PDFs. It uses the NAPS2 SDK and includes validated settings, core tests, a Windows CI workflow, privacy notes, and a seven-step roadmap.

Validation: the starter passed all 12 core tests and the Windows-targeted WPF build completed with 0 warnings and 0 errors. No physical scanner is attached to the development sandbox, so WIA/TWAIN device discovery, physical page capture, and scan quality still require testing on a Windows PC with a compatible scanner and driver.

The GitHub repository is private for staging. The baseline is on `main`; scheduled feature work is confined to `automation/daily-upgrades`. Seven daily sessions are configured for 10:00 AM Africa/Algiers, October 1–7. Final public visibility and release remain gated on review of the exact final contents and target.

## Scheduled session — 2026-10-01

No code change was made in this run because the GitHub connector was disabled in the scheduled-task session. The run stopped instead of using raw CLI credentials. The existing GitHub connector was enabled through the supported authorization flow during the October 2 session.

## Scheduled session — 2026-10-02 — Local document import

Added multi-file import for local PDF, JPEG, PNG, BMP, and TIFF files. Imported pages now appear in a workspace list in the file-picker order and can be exported together as one PDF. The workspace uses NAPS2 `FileImporter`, adds the optional PDFium binary package for PDF input, and stores intermediate page data in a per-session temporary directory to reduce memory use. The UI cancels an import when the window closes and removes workspace temp data on shutdown on a best-effort basis. Page thumbnails, reordering, removal, and image corrections remain planned for later sessions.

Local validation: `dotnet test NoorScanner.sln --configuration Release` passed 24 test cases; the Windows-only image-to-PDF-to-image round-trip test is guarded and did not execute on this Linux sandbox. `dotnet build src/NoorScanner.App/NoorScanner.App.csproj --configuration Release` succeeded with 0 warnings and 0 errors. GitHub Actions on Windows succeeded for commit `71a1dad` (restore, core tests, and Windows app build), exercising the platform-specific test path. No physical scanner was available, so scanner hardware behavior remains untested.


## Scheduled session — 2026-10-03 — Page queue and previews

Added a thumbnail preview to each imported page, explicit Move up/Move down controls, and page removal. The workspace reindexes the list after moves/removals; PDF export consumes that same ordered page collection, so the displayed queue is the output order. Thumbnails are generated at small preview size with NAPS2 `ThumbnailRenderer`, cached as frozen WPF bitmaps, and removed from the cache when a page is deleted. Added platform-independent queue tests for moving forward/backward, invalid indexes, no-op moves, removals, and invalid removal.

Validation: `dotnet test NoorScanner.sln --configuration Release` passed 34 tests. `dotnet build src/NoorScanner.App/NoorScanner.App.csproj --configuration Release` succeeded with 0 warnings and 0 errors; XAML parsed and compiled. Windows GitHub Actions run `37112301265` succeeded for commit `3e22673`, including tests and the Windows app build. No physical scanner is attached to this development sandbox, so scanner discovery, capture, and image quality remain unverified on hardware.


## Scheduled session — 2026-10-04 — Non-destructive page rotation

Added Rotate left and Rotate right actions for the selected workspace page. Rotation is recorded as an immutable NAPS2 transform rather than modifying the original image data; thumbnail rendering and PDF export consume the transformed page state. The Windows integration test now checks clockwise rotation, the inverse turn returning to the unrotated transform state, and PDF export of the rotated page. Cropping and enhancement remain planned.

Local validation: `dotnet test NoorScanner.sln --configuration Release` passed 34 tests; the Windows-only workspace integration test compiled but its Windows path did not execute in this Linux sandbox. `dotnet build src/NoorScanner.App/NoorScanner.App.csproj --configuration Release` succeeded with 0 warnings and 0 errors; XAML parsed successfully. Windows GitHub Actions is pending for this branch update. No physical scanner is attached, so scanner discovery, capture, and hardware image quality remain unverified.


Post-push verification: Windows GitHub Actions run `37191414629` succeeded for commit `ec75476`, including the Windows-only rotation/PDF integration path, the test suite, and the Windows app build. No physical scanner was attached for device/capture validation.


## Scheduled session — 2026-10-05 — Optional document enhancement

Added an `Enhance document` action using NAPS2 `CorrectionMode.Document` and a `Reset enhancement` action. Both are non-destructive transform-state updates: reset removes only this document-correction preset and preserves rotation and unrelated transforms. The selection-aware buttons show which action is available. This preset is intended for text documents; it may alter photographs, and crop/photo-specific adjustment remain planned.

Local validation: `dotnet test NoorScanner.sln --configuration Release` passed 37 tests, including three new platform-independent tests for idempotent enable, selective reset, and rotation preservation. The Windows-only workspace integration path now applies and renders the correction to a generated BMP before PDF export, but it did not execute in the Linux sandbox. `dotnet build src/NoorScanner.App/NoorScanner.App.csproj --configuration Release` succeeded with 0 warnings and 0 errors; XAML parsed successfully. Windows GitHub Actions is pending for this update. No physical scanner or representative real scan was available to assess image-quality effects.

Post-push verification: Windows GitHub Actions run `37288297267` succeeded for commit `4ce4b12`: 37/37 tests passed, the Windows app build had 0 warnings and 0 errors, and the Windows-only integration path exported a document-enhanced page to PDF before reset. GitHub also emitted an upstream Node.js 20 deprecation warning for `actions/checkout@v4` and `actions/setup-dotnet@v4`; it did not affect this run and should be reviewed during CI maintenance.


## Scheduled session — 2026-10-06 — Proportional symmetric crop

Added 2%, 5%, and 10% trim presets for each edge of the selected page. Margins are derived from the current cached thumbnail dimensions and NAPS2 scales those reference margins to the full-resolution render. Repeated trims apply to the remaining page; `Reset crop` removes only crop transforms and preserves rotation and document enhancement. This is a preset-based crop, not a freehand selection tool.

Local validation: `dotnet test NoorScanner.sln --configuration Release` passed 44 tests, including seven new cases for margin calculation, repeated composition, preserving other edits, and invalid inputs. The Windows-only integration path applies crop and document correction to a generated BMP, exports PDF, and checks that reset preserves other transforms; it did not execute in the Linux sandbox. `dotnet build src/NoorScanner.App/NoorScanner.App.csproj --configuration Release` succeeded with 0 warnings and 0 errors; XAML parsed successfully. Windows GitHub Actions is pending for this update. No physical scanner or representative real scans were available for hardware or visual-quality assessment.

Post-push verification: Windows GitHub Actions run `37440865163` succeeded for commit `f155f03`: 44/44 tests passed and the Windows app build completed with 0 warnings and 0 errors. The Windows integration path exercised rotation, document enhancement, proportional crop, PDF export, and crop reset while preserving the other edits. No physical scanner was available for hardware validation.


## Scheduled session — 2026-10-07 — Optional searchable-PDF OCR

Added opt-in searchable-PDF export for both new scanner captures and the local document workspace. The UI supports English (`eng`), French (`fra`), and Arabic (`ara`) with OCR off by default. The NAPS2-bundled Tesseract runtime is configured against a user-installed `.traineddata` file under `%LOCALAPPDATA%\NoorScannerPC\tessdata`; the app preflights the selected model and engine before starting a scan/export, and reports clear setup instructions if unavailable. OCR errors/timeouts abort export; PDFs are written to a temporary path and only moved into place on success. Language models were not added to the repository; no user documents, scanner content, or secrets were added.

Local validation: `dotnet test NoorScanner.sln --configuration Release` passed 52 tests on the Linux sandbox. The new Windows-only OCR failure integration test compiled but its Windows-specific path did not execute in this environment. `dotnet build src/NoorScanner.App/NoorScanner.App.csproj --configuration Release` succeeded with 0 warnings and 0 errors; the XAML and project XML parsed successfully; `git diff --check` passed. Windows GitHub Actions is pending for this branch update. No physical scanner or OCR language model was available, so actual device capture and text-recognition quality remain unverified.


Post-push verification: Windows GitHub Actions run `37599791469` succeeded for feature commit `bc9707a`: 52/52 tests passed, the Windows app build completed with 0 warnings and 0 errors, and the Windows-only OCR engine-failure integration test executed. The failure case confirms no final PDF remains when OCR cannot run.

Final-run audit: `dotnet list NoorScanner.sln package --vulnerable --include-transitive` reported no vulnerable packages from the configured NuGet source. OCR stays local; no language models, scanner documents, user content, or secrets were committed. The repository remains private, `main` remains the default branch at `baf13c8`, and GitHub reports no releases. No merge, visibility change, or release was made. The application source still has no selected license. Roadmap steps 5 (reusable scan settings) and 6 (document organization) remain unimplemented; physical scanning and actual OCR quality with installed language packs were not tested. The scheduled task expires at 2026-10-07 23:00 UTC (2026-10-08 00:00 Africa/Algiers); no later run is configured. Keep release deferred until the owner reviews the exact contents and target.

# Daily upgrade log

## Starter milestone — 2026-09-30

The initial Windows WPF app shell discovers connected scanners, exposes resolution and paper-source controls, supports cancellation, and saves feeder scans as multi-page PDFs. It uses the NAPS2 SDK and includes validated settings, core tests, a Windows CI workflow, privacy notes, and a seven-step roadmap.

Validation: the starter passed all 12 core tests and the Windows-targeted WPF build completed with 0 warnings and 0 errors. No physical scanner is attached to the development sandbox, so WIA/TWAIN device discovery, physical page capture, and scan quality still require testing on a Windows PC with a compatible scanner and driver.

The GitHub repository is private for staging. The baseline is on `main`; scheduled feature work is confined to `automation/daily-upgrades`. Seven daily sessions are configured for 10:00 AM Africa/Algiers, October 1–7. Final public visibility and release remain gated on review of the exact final contents and target.

## Scheduled session — 2026-10-01

No code change was made in this run because the GitHub connector was disabled in the scheduled-task session. The run stopped instead of using raw CLI credentials. The existing GitHub connector was enabled through the supported authorization flow during the October 2 session.

## Scheduled session — 2026-10-02 — Local document import

Added multi-file import for local PDF, JPEG, PNG, BMP, and TIFF files. Imported pages now appear in a workspace list in the file-picker order and can be exported together as one PDF. The workspace uses NAPS2 `FileImporter`, adds the optional PDFium binary package for PDF input, and stores intermediate page data in a per-session temporary directory to reduce memory use. The UI cancels an import when the window closes and removes workspace temp data on shutdown on a best-effort basis. Page thumbnails, reordering, removal, and image corrections remain planned for later sessions.

Local validation: `dotnet test NoorScanner.sln --configuration Release` passed 24 test cases; the Windows-only image-to-PDF-to-image round-trip test is guarded and did not execute on this Linux sandbox. `dotnet build src/NoorScanner.App/NoorScanner.App.csproj --configuration Release` succeeded with 0 warnings and 0 errors. The pushed Windows CI run must execute the real round-trip test. No physical scanner was available, so scanner hardware behavior remains untested.

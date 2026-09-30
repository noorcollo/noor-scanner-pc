# Daily upgrade log

## Starter milestone — 2026-09-30

The initial Windows WPF app shell now discovers connected scanners, exposes resolution and paper-source controls, supports cancellation, and saves feeder scans as multi-page PDFs. The scanner service uses the NAPS2 SDK, with validated settings, core tests, a Windows CI workflow, privacy notes, and the seven-step roadmap in place.

Validation completed: `dotnet test NoorScanner.sln --configuration Release` passed all 12 tests; the Windows-targeted WPF app built successfully with 0 warnings and 0 errors. The sandbox has no Windows scanner attached, so real WIA/TWAIN device discovery, physical page capture, and scan quality still require testing on a Windows PC with a compatible scanner and driver.

The new repository is currently private for safe staging. The baseline is on `main`; scheduled feature work will be pushed only to `automation/daily-upgrades`. Seven daily sessions are active for **10:00 AM Africa/Algiers**, from **2026-10-01 through 2026-10-07**. Final public visibility and release remain gated on review of the exact final contents and target.

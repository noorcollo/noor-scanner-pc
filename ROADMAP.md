# Noor Scanner PC — Seven-Session Upgrade Roadmap

This is the planned order for the seven scheduled 10:00 AM Algeria-time sessions. Every session must inspect the current branch, make one focused improvement, run the available tests/build, and update the daily log. Planned items are not claimed as shipped until committed and verified.

1. **Bring documents in (shipped 2026-10-02):** import local photos, images, and PDFs into a page workspace, list imported pages in file-picker order, and export them as one PDF. Thumbnails, page reordering, and removal remain in step 2.
2. **Build the page queue (shipped 2026-10-03):** preview imported pages with thumbnails, move the selected page earlier or later, remove pages, and export in the current queue order.
3. **Clean up scans (in progress; rotation shipped 2026-10-04):** rotate the selected page 90° clockwise or counterclockwise using the immutable NAPS2 transform state, with matching thumbnails and PDF output. Crop and image enhancement remain planned.
4. **Searchable PDFs:** add opt-in OCR, language selection, and clear behavior when the OCR engine or language data is unavailable.
5. **Reusable scan settings:** save and reuse resolution, source, color, and page-size preferences.
6. **Document organization:** improve naming, recent output access, error recovery, and keyboard/accessibility behavior.
7. **Release-quality improvement and audit:** complete one final usability improvement, run all tests and the Windows build, inspect dependencies/privacy/licensing, and prepare release notes. Do not make the repository public, merge to the default branch, or publish a release without first showing the exact final contents and target for the user's confirmation.

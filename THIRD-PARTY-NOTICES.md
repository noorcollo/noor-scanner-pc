# Third-party notices

## NAPS2 SDK

Noor Scanner PC references the NAPS2.Sdk NuGet package for scanner access, local image/PDF import, and PDF export. The NAPS2 project documents the SDK components under the GNU Lesser General Public License, version 2.1 or later (LGPL-2.1-or-later). See the official [NAPS2 SDK page](https://www.naps2.com/sdk) and [NAPS2 source repository](https://github.com/cyanfish/naps2) for the current license text and package details. The scanner SDK is an independently maintained open-source component; Noor Scanner PC does not claim authorship of it.

## NAPS2.Pdfium.Binaries / PDFium

The app references `NAPS2.Pdfium.Binaries` 1.3.0, which supplies precompiled PDFium binaries required by NAPS2 for PDF import. NuGet metadata declares the package license as Apache-2.0 and identifies [cyanfish/naps2-pdfium](https://github.com/cyanfish/naps2-pdfium) as its upstream repository; the package states that its binaries are sourced from [bblanchon/pdfium-binaries](https://github.com/bblanchon/pdfium-binaries). The upstream PDFium project includes separate copyright and third-party notices. Preserve the package's included license/readme materials and review the upstream notices when redistributing a packaged application.

The app source in this repository has no separate license grant unless and until a `LICENSE` file is added by the repository owner. Third-party licenses do not automatically grant a license to Noor Scanner PC's own source.

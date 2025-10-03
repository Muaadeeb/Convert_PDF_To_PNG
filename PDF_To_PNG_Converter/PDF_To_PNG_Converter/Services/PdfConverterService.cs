using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PdfiumViewer;

namespace PDF_To_PNG_Converter.Services
{
    public class PdfConverterService
    {
        public async Task<List<string>> ConvertPdfToPngAsync(
            string pdfPath,
            string outputDir,
            int dpi = 300,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pdfPath))
                throw new ArgumentException("PDF path is required.", nameof(pdfPath));
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException("PDF file not found.", pdfPath);
            if (string.IsNullOrWhiteSpace(outputDir))
                throw new ArgumentException("Output directory is required.", nameof(outputDir));

            Directory.CreateDirectory(outputDir);

            return await Task.Run(() =>
            {
                var outputFiles = new List<string>();
                using var document = PdfDocument.Load(pdfPath);

                var baseName = Path.GetFileNameWithoutExtension(pdfPath);

                for (int pageIndex = 0; pageIndex < document.PageCount; pageIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Annotations + print-quality flag at higher DPI
                    var flags = PdfRenderFlags.Annotations;
                    if (dpi >= 300) flags |= PdfRenderFlags.ForPrinting;

                    using var image = document.Render(pageIndex, dpi, dpi, flags);

                    var fileName = $"{baseName}_p{pageIndex + 1:000}.png";
                    var filePath = Path.Combine(outputDir, fileName);

                    image.Save(filePath, ImageFormat.Png);
                    outputFiles.Add(filePath);
                }

                return outputFiles;
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}

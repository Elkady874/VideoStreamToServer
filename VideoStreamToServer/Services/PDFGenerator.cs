using iText.Html2pdf;
using iText.Kernel.Pdf;

namespace VideoStreamToServer.Services
{
    public class PDFGenerator
    {
       public void ConvertHtmlToPdf(string htmlPath, string pdfDest)
        {
            // Read HTML content from file
            string htmlContent = File.ReadAllText(htmlPath);

            // Initialize PDF writer
            using (FileStream pdfFile = new FileStream(pdfDest, FileMode.Create))
            {
                PdfWriter writer = new PdfWriter(pdfFile);
                PdfDocument pdfDoc = new PdfDocument(writer);

                // Convert HTML to PDF
                HtmlConverter.ConvertToPdf(htmlContent, pdfDoc, new ConverterProperties());
            }
        }
    }
}

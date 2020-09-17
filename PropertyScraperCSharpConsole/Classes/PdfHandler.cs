using Syncfusion.Drawing;
using SfPdf = Syncfusion.Pdf;
using SfGraphics = Syncfusion.Pdf.Graphics;
using Syncfusion.HtmlConverter;

namespace PropertyScraperCSharpConsole.Classes
{
    public class PdfHandler
    {
        //PdfDocument pdfDocument;
        //PdfPage pdfPage;
        //PdfGraphics pdfGraphics;
        //PdfFont pdfFont;

        HtmlToPdfConverter htmlConverter;
        WebKitConverterSettings webKitSettings;

        public PdfHandler()
        {
            //pdfDocument = new PdfDocument();
            //pdfPage = pdfDocument.Pages.Add();
            //pdfGraphics = pdfPage.Graphics;
            //pdfFont = new PdfStandardFont(PdfFontFamily.Courier, 20);

            //pdfGraphics.DrawString("Hello World", pdfFont, PdfBrushes.Black, new PointF(0, 0));


            htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            webKitSettings = new WebKitConverterSettings();

            //https://help.syncfusion.com/file-formats/pdf/create-pdf-file-in-c-sharp-vb-net#converting-html-contents-to-pdf
        }
    }
}

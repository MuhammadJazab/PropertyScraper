using SfDrawing = Syncfusion.Drawing;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using SfGraphics = Syncfusion.Pdf.Graphics;
using System;
using System.IO;
using PropertyScraperCSharpConsole.Models;
using System.Text.RegularExpressions;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using System.Drawing;
using System.Net;

namespace PropertyScraperCSharpConsole.Classes
{
    public class PdfHandler
    {
        static HtmlToPdfConverter htmlConverter;
        static WebKitConverterSettings webKitSettings;

        public static string SavePdf(RightMoveModel rightMoveModel)
        {
            try
            {
                string archiveFolder = Path.Combine(Environment.CurrentDirectory, "Archives");
                string qtBinariespath = Path.Combine(Environment.CurrentDirectory, "QtBinariesDotNetCore");
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources\\pdftemplate.pdf");
                string tempFolder = Path.GetTempPath();

                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(new FileStream(path, FileMode.Open));

                htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
                webKitSettings = new WebKitConverterSettings();

                webKitSettings.WebKitPath = qtBinariespath;
                webKitSettings.EnableForm = true;

                htmlConverter.ConverterSettings = webKitSettings;

                if (!Directory.Exists(archiveFolder))
                {
                    Directory.CreateDirectory(archiveFolder);
                }

                if (loadedDocument.PageCount > 0)
                {
                    PdfLoadedPage pdfLoadedPage = loadedDocument.Pages[1] as PdfLoadedPage;

                    PdfTemplate pdfTemplate = new PdfTemplate(900, 600);

                    PdfFont pdfFont = new PdfStandardFont(PdfFontFamily.Helvetica, 15);

                    PdfBrush brush = new PdfSolidBrush(SfDrawing.Color.Black);

                    byte[] imageBytes = new WebClient().DownloadData(rightMoveModel.PropertyMainPicture);
                    Stream imageStream = new MemoryStream(imageBytes);

                    pdfTemplate.Graphics.DrawString($"Property Address: {rightMoveModel.PropertyAddress}", pdfFont, brush, 100, 30);
                    pdfTemplate.Graphics.DrawString($"Property Type: {rightMoveModel.PropertyType}", pdfFont, brush, 100, 50);
                    pdfTemplate.Graphics.DrawString($"PropertyPrice: {rightMoveModel.PropertyPrice} ", pdfFont, brush, 100, 70);
                    pdfTemplate.Graphics.DrawImage
                        (PdfImage.FromStream(imageStream), new SfDrawing.PointF(100, 100), new SfDrawing.SizeF(400, 400));

                    pdfLoadedPage.Graphics.DrawPdfTemplate(pdfTemplate, SfDrawing.PointF.Empty);

                    string rawName = rightMoveModel
                    .PropertyUrl
                    .Replace("/", "")
                    .Replace("-", "")
                    .Replace(".", "")
                    .Replace(":", "")
                    .Replace("//", "");

                    string fileName = Regex.Match(rawName, @"(\d+(?:\.\d{1,2})?)").Value;

                    if (!string.IsNullOrEmpty(rightMoveModel.PropertyHeatHtmlString))
                    {
                        PdfDocument document = htmlConverter
                            .Convert(rightMoveModel.PropertyHeatHtmlString, tempFolder);

                        document.Save(new FileStream(Path.Combine(tempFolder, $"temp{fileName}.pdf"), FileMode.Create));
                        document.Close(true);

                        PdfLoadedDocument tempLoadeDocument = new PdfLoadedDocument(new FileStream(Path.Combine(tempFolder, $"temp{fileName}.pdf"), FileMode.Open));

                        loadedDocument.ImportPage(tempLoadeDocument, 2);
                    }

                    string savePath = Path.Combine(archiveFolder, $"{fileName}.pdf");

                    loadedDocument.Save(new FileStream(savePath, FileMode.Create));

                    loadedDocument.Close(true);

                    return $"file successfully saved at: {savePath}";
                }
                else
                {
                    Console.WriteLine("Invalid PDF file");
                    return $"Invalid PDF file";
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                return $"Unable to save the file";
            }
        }
    }
}

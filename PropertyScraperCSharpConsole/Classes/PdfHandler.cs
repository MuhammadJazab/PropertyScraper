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

        public static string SavePdf(string url, string fileName)
        {
            try
            {
                string archiveFolder = Path.Combine(Environment.CurrentDirectory, "Archives");
                string qtBinariespath = Path.Combine(Environment.CurrentDirectory, "QtBinariesDotNetCore");

                htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
                webKitSettings = new WebKitConverterSettings();

                webKitSettings.WebKitPath = qtBinariespath;
                webKitSettings.EnableForm = true;

                htmlConverter.ConverterSettings = webKitSettings;

                PdfDocument document = htmlConverter.Convert(url);

                if (!Directory.Exists(archiveFolder))
                {
                    Directory.CreateDirectory(archiveFolder);
                }

                string savePath = Path.Combine(archiveFolder, $"{fileName}.pdf");

                FileStream fileStream = new FileStream(savePath, FileMode.Create);

                document.Save(fileStream);

                document.Close(true);

                return $"file successfully saved at: {savePath}";
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                return $"Unable to save the file";
            }
        }

        public static string SavePdf(string html, string temporaryFolderPath, string fileName)
        {
            try
            {
                string archiveFolder = Path.Combine(Environment.CurrentDirectory, "Archives");
                string qtBinariespath = Path.Combine(Environment.CurrentDirectory, "QtBinariesDotNetCore");

                htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
                webKitSettings = new WebKitConverterSettings();

                webKitSettings.WebKitPath = qtBinariespath;
                webKitSettings.EnableForm = true;

                htmlConverter.ConverterSettings = webKitSettings;

                PdfDocument document = htmlConverter.Convert(html, temporaryFolderPath);

                if (!Directory.Exists(archiveFolder))
                {
                    Directory.CreateDirectory(archiveFolder);
                }

                string savePath = Path.Combine(archiveFolder, $"{fileName}.pdf");

                FileStream fileStream = new FileStream(savePath, FileMode.Create);

                document.Save(fileStream);

                document.Close(true);

                return $"file successfully saved at: {savePath}";
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                return $"Unable to save the file";
            }
        }

        public static string SavePdf(RightMoveModel rightMoveModel)
        {
            try
            {
                string archiveFolder = Path.Combine(Environment.CurrentDirectory, "Archives");
                string qtBinariespath = Path.Combine(Environment.CurrentDirectory, "QtBinariesDotNetCore");
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources\\pdftemplate.pdf");

                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(new FileStream(path, FileMode.Open));

                htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
                webKitSettings = new WebKitConverterSettings();

                webKitSettings.WebKitPath = qtBinariespath;
                webKitSettings.EnableForm = true;

                htmlConverter.ConverterSettings = webKitSettings;

               // PdfDocument document = htmlConverter.Convert(rightMoveModel.PropertyUrl);

                if (!Directory.Exists(archiveFolder))
                {
                    Directory.CreateDirectory(archiveFolder);
                }

                if (loadedDocument.PageCount > 0)
                {
                    PdfLoadedPage pdfLoadedPage = loadedDocument.Pages[1] as PdfLoadedPage;

                    PdfTemplate pdfTemplate = new PdfTemplate(100, 50);

                    PdfFont pdfFont = new PdfStandardFont(PdfFontFamily.Helvetica, 18);

                    PdfBrush brush = new PdfSolidBrush(SfDrawing.Color.Black);

                    byte[] imageBytes = new WebClient().DownloadData(rightMoveModel.PropertyMainPicture);
                    Stream imageStream = new MemoryStream(imageBytes);

                    pdfTemplate.Graphics.DrawString(rightMoveModel.PropertyAddress, pdfFont, brush, 5, 5);
                    pdfTemplate.Graphics.DrawString(rightMoveModel.PropertyType, pdfFont, brush, 5, 50);
                    pdfTemplate.Graphics.DrawString(rightMoveModel.PropertyPrice, pdfFont, brush, 5, 100);
                    pdfTemplate.Graphics.DrawImage
                        (PdfImage.FromStream(imageStream), new SfDrawing.PointF(26, 27), new SfDrawing.SizeF(300, 300));

                    pdfLoadedPage.Graphics.DrawPdfTemplate(pdfTemplate, SfDrawing.PointF.Empty);

                    string rawName = rightMoveModel
                    .PropertyUrl
                    .Replace("/", "")
                    .Replace("-", "")
                    .Replace(".", "")
                    .Replace(":", "")
                    .Replace("//", "");

                    string fileName = Regex.Match(rawName, @"(\d+(?:\.\d{1,2})?)").Value;

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

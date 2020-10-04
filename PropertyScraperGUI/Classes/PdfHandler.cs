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
using System.Threading.Tasks;
using System.Threading;

namespace PropertyScraperCSharpConsole.Classes
{
    public class PdfHandler
    {
        static HtmlToPdfConverter htmlConverter;
        static WebKitConverterSettings webKitSettings;

        string rightMoveArchiveFolder, quickSoldArchiveFolder, checkMyPostCodeArchiveFolder, qtBinariespath, templatePath, tempFolder;

        public PdfHandler()
        {
            rightMoveArchiveFolder = Path.Combine(Environment.CurrentDirectory, "Archives\\RightMove");
            qtBinariespath = Path.Combine(Environment.CurrentDirectory, "QtBinariesDotNetCore");
            templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources\\pdftemplate.pdf");
            tempFolder = Path.GetTempPath();

            htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            webKitSettings = new WebKitConverterSettings();

            webKitSettings.WebKitPath = qtBinariespath;
            webKitSettings.EnableForm = true;

            htmlConverter.ConverterSettings = webKitSettings;
        }

        public string SaveRightMovePdf(RightMoveModel rightMoveModel)
        {
            try
            {
                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(new FileStream(templatePath, FileMode.Open));

                if (!Directory.Exists(rightMoveArchiveFolder))
                {
                    Directory.CreateDirectory(rightMoveArchiveFolder);
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
                    pdfTemplate.Graphics.DrawImage(PdfImage.FromStream(imageStream), new SfDrawing.PointF(100, 100), new SfDrawing.SizeF(400, 400));

                    pdfLoadedPage.Graphics.DrawPdfTemplate(pdfTemplate, SfDrawing.PointF.Empty);

                    string rawName = rightMoveModel
                    .PropertyUrl
                    .Replace("/", "")
                    .Replace("-", "")
                    .Replace(".", "")
                    .Replace(":", "")
                    .Replace("//", "");

                    string fileName = Regex.Match(rawName, @"(\d+(?:\.\d{1,2})?)").Value;

                    PdfDocument propertyHeatMapPdfDocument = htmlConverter.Convert(rightMoveModel.PropertyHeatHtmlString, string.Empty);
                    PdfDocument homeCoUKHtmlPdfDocument = htmlConverter.Convert(rightMoveModel.HomeCoUKHtmlString, string.Empty);

                    string tempPropertyHeatMap = Path.Combine(tempFolder, $"propertyHeatMap{fileName}.pdf");

                    using (FileStream propertyHeatMapStream = new FileStream(tempPropertyHeatMap, FileMode.Create))
                    {
                        propertyHeatMapPdfDocument.Save(propertyHeatMapStream);
                        propertyHeatMapPdfDocument.Close(true);
                        propertyHeatMapPdfDocument.Dispose();

                        propertyHeatMapStream.Close();
                        propertyHeatMapStream.Dispose();
                    }

                    string tempHomeCoUK = Path.Combine(tempFolder, $"homeCoUK{fileName}.pdf");

                    using (FileStream homeCoUKHtmlPdfStream = new FileStream(tempHomeCoUK, FileMode.Create))
                    {
                        homeCoUKHtmlPdfDocument.Save(homeCoUKHtmlPdfStream);
                        homeCoUKHtmlPdfDocument.Close(true);
                        homeCoUKHtmlPdfDocument.Dispose();

                        homeCoUKHtmlPdfStream.Close();
                        homeCoUKHtmlPdfStream.Dispose();
                    }

                    using (FileStream propertyHeatMapReadStream = new FileStream(tempPropertyHeatMap, FileMode.Open))
                    {
                        PdfLoadedDocument tempPropertyHeatMapDocument = new PdfLoadedDocument(propertyHeatMapReadStream);
                        loadedDocument.ImportPage(tempPropertyHeatMapDocument, 0);

                        propertyHeatMapReadStream.Close();
                        propertyHeatMapReadStream.Dispose();
                    }

                    using (FileStream homeCoUKReadStream = new FileStream(tempHomeCoUK, FileMode.Open))
                    {
                        PdfLoadedDocument tempHomeCoUKDocument = new PdfLoadedDocument(homeCoUKReadStream);

                        loadedDocument.ImportPage(tempHomeCoUKDocument, 0);

                        homeCoUKReadStream.Close();
                        homeCoUKReadStream.Dispose();
                    }

                    string savePath = Path.Combine(rightMoveArchiveFolder, $"{fileName}.pdf");

                    using (FileStream saveStream = new FileStream(savePath, FileMode.Create))
                    {
                        loadedDocument.Save(saveStream);
                        loadedDocument.Close(true);
                        loadedDocument.Dispose();
                        saveStream.Close();
                        saveStream.Dispose();
                    }

                    return $"{savePath}";
                }
                else
                {
                    Console.WriteLine("Invalid PDF file");
                    return $"Invalid PDF file";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to save the file. {ex.Message}");

                return $"Unable to save the file";
            }
        }

        public string SavePDF(string htmlString, string fileName, bool isQuickSold)
        {
            FileStream fs = null;

            try
            {

                if (isQuickSold)
                {
                    if (!Directory.Exists(quickSoldArchiveFolder))
                    {
                        Directory.CreateDirectory(quickSoldArchiveFolder);
                    }

                    PdfDocument document = htmlConverter.Convert(htmlString, tempFolder);

                    string savePath = string.Empty;
                    savePath = Path.Combine(quickSoldArchiveFolder, $"{fileName}.pdf");

                    fs = new FileStream(savePath, FileMode.CreateNew);

                    document.Save(fs);
                    document.Close(true);
                    fs.Close();
                    fs.Dispose();

                    return $"file successfully saved at: {savePath}";
                }
                else
                {
                    if (!Directory.Exists(checkMyPostCodeArchiveFolder))
                    {
                        Directory.CreateDirectory(checkMyPostCodeArchiveFolder);
                    }

                    PdfDocument document = htmlConverter.Convert(htmlString, tempFolder);

                    string savePath = string.Empty;
                    savePath = Path.Combine(checkMyPostCodeArchiveFolder, $"{fileName}.pdf");

                    fs = new FileStream(savePath, FileMode.CreateNew);

                    document.Save(fs);
                    document.Close(true);
                    fs.Close();
                    fs.Dispose();

                    return $"{savePath}";
                }

                //PdfLoadedDocument loadedDocument;

                //if (File.Exists(Path.Combine(archiveFolder, $"{fileName}.pdf")))
                //{
                //    loadedDocument = new PdfLoadedDocument(new FileStream(Path.Combine(archiveFolder, $"{fileName}.pdf"), FileMode.Open));
                //}
                //else
                //{
                //    loadedDocument = new PdfLoadedDocument(new FileStream(templatePath, FileMode.Open));
                //}

                //if (loadedDocument.PageCount > 0)
                //{
                //PdfLoadedPage pdfLoadedPage = loadedDocument.Pages[1] as PdfLoadedPage;

                //PdfTemplate pdfTemplate = new PdfTemplate(900, 600);

                //PdfFont pdfFont = new PdfStandardFont(PdfFontFamily.Helvetica, 15);

                //PdfBrush brush = new PdfSolidBrush(SfDrawing.Color.Black);

                // pdfLoadedPage.Graphics.DrawPdfTemplate(pdfTemplate, SfDrawing.PointF.Empty);

                //PdfDocument document = htmlConverter.Convert(htmlString, tempFolder);

                ////fs = new FileStream(Path.Combine(tempFolder, $"{fileName}.pdf"), FileMode.Create);
                ////document.Save(fs);
                ////document.Close(true);
                ////fs.Close();
                ////fs.Dispose();

                //string savePath = string.Empty;
                //savePath = Path.Combine(rightMoveArchiveFolder, $"{fileName}.pdf");

                //fs = new FileStream(savePath, FileMode.CreateNew);

                //document.Save(fs);
                //document.Close(true);
                //fs.Close();
                //fs.Dispose();

                //PdfLoadedDocument tempLoadeDocument = new PdfLoadedDocument(fs);

                //loadedDocument.ImportPage(tempLoadeDocument, loadedDocument.PageCount - 1);
                //loadedDocument.Save(new FileStream(savePath, FileMode.Create));
                //loadedDocument.Close(true);

                //fs.Close();
                //fs.Dispose();

                //return $"file successfully saved at: {savePath}";
                //}
                //else
                //{
                //    Console.WriteLine("Invalid PDF file");
                //    return $"Invalid PDF file";
                //}
            }
            catch (Exception ex)
            {
                //fs.Close();
                //fs.Dispose();

                Console.WriteLine($"Unable to save the file. {ex.Message}");

                return $"Unable to save the file";
            }
        }
    }
}

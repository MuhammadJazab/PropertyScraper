using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System;
using System.IO;

namespace PropertyScraperCSharpConsole.Classes
{
    public class PdfHandler
    {
        static HtmlToPdfConverter htmlConverter;
        static WebKitConverterSettings webKitSettings;

        public static bool SavePdf(string url,string fileName)
        {
            try
            {
                htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
                webKitSettings = new WebKitConverterSettings();

                string path = Path.Combine(Environment.CurrentDirectory, "QtBinariesDotNetCore");

                webKitSettings.WebKitPath = path;
                webKitSettings.EnableForm = true;

                htmlConverter.ConverterSettings = webKitSettings;

                PdfDocument document = htmlConverter.Convert(url);

                string savePath = Path.Combine(Environment.CurrentDirectory, $"Archives\\{fileName}.pdf");

                FileStream fileStream = new FileStream(savePath, FileMode.Create);

                document.Save(fileStream);

                document.Close(true);

                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                return false;
            }
        }
    }
}

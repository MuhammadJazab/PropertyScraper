using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PropertyScraperCSharpConsole.Classes;
using PropertyScraperCSharpConsole.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace PropertyScraperCSharpConsole
{
    class Program
    {
        static string rightMoveUrl = "https://www.rightmove.co.uk";
        static string propertyHeatmapUrl = "https://www.propertyheatmap.uk";
        static string homeCoUKUrl = "https://www.home.co.uk/for_rent/";
        static string checkMyPostCodeUrl = "https://checkmypostcode.uk/";

        static string defaultPostalCode = "NW3", postalCode;

        static List<string> propertiesLinks = new List<string>();
        static List<RightMoveModel> rightMoveModels = new List<RightMoveModel>();

        static ChromeDriverService service;

        static IWebDriver driver;
        static HtmlWeb htmlWeb;

        static PdfHandler pdfHandler;

        static void Main(string[] args)
        {
            pdfHandler = new PdfHandler();
            
            service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            NavigationOutput($"Enter postal code. Leaving it empty will use default {defaultPostalCode} postal code");
            postalCode = Console.ReadLine();
            NavigationOutput("starting data scrapping...");

            //ScrapRightMove();

            //ScrapPropertyHeat();

            //ScrapHomeCoUk();

            //ScrapCheckMyPostCode();

            //ScrapQuickSold();
        }

        private static void ScrapRightMove()
        {
            driver = new ChromeDriver(service);
            htmlWeb = new HtmlWeb();

            NavigationOutput($"Navigating to: {rightMoveUrl}");

            driver.Url = rightMoveUrl;

            driver.FindElement(By.Name("searchLocation"))
                .SendKeys(string.IsNullOrEmpty(postalCode) ? defaultPostalCode : postalCode);
            driver.FindElement(By.Id("buy")).Click();

            NavigationOutput($"Navigating to: {driver.Url}");

            driver.FindElement(By.Id("submit")).Click();

            #region Get property list URLs

            ReadOnlyCollection<IWebElement> readOnlyCollection = driver
                .FindElements(By.CssSelector(".propertyCard-moreInfoItem.is-carousel"));

            NavigationOutput($"Finding URLs on: {driver.Url}");

            foreach (var item in readOnlyCollection)
            {
                propertiesLinks.Add(item.GetAttribute("href"));
                NavigationOutput($"URL found: {item.GetAttribute("href")}");
            }

            #endregion

            driver.Quit();

            #region CollectDataFromUrls

            if (propertiesLinks.Count > 0)
            {
                NavigationOutput($"Successfully found {propertiesLinks.Count} URLs on: {driver.Url}");

                foreach (var _item in propertiesLinks)
                {
                    if (!string.IsNullOrEmpty(_item))
                    {
                        NavigationOutput($"Fatching data from: {_item}");

                        HtmlDocument htmlDocument = htmlWeb.Load(_item);

                        string propertyType = htmlDocument
                            .DocumentNode
                            .SelectNodes("//h1[@class='fs-22']")[0]
                            .InnerHtml;

                        string propertyAddress = htmlDocument
                            .DocumentNode
                            .SelectNodes("//meta[@itemprop='streetAddress']")[0]
                            .GetAttributeValue("content", string.Empty);

                        string propertyPriceHtml = htmlDocument
                            .DocumentNode
                            .SelectNodes("//p[@id='propertyHeaderPrice']")[0].InnerText;

                        string rawPrice = propertyPriceHtml
                            .Replace("\r", "")
                            .Replace("\n", "")
                            .Replace("\t", "")
                            .Replace(";", "")
                            .Replace(",", "");
                        string propertyPrice = Regex.Match(rawPrice, @"(\d+(?:\.\d{1,2})?)").Value;

                        string propertyMainPicture = htmlDocument
                            .DocumentNode
                            .SelectNodes("//img[@class='js-gallery-main']")[0]
                            .GetAttributeValue("src", string.Empty);

                        rightMoveModels.Add(new RightMoveModel()
                        {
                            PropertyAddress = propertyAddress,
                            PropertyMainPicture = propertyMainPicture,
                            PropertyPrice = propertyPrice,
                            PropertyType = propertyType
                        });
                    }
                    else NavigationOutput($"Invalid Url: {_item}");
                }
            }
            else NavigationOutput($"No URLs found on: {driver.Url}");

            #endregion
        }


        private static void ScrapPropertyHeat()
        {
            var _postalCode = string.IsNullOrEmpty(postalCode) ? defaultPostalCode : postalCode;

            var html = new HttpClient().GetStringAsync($"{propertyHeatmapUrl}/reports/{_postalCode}");
            var htmlString = html.GetAwaiter().GetResult();

            //var pdf = Pdf.From(htmlString).Content();

            // save file to pdf
        }

        private static void ScrapHomeCoUk()
        {
            driver = new ChromeDriver(service);
            htmlWeb = new HtmlWeb();

            NavigationOutput($"Navigating to: {homeCoUKUrl}");

            driver.Url = homeCoUKUrl;

            driver.FindElement(By.CssSelector(".homeco_pr_textbox.input--medium.ui-autocomplete-input"))
                .SendKeys(string.IsNullOrEmpty(postalCode) ? defaultPostalCode : postalCode);

            driver.FindElement(By.ClassName("homeco_pr_button button")).Click();

            NavigationOutput($"Navigating to: {driver.Url}");

            driver.FindElement(By.ClassName("homeco_pr_button button")).Click();

            #region Get property list URLs

            #endregion

            driver.Quit();

            var aa = Console.ReadKey();
        }

        private static void ScrapCheckMyPostCode()
        {
            driver = new ChromeDriver(service);
            htmlWeb = new HtmlWeb();

            NavigationOutput($"Enter postal code. Leaving it empty will use default WA130QX postal code");
            string checkMypostalCode = Console.ReadLine();
            NavigationOutput("starting data scrapping...");

            NavigationOutput($"Navigating to: {checkMyPostCodeUrl}");

            var postalCodeCheck = string.IsNullOrWhiteSpace(checkMypostalCode) ? "WA130QX" : checkMypostalCode;

            driver.Url = $"{checkMyPostCodeUrl}{postalCodeCheck}" ;

            NavigationOutput($"Navigating to: {driver.Url}");



            #region Get property list URLs

            #endregion

            driver.Quit();
        }

        private static void ScrapQuickSold()
        {

        }

        private static void NavigationOutput(string _url)
        {
            Console.WriteLine($"{_url}" + Environment.NewLine);
        }
    }
}

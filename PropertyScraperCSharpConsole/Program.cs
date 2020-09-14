using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PropertyScraperCSharpConsole.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace PropertyScraperCSharpConsole
{
    class Program
    {
        static string rightMoveUrl = "https://www.rightmove.co.uk";
        static string defaultPostalCode = "NW3", postalCode;

        static List<string> propertiesLinks = new List<string>();
        static List<RightMoveModel> rightMoveModels = new List<RightMoveModel>();

        static IWebDriver driver;
        static HtmlWeb htmlWeb;

        static void Main(string[] args)
        {
            NavigationOutput($"Enter postal code. Leaving it empty will use default {defaultPostalCode} postal code");
            postalCode = Console.ReadLine();
            NavigationOutput("starting data scrapping...");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

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

            var aaaa = propertiesLinks;

            #region CollectDataFromUrls

            if (propertiesLinks.Count > 0)
            {
                NavigationOutput($"Successfully found {propertiesLinks.Count} URLs on: {driver.Url}");

                foreach (var item in propertiesLinks)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        FetchRequiredDataFromRightMoveUrl(item);
                    }
                    else NavigationOutput($"Invalid Url: {item}");
                }
            }
            else NavigationOutput($"No URLs found on: {driver.Url}");

            #endregion
        }

        private static void FetchRequiredDataFromRightMoveUrl(string _item)
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
                .GetAttributeValue("content",string.Empty);

            string propertyPriceHtml = htmlDocument
                .DocumentNode
                .SelectNodes("//p[@id='propertyHeaderPrice']")[0].InnerText;
            string rawPrice = propertyPriceHtml.Replace("\r","").Replace("\n","").Replace("\t","").Replace(";","").Replace(",","");
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

        private static void NavigationOutput(string _url)
        {
            Console.WriteLine($"{_url}" + Environment.NewLine);
        }
    }
}

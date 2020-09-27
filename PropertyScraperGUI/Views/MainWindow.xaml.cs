using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PropertyScraperCSharpConsole.Classes;
using PropertyScraperCSharpConsole.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace PropertyScraperGUI
{
    public partial class MainWindow : Window
    {
        string rightMoveUrl = "https://www.rightmove.co.uk";
        string propertyHeatmapUrl = "https://www.propertyheatmap.uk";
        string checkMyPostCodeUrl = "https://checkmypostcode.uk";
        string quicksoldUrl = "https://quicksold.co.uk";

        string defaultPostalCode = "NW3", postalCode;

        List<string> propertiesLinks = new List<string>();

        ChromeDriverService service;

        IWebDriver driver;
        HtmlWeb htmlWeb;

        string tempFolder = string.Empty;

        PdfHandler pdfHandler;

        public MainWindow()
        {
            tempFolder = Path.GetTempPath();

            pdfHandler = new PdfHandler();

            service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            InitializeComponent();
        }

        private void RightMove_Click(object sender, RoutedEventArgs e)
        {
            postalCode = Text_PostalCode.Text;
            ScrapRightMove();
        }

        private void QuickSold_Click(object sender, RoutedEventArgs e)
        {
            ScrapQuickSold();
        }

        private void CheckMyPostCode_Click(object sender, RoutedEventArgs e)
        {
            ScrapCheckMyPostCode();
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        public void ScrapRightMove()
        {
            try
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

                if (propertiesLinks.Count > 0)
                {
                    NavigationOutput($"Successfully found {propertiesLinks.Count} URLs on: {driver.Url}");
                    driver.Quit();

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

                            string propertyHeatHtmlString = ScrapPropertyHeat();
                            string innerHtml = ScrapHomeCoUk();

                            RightMoveModel rightMoveModel = new RightMoveModel()
                            {
                                PropertyAddress = propertyAddress,
                                PropertyMainPicture = propertyMainPicture,
                                PropertyPrice = propertyPrice,
                                PropertyType = propertyType,
                                PropertyUrl = _item,
                                PropertyHeatHtmlString = propertyHeatHtmlString,
                                postalCode = string.IsNullOrEmpty(postalCode) ? defaultPostalCode : postalCode,
                                HomeCoUKHtmlString = innerHtml
                            };

                            pdfHandler.SaveRightMovePdf(rightMoveModel);
                        }
                        else NavigationOutput($"Invalid Url: {_item}");
                    }
                }
                else NavigationOutput($"No URLs found on: {driver.Url}");

                #endregion
            }
            catch (Exception ex)
            {
                NavigationOutput(ex.Message);
                return;
            }
        }

        public string ScrapPropertyHeat()
        {
            var _postalCode = string.IsNullOrEmpty(postalCode) ? defaultPostalCode : postalCode;

            string url = $"{propertyHeatmapUrl}/reports/{_postalCode}";

            HtmlDocument htmlDocument = htmlWeb.Load(url);

            string htmlString = htmlDocument.DocumentNode.SelectNodes("//main")[0].InnerHtml;

            return htmlString;
        }

        public string ScrapHomeCoUk()
        {
            driver = new ChromeDriver(service);
            htmlWeb = new HtmlWeb();

            string tempPostalCode = string.IsNullOrEmpty(postalCode) ? defaultPostalCode : postalCode;
            string url = $"https://www.home.co.uk/for_rent/{tempPostalCode}/current_rents?location={tempPostalCode}";

            NavigationOutput($"Navigating to: {url}");

            driver.Url = url;

            string innerHtml = driver.FindElement(By.CssSelector(".homeco_content_main.homeco_content_min_height")).GetAttribute("innerHTML");

            driver.Quit();

            return innerHtml;

            //driver.FindElement(By.CssSelector(".homeco_pr_textbox.input--medium.ui-autocomplete-input"))
            //    .SendKeys(string.IsNullOrEmpty(postalCode) ? defaultPostalCode : postalCode);

            //driver.FindElement(By.CssSelector(".homeco_pr_button.button")).Click();

            //NavigationOutput($"Navigating to: {driver.Url}");

            //driver.FindElement(By.XPath("//input[@value='Search']")).Click();

            //ReadOnlyCollection<IWebElement> readOnlyCollection = driver.FindElements(By.ClassName("homeco_prop_link"));

            //NavigationOutput($"Finding URLs on: {driver.Url}");

            //foreach (var item in readOnlyCollection)
            //{
            //    //propertiesLinks.Add(item.GetAttribute("href"));
            //    NavigationOutput($"URL found: {item.GetAttribute("href")}");
            //}
        }

        public void ScrapCheckMyPostCode()
        {
            NavigationOutput($"Navigating to: {checkMyPostCodeUrl}");

            HtmlDocument countriesLinkDocument = new HtmlWeb().Load($"{checkMyPostCodeUrl}/counties");

            foreach (HtmlNode countryLink in countriesLinkDocument.DocumentNode.SelectNodes("//div[contains(@class,'medium-12') and contains(@class,'columns')]//ul//li//a"))
            {
                HtmlAttribute rawCountryLink = countryLink.Attributes["href"];
                string countryUrl = $"{checkMyPostCodeUrl}{rawCountryLink.Value}";

                NavigationOutput($"Navigating to: {countryUrl}");

                HtmlDocument locailitiesInCityDocument = new HtmlWeb().Load(countryUrl);

                foreach (HtmlNode postalCodeInCity in locailitiesInCityDocument.DocumentNode.SelectNodes("//div[@class='threecol']//ul//li//a"))
                {
                    HtmlAttribute rawPostalCodeInCity = postalCodeInCity.Attributes["href"];
                    string postalCodeUrl = $"{checkMyPostCodeUrl}{rawPostalCodeInCity.Value}";

                    NavigationOutput($"Navigating to: {postalCodeUrl}");

                    HtmlDocument postCodesInCityDocument = new HtmlWeb().Load(postalCodeUrl);

                    foreach (var postCodesInCity in postCodesInCityDocument.DocumentNode.SelectNodes("//div[contains(@class,'medium-12') and contains(@class,'columns')]//ul//li//a"))
                    {
                        HtmlAttribute rawPostCodesInCity = postCodesInCity.Attributes["href"];
                        string postCodesInCityUrl = $"{checkMyPostCodeUrl}{rawPostCodesInCity.Value}";

                        NavigationOutput($"Navigating to: {postCodesInCityUrl}");

                        HtmlDocument postCodePageDocument = new HtmlWeb().Load(postCodesInCityUrl);

                        NavigationOutput($"Saving file to PDF");

                        string checkMyPostHtml = postCodePageDocument.DocumentNode.SelectNodes("//span")[1].InnerHtml;

                        Console.WriteLine(pdfHandler.SavePDF(checkMyPostHtml, $"CheckMyPostCode {rawPostCodesInCity.Value.Replace("/", "")}"));
                    }
                }
            }
        }

        public void ScrapQuickSold()
        {
            NavigationOutput($"Navigating to: {quicksoldUrl}/area-information");

            HtmlDocument postalCodeDocument = new HtmlWeb().Load($"{quicksoldUrl}/area-information");

            foreach (HtmlNode postalCodes in postalCodeDocument.DocumentNode.SelectNodes("//table[@class='table']//tr//td//a"))
            {

                HtmlAttribute rawPostalCodelink = postalCodes.Attributes["href"];
                string postalCodeLink = $"{quicksoldUrl}{rawPostalCodelink.Value}";

                NavigationOutput($"Navigating to: {postalCodeLink}");

                HtmlDocument postalAreaDocument = new HtmlWeb().Load(postalCodeLink);

                foreach (HtmlNode postalCodeAreas in postalAreaDocument.DocumentNode.SelectNodes("//div[@class='col-xs-2']//a"))
                {
                    HtmlAttribute rawPostalCodeArea = postalCodeAreas.Attributes["href"];
                    string postalCodeAreaLink = $"{quicksoldUrl}{rawPostalCodeArea.Value}";

                    NavigationOutput($"Navigating to: {postalCodeAreaLink}");

                    HtmlDocument postalCodeDistrictDocument = new HtmlWeb().Load(postalCodeAreaLink);

                    foreach (HtmlNode postalCodeDistrics in postalCodeDistrictDocument.DocumentNode.SelectNodes("//table[@class='table']//tr//td//a"))
                    {
                        HtmlAttribute rawPostalCodeDistrics = postalCodeDistrics.Attributes["href"];
                        string postalCodeDistricsLink = $"{quicksoldUrl}{rawPostalCodeDistrics.Value}";

                        NavigationOutput($"Navigating to: {postalCodeDistricsLink}");

                        HtmlDocument areaInformationPageDocument = new HtmlWeb().Load(postalCodeDistricsLink);

                        string areaInformationHtml = areaInformationPageDocument
                            .DocumentNode
                            .SelectNodes("//div[@class='row']")[0].InnerHtml;

                        NavigationOutput($"Saving file to PDF");

                        string fileSaved = pdfHandler.SavePDF(areaInformationHtml, "QuickSold");

                        NavigationOutput($"{fileSaved}");
                    }
                }
            }
        }

        public void NavigationOutput(string _url)
        {
            RichText_Outputs.AppendText($"{_url}" + Environment.NewLine);
        }
    }
}

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
using System.Windows.Controls;
using System.Windows.Media;

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
            postalCode = string.IsNullOrEmpty(Text_PostalCode.Text) ? Text_PostalCode.Text : defaultPostalCode;
            DisableControls();
            ScrapRightMove();
            EnableControls();
        }

        private void QuickSold_Click(object sender, RoutedEventArgs e)
        {
            DisableControls();
            ScrapQuickSold();
            EnableControls();
        }

        private void CheckMyPostCode_Click(object sender, RoutedEventArgs e)
        {
            DisableControls();
            ScrapCheckMyPostCode();
            EnableControls();
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
                    .SendKeys(postalCode);
                driver.FindElement(By.Id("buy")).Click();

                NavigationOutput($"Navigating to: {driver.Url}");

                MainProgressBar.Value = 2;

                driver.FindElement(By.Id("submit")).Click();

                #region Get property list URLs

                ReadOnlyCollection<IWebElement> readOnlyCollection;

                readOnlyCollection = driver.FindElements(By.CssSelector(".propertyCard-moreInfoItem.is-carousel"));

                if (readOnlyCollection.Count <= 0)
                {
                    readOnlyCollection = driver.FindElements(By.CssSelector(".propertyCard-link"));
                }

                NavigationOutput($"Finding URLs on: {driver.Url}");

                MainProgressBar.Value = 10;

                foreach (var item in readOnlyCollection)
                {
                    propertiesLinks.Add(item.GetAttribute("href"));
                }

                ProgressBar.Value = 90;

                if (propertiesLinks.Count > 0)
                {
                    NavigationOutput($"Successfully found {propertiesLinks.Count} URLs on: {driver.Url}");
                    driver.Quit();

                    var progresscount = propertiesLinks.Count / 100;

                    MainProgressBar.Value = 15;

                    foreach (var _item in propertiesLinks)
                    {
                        ProgressBar.Value = 0;

                        if (!string.IsNullOrEmpty(_item))
                        {
                            NavigationOutput($"Fatching data from: {_item}");

                            ProgressBar.Value += progresscount;
                            MainProgressBar.Value += progresscount / 2;

                            Text_Outputs.Content = $"Fatching data from: {_item}";

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
                                postalCode = postalCode,
                                HomeCoUKHtmlString = innerHtml
                            };

                            Text_Outputs.Content = $"Saving file to pdf";
                            string rVal = pdfHandler.SaveRightMovePdf(rightMoveModel);
                            Text_Outputs.Content = rVal;
                        }
                        else NavigationOutput($"Invalid Url: {_item}");
                    }
                    MainProgressBar.Value = 100;
                }
                else
                {
                    NavigationOutput($"No URLs found on: {driver.Url}");
                    driver.Quit();
                }
                #endregion
            }
            catch (Exception ex)
            {
                NavigationOutput(ex.Message);
                driver.Quit();
                return;
            }
        }

        public string ScrapPropertyHeat()
        {
            string url = $"{propertyHeatmapUrl}/reports/{postalCode}";

            HtmlDocument htmlDocument = htmlWeb.Load(url);

            string htmlString = htmlDocument.DocumentNode.SelectNodes("//main")[0].InnerHtml;

            return htmlString;
        }

        public string ScrapHomeCoUk()
        {
            string tempPostalCode, url, innerHtml;

            driver = new ChromeDriver(service);
            htmlWeb = new HtmlWeb();

            tempPostalCode = postalCode.Contains(" ") ? postalCode.Split(" ")[0] : defaultPostalCode;

            url = $"https://www.home.co.uk/for_rent/{tempPostalCode}/current_rents?location={tempPostalCode}";

            NavigationOutput($"Navigating to: {url}");

            driver.Url = url;

            innerHtml = driver.FindElement(By.CssSelector(".homeco_content_main.homeco_content_min_height")).GetAttribute("innerHTML");

            driver.Quit();

            return innerHtml;
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

                        Console.WriteLine(pdfHandler.SavePDF(checkMyPostHtml,
                            $"CheckMyPostCode {rawPostCodesInCity.Value.Replace("/", "")}", false));
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

                    foreach (HtmlNode postalCodeDistrics in postalCodeDistrictDocument
                        .DocumentNode
                        .SelectNodes("//table[@class='table']//tr//td//a"))
                    {
                        HtmlAttribute rawPostalCodeDistrics = postalCodeDistrics.Attributes["href"];
                        string postalCodeDistricsLink = $"{quicksoldUrl}{rawPostalCodeDistrics.Value}";

                        NavigationOutput($"Navigating to: {postalCodeDistricsLink}");

                        HtmlDocument areaInformationPageDocument = new HtmlWeb().Load(postalCodeDistricsLink);

                        string areaInformationHtml = areaInformationPageDocument
                            .DocumentNode
                            .SelectNodes("//div[@class='row']")[0].InnerHtml;

                        NavigationOutput($"Saving file to PDF");

                        string fileSaved = pdfHandler.SavePDF(areaInformationHtml, "QuickSold", true);

                        NavigationOutput($"{fileSaved}");
                    }
                }
            }
        }

        public void NavigationOutput(string _url)
        {
            MainText_Outputs.Content = $"{_url}";
        }

        public void DisableControls()
        {
            foreach (TextBox tb in FindVisualChildren<TextBox>(MainContainer))
            {
                tb.IsEnabled = false;
            }

            foreach (Button btn in FindVisualChildren<Button>(MainContainer))
            {
                btn.IsEnabled = false;
            }
        }

        public void EnableControls()
        {
            foreach (TextBox tb in FindVisualChildren<TextBox>(MainContainer))
            {
                tb.IsEnabled = true;
            }

            foreach (Button btn in FindVisualChildren<Button>(MainContainer))
            {
                btn.IsEnabled = true;
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}

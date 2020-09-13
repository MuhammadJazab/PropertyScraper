using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PropertyScraper
{
    class Program
    {
        //static string _baseUrl = "www.rightmove.co.uk/";
        //static string _pageUrl = @"property-for-sale/find.html?keywords=&sortType=2&viewType=LIST&channel=BUY&index=0&radius=0.0&locationIdentifier=USERDEFINEDAREA%5E%7B"polylines"%3A"enfxHmka%40mTfxrC%7DapAl%7DAoi%40efdDzarAnnM"%7D";
        static string _url = string.Empty;
        static HttpClient _httpClient = new HttpClient();
        static HtmlDocument _htmlDocument = new HtmlDocument();

        static void Main(string[] args)
        {
            Console.WriteLine("Enter Url");
            _url = Console.ReadLine();

            try
            {
                _httpClient.DefaultRequestHeaders
                        .Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Console.WriteLine($"Searching started on {_url}");

                StartCrawlingAsync().Wait();

                Console.WriteLine("Searching completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task StartCrawlingAsync()
        {
            try
            {
                string htmlString = await _httpClient.GetStringAsync($"{_url}");
                _htmlDocument.LoadHtml(htmlString);

                var parentDome = _htmlDocument.DocumentNode
                    .Descendants("td")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("titleColumn"))
                    .ToList();

                List<string> hrefLinks = new List<string>();

                foreach (var dome in parentDome)
                {
                    string element = dome.Descendants("a").FirstOrDefault().ChildAttributes("href").FirstOrDefault().Value;
                    hrefLinks.Add(element);
                }

                var aa = hrefLinks;

                //await CollectDataFromUrls(hrefLinks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //private static async Task CollectDataFromUrls(List<string> hrefLinks)
        //{
        //    List<DataCollection> dataCollections = new List<DataCollection>();

        //    foreach (var url in hrefLinks)
        //    {
        //        string movieUrl = $"{_baseUrl}{url}";
        //        _htmlDocument.LoadHtml(await _httpClient.GetStringAsync(movieUrl));

        //        var pageContent = _htmlDocument.DocumentNode
        //            .Descendants("div")
        //            .Where(node => node.GetAttributeValue("class", "")
        //            .Equals("pagecontent"))
        //            .ToList();

        //        foreach (var contect in pageContent)
        //        {
        //            var aa = contect.Descendants("div").Where(nn => nn.GetAttributeValue("class", "")
        //            .Equals("see-more inline canwrap"));

        //            var aaa = contect.Descendants("a").FirstOrDefault().ChildAttributes("href").FirstOrDefault().Value;

        //            //DataCollection dataCollection = new DataCollection()
        //            //{
        //            //    RatingScore = contect
        //            //    .Descendants("span")
        //            //    .Where(n => n.GetAttributeValue("itemprop", "")
        //            //    .Equals("ratingValue"))
        //            //    .FirstOrDefault()
        //            //    .InnerHtml,

        //            //    TotalRating = contect
        //            //    .Descendants("span")
        //            //    .Where(n => n.GetAttributeValue("itemprop", "")
        //            //    .Equals("bestRating"))
        //            //    .FirstOrDefault()
        //            //    .InnerHtml,

        //            //    Genre = contect
        //            //    .Descendants("div")
        //            //    .Where(n => n.GetAttributeValue("itemprop", "")
        //            //    .Equals("bestRating"))
        //            //    .FirstOrDefault()
        //            //    .InnerHtml,
        //            //};
        //        }

        //        //dome.Descendants("a").FirstOrDefault().ChildAttributes("href").FirstOrDefault().Value;

        //        Console.WriteLine(pageContent);
        //    }
        //}
    }
}
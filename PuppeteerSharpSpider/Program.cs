using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PuppeteerSharp;

namespace PuppeteerSharpSpider
{
    class Program
    {
        private const string Url = "http://www.877jn.com/ed2-7.html";
        private const int ChromiumRevision = BrowserFetcher.DefaultRevision;

        private static async Task Main(string[] args)
        {
            //Download chromium browser revision package
            //756066
            await new BrowserFetcher().DownloadAsync(ChromiumRevision);

            //Test AngleSharp
            await TestAngleSharp();

            Console.ReadKey();
        }

        private static async Task TestAngleSharp()
        {
            /*
             * Used AngleSharp loading of HTML document
             * TODO: Used WithJavaScript function need install AngleSharp.Scripting.Javascript nuget package
             * Note: that JavaScripts support is an experimental and does not support complex JavaScripts code.
             */
            //IConfiguration config = Configuration.Default.WithDefaultLoader();
            //IBrowsingContext context = BrowsingContext.New(config);
            //IDocument document = await context.OpenAsync(Url);

            //Used PuppeteerSharp loading of HTML document
            var htmlString = await TestPuppeteerSharp();

            /*
             * Parsing of HTML document string
             */

            if (File.Exists("123.txt"))
                File.Delete("123.txt");

            using (StreamWriter streamWriter = new StreamWriter("123.txt"))
            {
                streamWriter.Write(htmlString);
                streamWriter.Flush();
            }


            //var context = BrowsingContext.New(Configuration.Default);
            //var parser = context.GetService<IHtmlParser>();
            //var document = parser.ParseDocument(htmlString);

            ////Selector carbox element list
            //var carboxList = document.QuerySelectorAll("div.shop-content div.content div.list li.carbox");

            //var carModelList = new List<CarModel>();
            //foreach (var carbox in carboxList)
            //{
            //    //Parsing and converting to the car model object.
            //    var model = CreateModelWithAngleSharp(carbox);
            //    carModelList.Add(model);

            //    //Printing to console windows
            //    var jsonString = JsonConvert.SerializeObject(model);
            //    Console.WriteLine(jsonString);
            //    Console.WriteLine();
            //}

            //Console.WriteLine("Total count:" + carModelList.Count);
        }

        private static async Task<string> TestPuppeteerSharp()
        {
            //Enabled headless option
            var launchOptions = new LaunchOptions { Headless = true };
            //Starting headless browser
            var browser = await Puppeteer.LaunchAsync(launchOptions);

            //Get all(default) pages 
            var pages = await browser.PagesAsync();
            //Get first page or new tab page
            var firstPage = pages.Length > 0 ? pages[0] : await browser.NewPageAsync();
            //Request URL to get the page
            await firstPage.GoToAsync(Url);

            //Get and return the HTML content of the page
            var htmlString = await firstPage.GetContentAsync();

            #region Dispose resources
            //Close tab page
            await firstPage.CloseAsync();

            //Close headless browser, all pages will be closed here.
            await browser.CloseAsync();
            #endregion

            return htmlString;
        }
    }
}

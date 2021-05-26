using AngleSharp;
using System;

namespace MainSpider
{
    class Program
    {
        public static async void Main(string[] args)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var address = "https://www.cnblogs.com";
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(address);




            Console.WriteLine("Hello World!");
        }
    }
}

namespace Larpx.ResourceSpider.DotnetSpiderEx.Downloader
{
    public static class Downloaders
    {
        /// <summary>
        /// 纯http下载器
        /// </summary>
        public const string HttpClient = "DotnetSpiderEx_HttpClient_Downloader";

        /// <summary>
        /// 代理Http下载器
        /// </summary>
        public const string ProxyHttpClient = "DotnetSpiderEx_Proxy_HttpClient_Downloader";

        /// <summary>
        /// Puppeteer下载器
        /// </summary>
        public const string Puppeteer = "DotnetSpiderEx_Puppeteer_Downloader";

        /// <summary>
        /// 文件下载器ID
        /// </summary>
        public const string File = "DotnetSpiderEx_File_Downloader";
    }
}

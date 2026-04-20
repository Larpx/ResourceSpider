namespace ResourceSpider.Core.Exceptions;

public class DownloadException : SpiderException
{
    public string? Url { get; set; }

    public DownloadException(string message, string? url = null) 
        : base(message)
    {
        Url = url;
    }

    public DownloadException(string message, string? url, Exception innerException) 
        : base(message, innerException)
    {
        Url = url;
    }
}

namespace ResourceSpider.Core.Exceptions;

public class SpiderException : Exception
{
    public SpiderException(string message) : base(message)
    {
    }

    public SpiderException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

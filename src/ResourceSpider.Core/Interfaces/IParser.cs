using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public interface IParser
{
    IEnumerable<DataRecord> Parse(Response response);
}

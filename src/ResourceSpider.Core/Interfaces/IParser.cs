using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public interface IParser : IDataFlow
{
    IEnumerable<DataRecord> Parse(Response response);
}

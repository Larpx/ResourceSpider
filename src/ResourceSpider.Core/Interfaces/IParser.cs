using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

/// <summary>
/// 解析器接口，定义从响应内容中提取结构化数据的通用契约
/// </summary>
public interface IParser
{
    /// <summary>
    /// 解析响应内容，提取结构化数据记录
    /// </summary>
    /// <param name="response">待解析的响应对象</param>
    /// <returns>提取的数据记录集合</returns>
    IEnumerable<DataRecord> Parse(Response response);
}

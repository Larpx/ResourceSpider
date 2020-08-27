using Larpx.ResourceSpider.DataFlow.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Larpx.ResourceSpider.DataFlow.Parser
{
    /// <summary>
    /// 实体解析器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataParser<T> : DataParser where T : EntityBase<T>, new()
    {
        protected override Task Parse(DataContext context)
        {
            throw new NotImplementedException();
        }
    }
}

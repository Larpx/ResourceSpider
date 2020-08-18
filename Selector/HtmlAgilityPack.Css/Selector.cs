using System.Collections.Generic;

namespace Larpx.ResourceSpider.Selector.HtmlAgilityPack.Css
{
    /// <summary>
    /// 表示任意类型元素上的选择器实现。
    /// </summary>
    public delegate IEnumerable<TElement> Selector<TElement>(IEnumerable<TElement> elements);
}

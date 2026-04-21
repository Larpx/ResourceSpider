using System.Collections.Generic;

namespace ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

public delegate IEnumerable<TElement> Selector<TElement>(IEnumerable<TElement> elements);

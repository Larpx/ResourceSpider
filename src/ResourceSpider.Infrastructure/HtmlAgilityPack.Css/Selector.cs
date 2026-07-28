using System.Collections.Generic;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

public delegate IEnumerable<TElement> Selector<TElement>(IEnumerable<TElement> elements);

namespace ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

public interface IElementOps<TElement>
{
    Selector<TElement> Type(NamespacePrefix prefix, string name);
    Selector<TElement> Universal(NamespacePrefix prefix);
    Selector<TElement> Id(string id);
    Selector<TElement> Class(string clazz);
    Selector<TElement> AttributeExists(NamespacePrefix prefix, string name);
    Selector<TElement> AttributeExact(NamespacePrefix prefix, string name, string value);
    Selector<TElement> AttributeNotEqual(NamespacePrefix prefix, string name, string value);
    Selector<TElement> AttributeIncludes(NamespacePrefix prefix, string name, string value);
    Selector<TElement> AttributeRegexMatch(NamespacePrefix prefix, string name, string value);
    Selector<TElement> AttributeDashMatch(NamespacePrefix prefix, string name, string value);
    Selector<TElement> AttributePrefixMatch(NamespacePrefix prefix, string name, string value);
    Selector<TElement> AttributeSuffixMatch(NamespacePrefix prefix, string name, string value);
    Selector<TElement> AttributeSubstring(NamespacePrefix prefix, string name, string value);
    Selector<TElement> FirstChild();
    Selector<TElement> LastChild();
    Selector<TElement> NthChild(int a, int b);
    Selector<TElement> OnlyChild();
    Selector<TElement> Empty();
    Selector<TElement> Child();
    Selector<TElement> Descendant();
    Selector<TElement> Adjacent();
    Selector<TElement> GeneralSibling();
    Selector<TElement> NthLastChild(int a, int b);
    Selector<TElement> Eq(int n);
    Selector<TElement> Has(ISelectorGenerator subgenerator);
    Selector<TElement> SplitAfter(ISelectorGenerator subgenerator);
    Selector<TElement> SplitBefore(ISelectorGenerator subgenerator);
    Selector<TElement> SplitBetween(ISelectorGenerator subgenerator);
    Selector<TElement> SplitAll(ISelectorGenerator subgenerator);
    Selector<TElement> Before(ISelectorGenerator subgenerator);
    Selector<TElement> After(ISelectorGenerator subgenerator);
    Selector<TElement> Between(ISelectorGenerator startGenerator, ISelectorGenerator endGenerator);
    Selector<TElement> Not(ISelectorGenerator subgenerator);
    Selector<TElement> SelectParent();
    Selector<TElement> Contains(string text);
    Selector<TElement> Matches(string regex);
    Selector<TElement> Last();
}

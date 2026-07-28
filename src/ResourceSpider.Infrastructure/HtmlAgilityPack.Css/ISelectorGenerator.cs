namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

public interface ISelectorGenerator
{
    void OnInit();
    void OnClose();
    void OnSelector();
    void Type(NamespacePrefix prefix, string name);
    void Universal(NamespacePrefix prefix);
    void Id(string id);
    void Class(string clazz);
    void AttributeExists(NamespacePrefix prefix, string name);
    void AttributeExact(NamespacePrefix prefix, string name, string value);
    void AttributeNotEqual(NamespacePrefix prefix, string name, string value);
    void AttributeIncludes(NamespacePrefix prefix, string name, string value);
    void AttributeRegexMatch(NamespacePrefix prefix, string name, string value);
    void AttributeDashMatch(NamespacePrefix prefix, string name, string value);
    void AttributePrefixMatch(NamespacePrefix prefix, string name, string value);
    void AttributeSuffixMatch(NamespacePrefix prefix, string name, string value);
    void AttributeSubstring(NamespacePrefix prefix, string name, string value);
    void FirstChild();
    void LastChild();
    void NthChild(int a, int b);
    void OnlyChild();
    void Empty();
    void Eq(int n);
    void Has(ISelectorGenerator generator);
    void SplitAfter(ISelectorGenerator subgenerator);
    void SplitBefore(ISelectorGenerator subgenerator);
    void SplitBetween(ISelectorGenerator subgenerator);
    void SplitAll(ISelectorGenerator subgenerator);
    void Before(ISelectorGenerator subgenerator);
    void After(ISelectorGenerator subgenerator);
    void Between(ISelectorGenerator startGenerator, ISelectorGenerator endGenerator);
    void Not(ISelectorGenerator generator);
    void SelectParent();
    void Contains(string text);
    void Matches(string regex);
    void CustomSelector(object selector);
    void Child();
    void Descendant();
    void Adjacent();
    void GeneralSibling();
    void NthLastChild(int a, int b);
    ISelectorGenerator CreateNew();
    void AnchorToRoot();
    void Last();
    object Selector { get; }
}

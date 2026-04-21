using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

using TokenSpec = Either<TokenKind, Token>;

public sealed class CssParser
{
    private readonly Reader<Token> _reader;
    private readonly ISelectorGenerator _generator;
    private readonly bool _expectEoi;

    private CssParser(Reader<Token> reader, ISelectorGenerator generator) : this(reader, generator, true) { }
    private CssParser(Reader<Token> reader, ISelectorGenerator generator, bool expectEoi) { _reader = reader; _generator = generator; _expectEoi = expectEoi; }

    public static TGenerator Parse<TGenerator>(string selectors, TGenerator generator) where TGenerator : ISelectorGenerator
        => Parse(selectors, generator, g => g);

    public static T Parse<TGenerator, T>(string selectors, TGenerator generator, Func<TGenerator, T> resultor) where TGenerator : ISelectorGenerator
    {
        if (selectors == null) throw new ArgumentNullException("selectors");
        if (selectors.Length == 0) throw new ArgumentException(null, "selectors");
        return Parse(Tokener.Tokenize(selectors), generator, resultor);
    }

    public static TGenerator Parse<TGenerator>(IEnumerable<Token> tokens, TGenerator generator) where TGenerator : ISelectorGenerator
        => Parse(tokens, generator, g => g);

    public static T Parse<TGenerator, T>(IEnumerable<Token> tokens, TGenerator generator, Func<TGenerator, T> resultor) where TGenerator : ISelectorGenerator
    {
        if (tokens == null) throw new ArgumentNullException("tokens");
        if (resultor == null) throw new ArgumentNullException("resultor");
        new CssParser(new Reader<Token>(tokens.GetEnumerator()), generator).ParseInternal();
        return resultor(generator);
    }

    private void ParseInternal()
    {
        _generator.OnInit();
        if (TryRead(ToTokenSpec(Token.Slash())) != null) { _generator.AnchorToRoot(); TryRead(ToTokenSpec(TokenKind.WhiteSpace)); }
        SelectorGroup();
        _generator.OnClose();
    }

    private void SelectorGroup()
    {
        Selector();
        while (TryRead(ToTokenSpec(Token.Comma())) != null) { TryRead(ToTokenSpec(TokenKind.WhiteSpace)); Selector(); }
        if (_expectEoi) Read(ToTokenSpec(TokenKind.Eoi));
    }

    private void Selector()
    {
        _generator.OnSelector();
        SimpleSelectorSequence();
        while (TryCombinator()) SimpleSelectorSequence();
    }

    private bool TryCombinator()
    {
        var token = TryRead(ToTokenSpec(TokenKind.Plus), ToTokenSpec(TokenKind.Greater), ToTokenSpec(TokenKind.Tilde), ToTokenSpec(TokenKind.WhiteSpace));
        if (token == null) return false;
        if (token.Value.Kind == TokenKind.WhiteSpace) _generator.Descendant();
        else
        {
            switch (token.Value.Kind)
            {
                case TokenKind.Tilde: _generator.GeneralSibling(); break;
                case TokenKind.Greater: _generator.Child(); break;
                case TokenKind.Plus: _generator.Adjacent(); break;
            }
            TryRead(ToTokenSpec(TokenKind.WhiteSpace));
        }
        return true;
    }

    private void SimpleSelectorSequence()
    {
        var named = false;
        for (var modifiers = 0; ; modifiers++)
        {
            var token = TryRead(ToTokenSpec(TokenKind.Hash), ToTokenSpec(Token.Dot()), ToTokenSpec(Token.LeftBracket()), ToTokenSpec(Token.Colon()));
            if (token == null)
            {
                if (named || modifiers > 0) break;
                TypeSelectorOrUniversal();
                named = true;
            }
            else
            {
                if (modifiers == 0 && !named) _generator.Universal(NamespacePrefix.None);
                if (token.Value.Kind == TokenKind.Hash) _generator.Id(token.Value.Text);
                else
                {
                    Unread(token.Value);
                    switch (token.Value.Text[0])
                    {
                        case '.': Class(); break;
                        case '[': Attrib(); break;
                        case ':': Pseudo(); break;
                        default: throw new Exception("Internal error.");
                    }
                }
            }
        }
    }

    private void Pseudo() => PseudoClass();

    private void PseudoClass()
    {
        Read(ToTokenSpec(Token.Colon()));
        if (!TryFunctionalPseudo())
        {
            var clazz = Read(ToTokenSpec(TokenKind.Ident)).Text;
            switch (clazz)
            {
                case "first-child": _generator.FirstChild(); break;
                case "last-child": _generator.LastChild(); break;
                case "only-child": _generator.OnlyChild(); break;
                case "empty": _generator.Empty(); break;
                case "last": _generator.Last(); break;
                case "select-parent": _generator.SelectParent(); break;
                default: CustomSelector(clazz, false); break;
            }
        }
    }

    private void CustomSelector(string name, bool hasArguments)
    {
        if (!CustomSelectors.TryGetValue(name, out var deleg))
            throw new FormatException(string.Format("Unknown pseudo-selector '{0}'.", name));
        var formalParameters = deleg.ParameterTypes;
        var actualParameters = new object[formalParameters.Count];
        for (var i = 0; i < formalParameters.Count; i++)
        {
            if (i != 0) { Read(ToTokenSpec(Token.Semicolon())); Read(ToTokenSpec(TokenKind.WhiteSpace)); }
            var type = formalParameters[i];
            var typeName = type.Name;
            if (typeName == TypeCode.String.ToString()) actualParameters[i] = Read(ToTokenSpec(TokenKind.String)).Text;
            else if (typeName == TypeCode.Int32.ToString()) actualParameters[i] = int.Parse(Read(ToTokenSpec(TokenKind.Integer)).Text, CultureInfo.InvariantCulture);
            else if (type.GetGenericTypeDefinition() == typeof(Selector<>)) actualParameters[i] = ParseSubGenerator().Selector;
            else throw new ArgumentException(string.Format("Unsupported parameter type for custom selector '{0}'", name));
        }
        var selector = deleg.Method.DynamicInvoke(actualParameters);
        _generator.CustomSelector(selector!);
    }

    private bool TryFunctionalPseudo()
    {
        var token = TryRead(ToTokenSpec(TokenKind.Function));
        if (token == null) return false;
        TryRead(ToTokenSpec(TokenKind.WhiteSpace));
        switch (token.Value.Text)
        {
            case "eq": Eq(); break;
            case "nth-child": Nth(); break;
            case "nth-last-child": NthLast(); break;
            case "has": Has(); break;
            case "split-after": SplitAfter(); break;
            case "split-before": SplitBefore(); break;
            case "split-between": SplitBetween(); break;
            case "split-all": SplitAll(); break;
            case "before": Before(); break;
            case "after": After(); break;
            case "between": Between(); break;
            case "not": Not(); break;
            case "contains": Contains(); break;
            case "matches": Matches(); break;
            default: CustomSelector(token.Value.Text, true); break;
        }
        Read(ToTokenSpec(Token.RightParenthesis()));
        return true;
    }

    private void Contains() { var text = Read(ToTokenSpec(TokenKind.String)).Text; _generator.Contains(text); }
    private void Matches() { var text = Read(ToTokenSpec(TokenKind.String)).Text; _generator.Matches(text); }
    private void Has() => ParseWithExpression(_generator.Has);
    private void SplitAfter() => ParseWithExpression(_generator.SplitAfter);
    private void SplitBefore() => ParseWithExpression(_generator.SplitBefore);
    private void SplitBetween() => ParseWithExpression(_generator.SplitBetween);
    private void SplitAll() => ParseWithExpression(_generator.SplitAll);
    private void Before() => ParseWithExpression(_generator.Before);
    private void After() => ParseWithExpression(_generator.After);
    private void Between() { var gen1 = ParseSubGenerator(); Read(ToTokenSpec(Token.Semicolon())); Read(ToTokenSpec(TokenKind.WhiteSpace)); var gen2 = ParseSubGenerator(); _generator.Between(gen1, gen2); }
    private void Not() => ParseWithExpression(_generator.Not);
    private void ParseWithExpression(Action<ISelectorGenerator> generatorMethod) => generatorMethod(ParseSubGenerator());
    private ISelectorGenerator ParseSubGenerator() { var subgenerator = _generator.CreateNew(); new CssParser(_reader, subgenerator, false).ParseInternal(); return subgenerator; }
    private void Nth() => _generator.NthChild(1, NthB());
    private void NthLast() => _generator.NthLastChild(1, NthB());
    private void Eq() => _generator.Eq(NthB());
    private int NthB() => int.Parse(Read(ToTokenSpec(TokenKind.Integer)).Text, CultureInfo.InvariantCulture);

    private void Attrib()
    {
        Read(ToTokenSpec(Token.LeftBracket()));
        var prefix = TryNamespacePrefix() ?? NamespacePrefix.None;
        var name = Read(ToTokenSpec(TokenKind.Ident)).Text;
        var hasValue = false;
        while (true)
        {
            var op = TryRead(ToTokenSpec(Token.Equals()), ToTokenSpec(TokenKind.NotEqual), ToTokenSpec(TokenKind.Includes), ToTokenSpec(TokenKind.RegexMatch), ToTokenSpec(TokenKind.DashMatch), ToTokenSpec(TokenKind.PrefixMatch), ToTokenSpec(TokenKind.SuffixMatch), ToTokenSpec(TokenKind.SubstringMatch));
            if (op == null) break;
            hasValue = true;
            var value = Read(ToTokenSpec(TokenKind.String), ToTokenSpec(TokenKind.Ident)).Text;
            if (op.Value == Token.Equals()) _generator.AttributeExact(prefix, name, value);
            else
            {
                switch (op.Value.Kind)
                {
                    case TokenKind.Includes: _generator.AttributeIncludes(prefix, name, value); break;
                    case TokenKind.RegexMatch: _generator.AttributeRegexMatch(prefix, name, value); break;
                    case TokenKind.DashMatch: _generator.AttributeDashMatch(prefix, name, value); break;
                    case TokenKind.PrefixMatch: _generator.AttributePrefixMatch(prefix, name, value); break;
                    case TokenKind.SuffixMatch: _generator.AttributeSuffixMatch(prefix, name, value); break;
                    case TokenKind.SubstringMatch: _generator.AttributeSubstring(prefix, name, value); break;
                    case TokenKind.NotEqual: _generator.AttributeNotEqual(prefix, name, value); break;
                }
            }
        }
        if (!hasValue) _generator.AttributeExists(prefix, name);
        Read(ToTokenSpec(Token.RightBracket()));
    }

    private void Class() { Read(ToTokenSpec(Token.Dot())); _generator.Class(Read(ToTokenSpec(TokenKind.Ident)).Text); }

    private NamespacePrefix? TryNamespacePrefix()
    {
        var pipe = Token.Pipe();
        var token = TryRead(ToTokenSpec(TokenKind.Ident), ToTokenSpec(Token.Star()), ToTokenSpec(pipe));
        if (token == null) return null;
        if (token.Value == pipe) return NamespacePrefix.Empty;
        var prefix = token.Value;
        if (TryRead(ToTokenSpec(pipe)) == null) { Unread(prefix); return null; }
        return prefix.Kind == TokenKind.Ident ? new NamespacePrefix(prefix.Text) : NamespacePrefix.Any;
    }

    private void TypeSelectorOrUniversal()
    {
        var prefix = TryNamespacePrefix() ?? NamespacePrefix.None;
        var token = Read(ToTokenSpec(TokenKind.Ident), ToTokenSpec(Token.Star()));
        if (token.Kind == TokenKind.Ident) _generator.Type(prefix, token.Text);
        else _generator.Universal(prefix);
    }

    private Token Peek() => _reader.Peek();
    private Token Read(TokenSpec spec) { var token = TryRead(spec); if (token == null) throw new FormatException(string.Format("Unexpected token {{{0}}} where {{{1}}} was expected.", Peek().Kind, spec)); return token.Value; }
    private Token Read(params TokenSpec[] specs) { var token = TryRead(specs); if (token == null) throw new FormatException(string.Format("Unexpected token {{{0}}} where one of [{1}] was expected.", Peek().Kind, string.Join(", ", specs.Select(k => k.ToString()).ToArray()))); return token.Value; }
    private Token? TryRead(params TokenSpec[] specs) { foreach (var kind in specs) { var token = TryRead(kind); if (token != null) return token; } return null; }
    private Token? TryRead(TokenSpec spec) { var token = Peek(); if (!spec.Fold(a => a == token.Kind, b => b == token)) return null; _reader.Read(); return token; }
    private void Unread(Token token) => _reader.Unread(token);
    private static TokenSpec ToTokenSpec(TokenKind kind) => TokenSpec.A(kind);
    private static TokenSpec ToTokenSpec(Token token) => TokenSpec.B(token);

    internal static Dictionary<string, DelegateInfo> CustomSelectors = new();

    public static void RegisterCustomSelector<TNode>(string name, Func<Selector<TNode>> selector) => CustomSelectors.Add(name, new DelegateInfo { Method = selector, ParameterTypes = [] });
    public static void RegisterCustomSelector<TNode, T1>(string name, Func<T1, Selector<TNode>> selector) => CustomSelectors.Add(name, new DelegateInfo { Method = selector, ParameterTypes = [typeof(T1)] });
    public static void RegisterCustomSelector<TNode, T1, T2>(string name, Func<T1, T2, Selector<TNode>> selector) => CustomSelectors.Add(name, new DelegateInfo { Method = selector, ParameterTypes = [typeof(T1), typeof(T2)] });

    internal class DelegateInfo { public Delegate Method { get; set; } = null!; public List<Type> ParameterTypes { get; set; } = []; }
}

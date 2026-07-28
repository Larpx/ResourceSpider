using System;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

public struct Token : IEquatable<Token>
{
    public TokenKind Kind { get; private set; }
    public string Text { get; private set; }

    private Token(TokenKind kind) : this(kind, string.Empty) { }
    private Token(TokenKind kind, string text) : this() { Kind = kind; Text = text; }

    public static Token Eoi() => new(TokenKind.Eoi);
    private static readonly Token _star = Char('*');
    private static readonly Token _dot = Char('.');
    private static readonly Token _colon = Char(':');
    private static readonly Token _comma = Char(',');
    private static readonly Token _semicolon = Char(';');
    private static readonly Token _rightParenthesis = Char(')');
    private static readonly Token _equals = Char('=');
    private static readonly Token _pipe = Char('|');
    private static readonly Token _leftBracket = Char('[');
    private static readonly Token _rightBracket = Char(']');
    public static Token Star() => _star;
    public static Token Dot() => _dot;
    public static Token Colon() => _colon;
    public static Token Comma() => _comma;
    public static Token Semicolon() => _semicolon;
    public static Token RightParenthesis() => _rightParenthesis;
    public static Token Equals() => _equals;
    public static Token NotEqual() => new(TokenKind.NotEqual);
    public static Token LeftBracket() => _leftBracket;
    public static Token RightBracket() => _rightBracket;
    public static Token Pipe() => _pipe;
    public static Token Plus() => new(TokenKind.Plus);
    public static Token Greater() => new(TokenKind.Greater);
    public static Token Includes() => new(TokenKind.Includes);
    public static Token RegexMatch() => new(TokenKind.RegexMatch);
    public static Token DashMatch() => new(TokenKind.DashMatch);
    public static Token PrefixMatch() => new(TokenKind.PrefixMatch);
    public static Token SuffixMatch() => new(TokenKind.SuffixMatch);
    public static Token SubstringMatch() => new(TokenKind.SubstringMatch);
    public static Token Tilde() => new(TokenKind.Tilde);
    public static Token Slash() => new(TokenKind.Slash);
    public static Token Ident(string text) { ValidateTextArgument(text); return new Token(TokenKind.Ident, text); }
    public static Token Integer(string text) { ValidateTextArgument(text); return new Token(TokenKind.Integer, text); }
    public static Token Hash(string text) { ValidateTextArgument(text); return new Token(TokenKind.Hash, text); }
    public static Token WhiteSpace(string space) { ValidateTextArgument(space); return new Token(TokenKind.WhiteSpace, space); }
    public static Token String(string text) => new(TokenKind.String, text ?? string.Empty);
    public static Token Function(string text) { ValidateTextArgument(text); return new Token(TokenKind.Function, text); }
    public static Token Char(char ch) => new(TokenKind.Char, ch.ToString());

    public override bool Equals(object? obj) => obj != null && obj is Token && Equals((Token)obj);
    public override int GetHashCode() => Text == null ? Kind.GetHashCode() : Kind.GetHashCode() ^ Text.GetHashCode();
    public bool Equals(Token other) => Kind == other.Kind && Text == other.Text;
    public override string ToString() => Text == null ? Kind.ToString() : Kind + ": " + Text;
    public static bool operator ==(Token a, Token b) => a.Equals(b);
    public static bool operator !=(Token a, Token b) => !a.Equals(b);

    private static void ValidateTextArgument(string text)
    {
        if (text == null) throw new ArgumentNullException("text");
        if (text.Length == 0) throw new ArgumentException(null, "text");
    }
}

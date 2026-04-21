using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

public static class Tokener
{
    public static IEnumerable<Token> Tokenize(TextReader reader)
    {
        if (reader == null) throw new ArgumentNullException("reader");
        return Tokenize(reader.ReadToEnd());
    }

    public static IEnumerable<Token> Tokenize(string input)
    {
        var reader = new TokenReader(input ?? string.Empty);

        while (reader.Read() != null)
        {
            var ch = reader.Value;
            if (ch == '-' || IsNmStart(ch))
            {
                reader.Mark();
                if (reader.Value == '-')
                {
                    if (!IsNmStart(reader.Read()))
                        throw new FormatException(string.Format("Invalid identifier at position {0}.", reader.Position));
                }
                while (IsNmChar(reader.Read())) { }
                if (reader.Value == '(')
                    yield return Token.Function(reader.Marked());
                else
                    yield return Token.Ident(reader.MarkedWithUnread());
            }
            else if (IsDigit(ch))
            {
                reader.Mark();
                do { } while (IsDigit(reader.Read()));
                yield return Token.Integer(reader.MarkedWithUnread());
            }
            else if (IsS(ch))
            {
                var space = ParseWhiteSpace(reader);
                ch = reader.Read();
                switch (ch)
                {
                    case ',': yield return Token.Comma(); break;
                    case ';': yield return Token.Semicolon(); break;
                    case '+': yield return Token.Plus(); break;
                    case '>': yield return Token.Greater(); break;
                    case '~': yield return Token.Tilde(); break;
                    default: reader.Unread(); yield return Token.WhiteSpace(space); break;
                }
            }
            else switch (ch)
            {
                case '*': case '~': case '|':
                    if (reader.Read() == '=')
                    {
                        yield return ch == '*' ? Token.SubstringMatch()
                            : ch == '|' ? Token.DashMatch()
                            : Token.Includes();
                    }
                    else
                    {
                        reader.Unread();
                        yield return ch == '*' || ch == '|' ? Token.Char(ch.Value) : Token.Tilde();
                    }
                    break;
                case '^': case '$': case '%': case '!':
                    if (reader.Read() != '=')
                        throw new FormatException(string.Format("Invalid character at position {0}.", reader.Position));
                    switch (ch)
                    {
                        case '^': yield return Token.PrefixMatch(); break;
                        case '$': yield return Token.SuffixMatch(); break;
                        case '%': yield return Token.RegexMatch(); break;
                        case '!': yield return Token.NotEqual(); break;
                    }
                    break;
                case '.': yield return Token.Dot(); break;
                case ':': yield return Token.Colon(); break;
                case ',': yield return Token.Comma(); break;
                case ';': yield return Token.Semicolon(); break;
                case '=': yield return Token.Equals(); break;
                case '[': yield return Token.LeftBracket(); break;
                case ']': yield return Token.RightBracket(); break;
                case ')': yield return Token.RightParenthesis(); break;
                case '+': yield return Token.Plus(); break;
                case '>': yield return Token.Greater(); break;
                case '/': yield return Token.Slash(); break;
                case '#': yield return Token.Hash(ParseHash(reader)); break;
                case '\"': case '\'': yield return ParseString(reader, ch.Value); break;
                default: throw new FormatException(string.Format("Invalid character at position {0}.", reader.Position));
            }
        }
        yield return Token.Eoi();
    }

    private static string ParseWhiteSpace(TokenReader reader)
    {
        Debug.Assert(reader != null);
        reader.Mark();
        while (IsS(reader.Read())) { }
        return reader.MarkedWithUnread();
    }

    private static string ParseHash(TokenReader reader)
    {
        Debug.Assert(reader != null);
        reader.MarkFromNext();
        while (IsNmChar(reader.Read())) { }
        var text = reader.MarkedWithUnread();
        if (text.Length == 0) throw new FormatException(string.Format("Invalid hash at position {0}.", reader.Position));
        return text;
    }

    private static Token ParseString(TokenReader reader, char quote)
    {
        Debug.Assert(reader != null);
        var strpos = reader.Position;
        reader.MarkFromNext();
        char? ch;
        StringBuilder? sb = null;
        while ((ch = reader.Read()) != quote)
        {
            if (ch == null) throw new FormatException(string.Format("Unterminated string at position {0}.", strpos));
            if (ch == '\\')
            {
                ch = reader.Read();
                if (ch != quote && ch != '\\')
                    throw new FormatException(string.Format("Invalid escape sequence at position {0}.", reader.Position));
                if (sb == null) sb = new StringBuilder();
                sb.Append(reader.MarkedExceptLast());
                reader.Mark();
            }
        }
        var text = reader.Marked();
        if (sb != null) text = sb.Append(text).ToString();
        return Token.String(text);
    }

    private static bool IsDigit(char? ch) => ch >= '0' && ch <= '9';
    private static bool IsS(char? ch) => ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n' || ch == '\f';
    private static bool IsNmStart(char? ch) => ch == '_' || ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z';
    private static bool IsNmChar(char? ch) => IsNmStart(ch) || ch == '-' || ch >= '0' && ch <= '9';

    private sealed class TokenReader(string input)
    {
        private int _index = -1;
        private int _start = -1;
        private bool Ready => _index >= 0 && _index < input.Length;
        public char? Value => Ready ? input[_index] : null;
        public int Position => _index + 1;
        public void Mark() => _start = _index;
        public void MarkFromNext() => _start = _index + 1;
        public string Marked() => Marked(0);
        public string MarkedExceptLast() => Marked(-1);
        private string Marked(int trim)
        {
            var start = _start;
            var count = Math.Min(input.Length, _index + trim) - start;
            return count > 0 ? input.Substring(start, count) : string.Empty;
        }
        public char? Read() { _index = Position >= input.Length ? input.Length : _index + 1; return Value; }
        public void Unread() => _index = Math.Max(-1, _index - 1);
        public string MarkedWithUnread() { var text = Marked(); Unread(); return text; }
    }
}

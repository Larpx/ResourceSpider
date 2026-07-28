namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

public struct NamespacePrefix
{
    public static readonly NamespacePrefix None = new(null!);
    public static readonly NamespacePrefix Empty = new(string.Empty);
    public static readonly NamespacePrefix Any = new("*");

    public NamespacePrefix(string? text) : this()
    {
        Text = text ?? string.Empty;
    }

    public string Text { get; private set; } = string.Empty;
    public bool IsNone => Text == null;
    public bool IsAny => !IsNone && Text.Length == 1 && Text[0] == '*';
    public bool IsEmpty => !IsNone && Text.Length == 0;
    public bool IsSpecific => !IsNone && !IsAny;

    public override bool Equals(object? obj) => obj is NamespacePrefix && Equals((NamespacePrefix)obj);
    public bool Equals(NamespacePrefix other) => Text == other.Text;
    public override int GetHashCode() => IsNone ? 0 : Text.GetHashCode();
    public override string ToString() => IsNone ? "(none)" : Text;

    public string Format(string name)
    {
        if (name == null) throw new ArgumentNullException("name");
        if (name.Length == 0) throw new ArgumentException(null, "name");
        return Text + (IsNone ? "" : "|") + name;
    }
}

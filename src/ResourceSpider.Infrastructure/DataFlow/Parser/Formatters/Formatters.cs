namespace ResourceSpider.Infrastructure.DataFlow.Parser.Formatters;

public class TrimFormatter : FormatterAttribute
{
    protected override string Handle(string value) => value?.Trim() ?? string.Empty;
    protected override void CheckArguments() { }
}

public class ReplaceFormatter : FormatterAttribute
{
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    protected override string Handle(string value) => value?.Replace(OldValue ?? "", NewValue ?? "") ?? string.Empty;
    protected override void CheckArguments() { if (string.IsNullOrEmpty(OldValue)) throw new ArgumentException("OldValue is required"); }
}

public class RegexFormatter : FormatterAttribute
{
    public string Pattern { get; set; }
    public string Replacement { get; set; } = "$0";
    protected override string Handle(string value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(Pattern)) return value;
        try { var match = System.Text.RegularExpressions.Regex.Match(value, Pattern); return match.Success ? match.Result(Replacement) : value; }
        catch { return value; }
    }
    protected override void CheckArguments() { if (string.IsNullOrEmpty(Pattern)) throw new ArgumentException("Pattern is required"); }
}

public class RegexReplaceFormatter : FormatterAttribute
{
    public string Pattern { get; set; }
    public string Replacement { get; set; } = "";
    protected override string Handle(string value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(Pattern)) return value;
        try { return System.Text.RegularExpressions.Regex.Replace(value, Pattern, Replacement); }
        catch { return value; }
    }
    protected override void CheckArguments() { if (string.IsNullOrEmpty(Pattern)) throw new ArgumentException("Pattern is required"); }
}

public class HtmlDecodeFormatter : FormatterAttribute
{
    protected override string Handle(string value) => System.Net.WebUtility.HtmlDecode(value ?? "");
    protected override void CheckArguments() { }
}

public class UrlDecodeFormatter : FormatterAttribute
{
    protected override string Handle(string value) => Uri.UnescapeDataString(value ?? "");
    protected override void CheckArguments() { }
}

public class UrlEncodeFormatter : FormatterAttribute
{
    protected override string Handle(string value) => Uri.EscapeDataString(value ?? "");
    protected override void CheckArguments() { }
}

public class CharacterCaseFormatter : FormatterAttribute
{
    public bool ToUpper { get; set; }
    protected override string Handle(string value) => ToUpper ? value?.ToUpper() ?? "" : value?.ToLower() ?? "";
    protected override void CheckArguments() { }
}

public class CutoutFormatter : FormatterAttribute
{
    public int Start { get; set; }
    public int Length { get; set; } = int.MaxValue;
    protected override string Handle(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (Start >= value.Length) return "";
        var len = Math.Min(Length, value.Length - Start);
        return value.Substring(Start, len);
    }
    protected override void CheckArguments() { if (Start < 0) throw new ArgumentException("Start must be >= 0"); }
}

public class DisplaceFormatter : FormatterAttribute
{
    public string DisplacedValue { get; set; }
    protected override string Handle(string value) => string.IsNullOrEmpty(value) ? DisplacedValue ?? "" : value;
    protected override void CheckArguments() { }
}

public class SplitFormatter : FormatterAttribute
{
    public string Separator { get; set; } = ",";
    public int Index { get; set; }
    protected override string Handle(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var parts = value.Split(Separator);
        return Index >= 0 && Index < parts.Length ? parts[Index] : value;
    }
    protected override void CheckArguments() { }
}

public class TimeStampFormatter : FormatterAttribute
{
    public string Format { get; set; } = "yyyy-MM-dd HH:mm:ss";
    protected override string Handle(string value)
    {
        if (long.TryParse(value, out var timestamp))
        {
            var dto = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            return dto.ToString(Format);
        }
        return value;
    }
    protected override void CheckArguments() { }
}

public class StringFormatter : FormatterAttribute
{
    public string FormatTemplate { get; set; }
    protected override string Handle(string value) => string.IsNullOrEmpty(FormatTemplate) ? value : string.Format(FormatTemplate, value);
    protected override void CheckArguments() { }
}

public class RegexAppendFormatter : FormatterAttribute
{
    public string Pattern { get; set; }
    public string AppendValue { get; set; } = "";
    protected override string Handle(string value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(Pattern)) return value;
        try { return System.Text.RegularExpressions.Regex.IsMatch(value, Pattern) ? value + AppendValue : value; }
        catch { return value; }
    }
    protected override void CheckArguments() { if (string.IsNullOrEmpty(Pattern)) throw new ArgumentException("Pattern is required"); }
}

public class DigitUnitFormatter : FormatterAttribute
{
    public double Unit { get; set; } = 1.0;
    public string Suffix { get; set; } = "";
    protected override string Handle(string value)
    {
        if (double.TryParse(value, out var num)) return (num * Unit).ToString("F2") + Suffix;
        return value;
    }
    protected override void CheckArguments() { }
}

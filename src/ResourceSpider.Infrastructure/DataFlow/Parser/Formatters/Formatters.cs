namespace ResourceSpider.Infrastructure.DataFlow.Parser.Formatters;

/// <summary>
/// 去除首尾空白字符的格式化器
/// </summary>
public class TrimFormatter : FormatterAttribute
{
    protected override string? Handle(string? value) => value?.Trim() ?? string.Empty;
    protected override void CheckArguments() { }
}

/// <summary>
/// 字符串替换格式化器，将指定旧值替换为新值
/// </summary>
public class ReplaceFormatter : FormatterAttribute
{
    /// <summary>
    /// 要替换的旧值
    /// </summary>
    public string OldValue { get; set; } = string.Empty;

    /// <summary>
    /// 替换后的新值
    /// </summary>
    public string NewValue { get; set; } = string.Empty;

    protected override string? Handle(string? value) => value?.Replace(OldValue ?? "", NewValue ?? "") ?? string.Empty;
    protected override void CheckArguments() { if (string.IsNullOrEmpty(OldValue)) throw new ArgumentException("OldValue is required"); }
}

/// <summary>
/// 正则表达式匹配格式化器，提取第一个匹配结果
/// </summary>
public class RegexFormatter : FormatterAttribute
{
    /// <summary>
    /// 正则表达式模式
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// 替换模式，默认 "$0"（完整匹配）
    /// </summary>
    public string Replacement { get; set; } = "$0";

    protected override string? Handle(string? value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(Pattern)) return value;
        try { var match = System.Text.RegularExpressions.Regex.Match(value, Pattern); return match.Success ? match.Result(Replacement) : value; }
        catch { return value; }
    }
    protected override void CheckArguments() { if (string.IsNullOrEmpty(Pattern)) throw new ArgumentException("Pattern is required"); }
}

/// <summary>
/// 正则表达式替换格式化器，将所有匹配项替换为指定字符串
/// </summary>
public class RegexReplaceFormatter : FormatterAttribute
{
    /// <summary>
    /// 正则表达式模式
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// 替换字符串
    /// </summary>
    public string Replacement { get; set; } = "";

    protected override string? Handle(string? value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(Pattern)) return value;
        try { return System.Text.RegularExpressions.Regex.Replace(value, Pattern, Replacement); }
        catch { return value; }
    }
    protected override void CheckArguments() { if (string.IsNullOrEmpty(Pattern)) throw new ArgumentException("Pattern is required"); }
}

/// <summary>
/// HTML 解码格式化器，将 HTML 实体转换为普通字符
/// </summary>
public class HtmlDecodeFormatter : FormatterAttribute
{
    protected override string? Handle(string? value) => System.Net.WebUtility.HtmlDecode(value ?? "");
    protected override void CheckArguments() { }
}

/// <summary>
/// URL 解码格式化器，将 URL 编码字符串转换为普通字符串
/// </summary>
public class UrlDecodeFormatter : FormatterAttribute
{
    protected override string? Handle(string? value) => Uri.UnescapeDataString(value ?? "");
    protected override void CheckArguments() { }
}

/// <summary>
/// URL 编码格式化器，将字符串进行 URL 编码
/// </summary>
public class UrlEncodeFormatter : FormatterAttribute
{
    protected override string? Handle(string? value) => Uri.EscapeDataString(value ?? "");
    protected override void CheckArguments() { }
}

/// <summary>
/// 大小写转换格式化器，支持转换为大写或小写
/// </summary>
public class CharacterCaseFormatter : FormatterAttribute
{
    /// <summary>
    /// 是否转换为大写，false 时转换为小写
    /// </summary>
    public bool ToUpper { get; set; }

    protected override string? Handle(string? value) => ToUpper ? value?.ToUpper() ?? "" : value?.ToLower() ?? "";
    protected override void CheckArguments() { }
}

/// <summary>
/// 字符串截取格式化器，按起始位置和长度截取子字符串
/// </summary>
public class CutoutFormatter : FormatterAttribute
{
    /// <summary>
    /// 截取起始位置
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// 截取长度，默认为最大值（截取到末尾）
    /// </summary>
    public int Length { get; set; } = int.MaxValue;

    protected override string? Handle(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (Start >= value.Length) return "";
        var len = Math.Min(Length, value.Length - Start);
        return value.Substring(Start, len);
    }
    protected override void CheckArguments() { if (Start < 0) throw new ArgumentException("Start must be >= 0"); }
}

/// <summary>
/// 空值替换格式化器，当值为空时使用指定的替换值
/// </summary>
public class DisplaceFormatter : FormatterAttribute
{
    /// <summary>
    /// 空值时的替换值
    /// </summary>
    public string DisplacedValue { get; set; } = string.Empty;

    protected override string? Handle(string? value) => string.IsNullOrEmpty(value) ? DisplacedValue ?? "" : value;
    protected override void CheckArguments() { }
}

/// <summary>
/// 分割格式化器，按分隔符分割字符串并取指定索引的元素
/// </summary>
public class SplitFormatter : FormatterAttribute
{
    /// <summary>
    /// 分隔符，默认逗号
    /// </summary>
    public string Separator { get; set; } = ",";

    /// <summary>
    /// 要取的元素索引
    /// </summary>
    public int Index { get; set; }

    protected override string? Handle(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var parts = value.Split(Separator);
        return Index >= 0 && Index < parts.Length ? parts[Index] : value;
    }
    protected override void CheckArguments() { }
}

/// <summary>
/// 时间戳格式化器，将 Unix 时间戳转换为日期时间字符串
/// </summary>
public class TimeStampFormatter : FormatterAttribute
{
    /// <summary>
    /// 日期时间格式，默认 "yyyy-MM-dd HH:mm:ss"
    /// </summary>
    public new string Format { get; set; } = "yyyy-MM-dd HH:mm:ss";

    protected override string? Handle(string? value)
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

/// <summary>
/// 字符串格式化器，使用格式模板对值进行格式化
/// </summary>
public class StringFormatter : FormatterAttribute
{
    /// <summary>
    /// 格式模板，如 "值：{0}"
    /// </summary>
    public string FormatTemplate { get; set; } = string.Empty;

    protected override string? Handle(string? value) => string.IsNullOrEmpty(FormatTemplate) ? value : string.Format(FormatTemplate, value);
    protected override void CheckArguments() { }
}

/// <summary>
/// 正则追加格式化器，当值匹配正则时追加指定字符串
/// </summary>
public class RegexAppendFormatter : FormatterAttribute
{
    /// <summary>
    /// 正则表达式模式
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// 匹配成功时追加的字符串
    /// </summary>
    public string AppendValue { get; set; } = "";

    protected override string? Handle(string? value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(Pattern)) return value;
        try { return System.Text.RegularExpressions.Regex.IsMatch(value, Pattern) ? value + AppendValue : value; }
        catch { return value; }
    }
    protected override void CheckArguments() { if (string.IsNullOrEmpty(Pattern)) throw new ArgumentException("Pattern is required"); }
}

/// <summary>
/// 数字单位格式化器，将数值乘以单位系数并添加后缀
/// </summary>
public class DigitUnitFormatter : FormatterAttribute
{
    /// <summary>
    /// 单位系数，默认 1.0
    /// </summary>
    public double Unit { get; set; } = 1.0;

    /// <summary>
    /// 数值后缀，如 "万"、"k"
    /// </summary>
    public string Suffix { get; set; } = "";

    protected override string? Handle(string? value)
    {
        if (double.TryParse(value, out var num)) return (num * Unit).ToString("F2") + Suffix;
        return value;
    }
    protected override void CheckArguments() { }
}

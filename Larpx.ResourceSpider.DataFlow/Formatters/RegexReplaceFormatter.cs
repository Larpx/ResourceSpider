using System;
using System.Text.RegularExpressions;

namespace Larpx.ResourceSpider.DataFlow.Formatters
{
    /// <summary>
    /// 在指定的输入字符串中，用指定的替换字符串替换与指定正则表达式匹配的所有字符串。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RegexReplaceFormatter : Formatter
    {
        /// <summary>
        /// 正则表达式
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// 要替换的字符串
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// 实现数值的转化
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>被格式化后的数值</returns>
        protected override string Handle(string value)
        {
            return Regex.Replace(value, Pattern, NewValue);
        }

        /// <summary>
        /// 校验参数是否设置正确
        /// </summary>
        protected override void CheckArguments()
        {
            if (string.IsNullOrWhiteSpace(Pattern))
            {
                throw new ArgumentException("Pattern should not be null or empty");
            }
        }
    }
}

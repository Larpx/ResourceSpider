using System;

namespace Larpx.ResourceSpider.Selector.HtmlAgilityPack.Css
{
    /// <summary>
    /// 表示类型或属性名称。
    /// </summary>
    public struct NamespacePrefix
    {
        /// <summary>
        /// 表示目标文档中默认命名空间或任何命名空间中的名称，
        /// 具体取决于默认命名空间是否有效
        /// </summary>
        public static readonly NamespacePrefix None = new NamespacePrefix(null);

        /// <summary>
        /// 表示空命名空间。
        /// </summary>
        public static readonly NamespacePrefix Empty = new NamespacePrefix(string.Empty);

        /// <summary>
        /// 表示任意命名空间。
        /// </summary>
        public static readonly NamespacePrefix Any = new NamespacePrefix("*");

        /// <summary>
        /// 使用命名空间前缀规范初始化实例。
        /// </summary>
        public NamespacePrefix(string text) : this()
        {
            Text = text;
        }

        /// <summary>
        /// 获取此实例的原始文本值。
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Indicates whether this instance represents a name
        /// from either the default or any namespace in a target
        /// document, depending on whether a default namespace is
        /// in effect or not.
        /// </summary>
        public bool IsNone => Text == null;

        /// <summary>
        /// Indicates whether this instance represents a name
        /// from any namespace (including one without one)
        /// in a target document.
        /// </summary>
        public bool IsAny => !IsNone && Text.Length == 1 && Text[0] == '*';

        /// <summary>
        /// Indicates whether this instance represents a name
        /// without a namespace in a target document.
        /// </summary>
        public bool IsEmpty => !IsNone && Text.Length == 0;

        /// <summary>
        /// Indicates whether this instance represents a name from a 
        /// specific namespace or not.
        /// </summary>
        public bool IsSpecific => !IsNone && !IsAny;

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is NamespacePrefix && Equals((NamespacePrefix)obj);
        }

        /// <summary>
        /// Indicates whether this instance and another are equal.
        /// </summary>
        public bool Equals(NamespacePrefix other)
        {
            return Text == other.Text;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return IsNone ? 0 : Text.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return IsNone ? "(none)" : Text;
        }

        /// <summary>
        /// Formats this namespace together with a name.
        /// </summary>
        public string Format(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (name.Length == 0) throw new ArgumentException(null, "name");

            return Text + (IsNone ? null : "|") + name;
        }
    }
}

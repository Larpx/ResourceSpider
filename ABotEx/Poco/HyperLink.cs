using System;

namespace Larpx.ResourceSpider.ABotEx.Poco
{
    /// <summary>
    /// 超链接
    /// </summary>
    public class HyperLink : IEquatable<HyperLink>
    {
        /// <summary>
        /// 原始Href值
        /// </summary>
        public string RawHrefValue { get; set; }

        /// <summary>
        /// 原始Href文本
        /// </summary>
        public string RawHrefText { get; set; }

        /// <summary>
        /// Href值
        /// </summary>
        public Uri HrefValue { get; set; }

        public override int GetHashCode()
        {
            return HrefValue != null ? HrefValue.AbsoluteUri.GetHashCode() : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HyperLink);
        }

        public bool Equals(HyperLink other)
        {
            return 
                this.HrefValue != null && other.HrefValue != null ? 
                    this.HrefValue.AbsoluteUri == other.HrefValue.AbsoluteUri : 
                    this.RawHrefValue == other.RawHrefValue;
        }
    }
}

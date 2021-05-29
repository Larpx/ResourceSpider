using System.Text;

namespace Larpx.ResourceSpider.ABotEx.Poco
{
    /// <summary>
    /// 页面内容
    /// </summary>
    public class PageContent
    {
        /// <summary>
        /// 页面内容
        /// </summary>
        public PageContent()
        {
            Text = "";
        }

        /// <summary>
        /// 从web响应获取的原始数据字节
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// 内容的编码类型和字符集
        /// </summary>
        public string Charset { get; set; }

        /// <summary>
        /// web响应的编码
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// 从web响应中获取的原始文本
        /// </summary>
        public string Text { get; set; }
    }
}

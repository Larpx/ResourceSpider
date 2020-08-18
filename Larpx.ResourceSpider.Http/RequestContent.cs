using Larpx.ResourceSpider.BaseLibrary.Helpers;
using MessagePack;
using System.Collections.Generic;

namespace Larpx.ResourceSpider.Http
{
    public abstract class RequestContent
    {
        /// <summary>
        /// Headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 设置Http头信息
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void SetHeader(string header, string value)
        {
            header.NotNullOrWhiteSpace(nameof(header));
            value.NotNullOrWhiteSpace(nameof(value));

            if (Headers.ContainsKey(header))
            {
                Headers[header] = value.Trim();
            }
            else
            {
                Headers.Add(header, value.Trim());
            }
        }

        /// <summary>
        /// 获取Http头
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public string GetHeader(string header)
        {
            header.NotNullOrWhiteSpace(nameof(header));
            return Headers.ContainsKey(header) ? Headers[header] : null;
        }

        public virtual byte[] ToBytes()
        {
            return MessagePackSerializer.Typeless.Serialize(this);
        }
    }
}

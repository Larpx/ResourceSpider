using System;
using System.Collections.Generic;

namespace Larpx.ResourceSpider.Http.Content
{
    [Serializable]
    public class FormUrlEncodedContent : RequestContent
    {
        /// <summary>
        /// 参数
        /// </summary>
        public List<KeyValuePair<string, string>> NameValueCollection { get; set; }

        public FormUrlEncodedContent()
        {
            NameValueCollection = new List<KeyValuePair<string, string>>();
        }

        public FormUrlEncodedContent(List<KeyValuePair<string, string>> nameValueCollection)
        {
            this.NameValueCollection.AddRange(nameValueCollection);
        }
    }
}

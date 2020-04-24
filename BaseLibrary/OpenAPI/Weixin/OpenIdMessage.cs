using System;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// OpenID列表群发消息
    /// </summary>
    public sealed class OpenIdMessage : BulkMessageBase
    {
        /// <summary>
        /// 接收者，一串OpenID列表，OpenID最少2个，最多10000个
        /// </summary>
        public string[] touser;
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="value"></param>
        
        [Json.SerializeCustom]
        private static void toJson(Json.Serializer serializer, OpenIdMessage value)
        {
            serializer.CustomWriteFirstName("touser");
            serializer.TypeSerialize(value.touser);
            value.toJson(serializer);
        }
    }
}

using System;
usingMetadata;

namespace Larpx.ResourceSpider.BaseLibrary.Xml
{
    /// <summary>
    /// XML序列化成员配置
    /// </summary>
    public class MemberAttribute :Metadata.IgnoreMemberAttribute
    {
        /// <summary>
        /// 集合子节点名称(不能包含非法字符)
        /// </summary>
        public string ItemName;
    }
}

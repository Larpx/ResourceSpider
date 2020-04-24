using System;

namespace Larpx.ResourceSpider.BaseLibrary.Xml
{
    /// <summary>
    /// XML 序列化成员忽略配置
    /// </summary>
    public sealed class IgnoreMemberAttribute :Metadata.IgnoreMemberAttribute
    {
        /// <summary>
        /// 禁止当前安装
        /// </summary>
        internal override bool GetIsIgnoreCurrent
        {
            get { return true; }
        }
    }
}

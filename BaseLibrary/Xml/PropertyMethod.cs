using System;
usingMetadata;
using System.Reflection;

namespace Larpx.ResourceSpider.BaseLibrary.Xml
{
    /// <summary>
    /// 属性成员信息
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    internal struct PropertyMethod
    {
        /// <summary>
        /// 属性索引
        /// </summary>
        public PropertyIndex Property;
        /// <summary>
        /// 访问函数
        /// </summary>
        public MethodInfo Method;
        /// <summary>
        /// 自定义属性
        /// </summary>
        public MemberAttribute Attribute;
    }
}

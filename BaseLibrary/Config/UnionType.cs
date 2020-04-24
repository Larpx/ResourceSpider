using System.Runtime.InteropServices;

namespace Larpx.ResourceSpider.BaseLibrary.Config
{
    /// <summary>
    /// 类型转换
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct UnionType
    {
        /// <summary>
        /// 目标对象
        /// </summary>
        [FieldOffset(0)]
        public object Value;
        /// <summary>
        /// 公用全局配置
        /// </summary>
        [FieldOffset(0)]
        public Pub Pub;
    }
}

using System;

namespace Larpx.ResourceSpider.BaseLibrary.Config
{
    /// <summary>
    /// 配置加载
    /// </summary>
    internal static partial class UnionLoader
    {
        /// <summary>
        /// 获取配置数据
        /// </summary>
        /// <param name="type">配置类型</param>
        /// <param name="name">配置名称</param>
        /// <returns></returns>
        
        internal static UnionType GetUnion(Type type, string name = "")
        {
            return new UnionType { Value = AutoCSer.Config.Loader.GetObject(type, name) };
        }
    }
}

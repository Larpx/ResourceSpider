namespace Larpx.ResourceSpider.BaseLibrary
{
    /// <summary>
    /// 空值相关参数
    /// </summary>
    /// <typeparam name="valueType">值类型</typeparam>
    public static class NullValue<valueType>
    {
        /// <summary>
        /// 0元素数组
        /// </summary>
        public static readonly valueType[] Array = new valueType[0];
    }
}

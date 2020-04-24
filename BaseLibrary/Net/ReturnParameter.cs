using System;

namespace Larpx.ResourceSpider.BaseLibrary.Net
{
#if NOJIT
    /// <summary>
    /// 返回参数
    /// </summary>
    public interface IReturnParameter
    {
        /// <summary>
        /// 返回值
        /// </summary>
        object ReturnObject { get; set; }
    }
    ///// <summary>
    ///// 返回参数
    ///// </summary>
    ///// <typeparam name="valueType">返回参数类型</typeparam>
    //public class ReturnParameter<valueType> : IReturnParameter
    //{
    //    [Json.SerializeMember(IsIgnoreCurrent = true)]
    //    [Json.ParseMember(IsIgnoreCurrent = true)]
    //    internal valueType Ret;
    //    /// <summary>
    //    /// 返回值
    //    /// </summary>
    //    public valueType Return
    //    {
    //        get { return Ret; }
    //        set { Ret = value; }
    //    }
    //    /// <summary>
    //    /// 返回值
    //    /// </summary>
    //    [Metadata.Ignore]
    //    public object ReturnObject
    //    {
    //        get { return Ret; }
    //        set { Ret = (valueType)value; }
    //    }
    //}
#else
    /// <summary>
    /// 返回参数
    /// </summary>
    /// <typeparam name="valueType">返回参数类型</typeparam>
    public interface IReturnParameter<valueType>
    {
        /// <summary>
        /// 返回值
        /// </summary>
        valueType Return { get; set; }
    }
    ///// <summary>
    ///// 返回参数
    ///// </summary>
    ///// <typeparam name="valueType">返回参数类型</typeparam>
    //public class ReturnParameter<valueType> : IReturnParameter<valueType>
    //{
    //    [Json.SerializeMember(IsIgnoreCurrent = true)]
    //    [Json.ParseMember(IsIgnoreCurrent = true)]
    //    internal valueType Ret;
    //    /// <summary>
    //    /// 返回值
    //    /// </summary>
    //    public valueType Return
    //    {
    //        get { return Ret; }
    //        set { Ret = value; }
    //    }
    //}
#endif
}

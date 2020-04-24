using System;

namespace Larpx.ResourceSpider.BaseLibrary.Net.TcpServer
{
    /// <summary>
    /// await 返回值包装
    /// </summary>
    /// <typeparam name="returnType">返回值类型</typeparam>
    [BinarySerialize.Serialize(IsMemberMap = false, IsReferenceMember = false)]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    public struct AwaiterReturnValue<returnType>
#if NOJIT
        : Net.IReturnParameter
#else
        : Net.IReturnParameter<returnType>
#endif
    {
        /// <summary>
        /// 返回值
        /// </summary>
        [Json.IgnoreMember]
        public returnType Ret;
        /// <summary>
        /// 返回值
        /// </summary>
        
        public returnType Return
        {
            get { return Ret; }
            set { Ret = value; }
        }
#if NOJIT
        /// <summary>
        /// 返回值
        /// </summary>
        [Metadata.Ignore]
        public object ReturnObject
        {
            get { return Ret; }
            set { Ret = (returnType)value; }
        }
#endif
    }
    /// <summary>
    /// await 返回值包装
    /// </summary>
    /// <typeparam name="returnType">返回值类型</typeparam>
    [BinarySerialize.Serialize(IsMemberMap = false, IsReferenceMember = false)]
    [Metadata.BoxSerialize]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    public struct AwaiterReturnValueBox<returnType>
#if NOJIT
        : Net.IReturnParameter
#else
        : Net.IReturnParameter<returnType>
#endif
    {
        /// <summary>
        /// 返回值
        /// </summary>
        [Json.IgnoreMember]
        public returnType Ret;
        /// <summary>
        /// 返回值
        /// </summary>
        
        public returnType Return
        {
            get { return Ret; }
            set { Ret = value; }
        }
#if NOJIT
        /// <summary>
        /// 返回值
        /// </summary>
        [Metadata.Ignore]
        public object ReturnObject
        {
            get { return Ret; }
            set { Ret = (returnType)value; }
        }
#endif
    }
    /// <summary>
    /// await 返回值包装
    /// </summary>
    /// <typeparam name="returnType">返回值类型</typeparam>
    [BinarySerialize.Serialize(IsMemberMap = false)]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    public struct AwaiterReturnValueReference<returnType>
#if NOJIT
        : Net.IReturnParameter
#else
        : Net.IReturnParameter<returnType>
#endif
    {
        /// <summary>
        /// 返回值
        /// </summary>
        [Json.IgnoreMember]
        public returnType Ret;
        /// <summary>
        /// 返回值
        /// </summary>
        
        public returnType Return
        {
            get { return Ret; }
            set { Ret = value; }
        }
#if NOJIT
        /// <summary>
        /// 返回值
        /// </summary>
        [Metadata.Ignore]
        public object ReturnObject
        {
            get { return Ret; }
            set { Ret = (returnType)value; }
        }
#endif
    }
    /// <summary>
    /// await 返回值包装
    /// </summary>
    /// <typeparam name="returnType">返回值类型</typeparam>
    [BinarySerialize.Serialize(IsMemberMap = false)]
    [Metadata.BoxSerialize]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    public struct AwaiterReturnValueBoxReference<returnType>
#if NOJIT
        : Net.IReturnParameter
#else
        : Net.IReturnParameter<returnType>
#endif
    {
        /// <summary>
        /// 返回值
        /// </summary>
        [Json.IgnoreMember]
        public returnType Ret;
        /// <summary>
        /// 返回值
        /// </summary>
        
        public returnType Return
        {
            get { return Ret; }
            set { Ret = value; }
        }
#if NOJIT
        /// <summary>
        /// 返回值
        /// </summary>
        [Metadata.Ignore]
        public object ReturnObject
        {
            get { return Ret; }
            set { Ret = (returnType)value; }
        }
#endif
    }
}

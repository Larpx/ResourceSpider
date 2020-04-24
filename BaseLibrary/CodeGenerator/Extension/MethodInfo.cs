using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary.Extension
{
    /// <summary>
    /// 成员方法相关操作
    /// </summary>
    public static class MethodInfoExtension
    {
        /// <summary>
        /// 成员方法全名
        /// </summary>
        /// <param name="method">成员方法</param>
        /// <returns>成员方法全名</returns>
        
        public static string fullName(this MethodInfo method)
        {
            return method != null ? method.DeclaringType.fullName() + "." + method.Name : null;
        }
    }
}

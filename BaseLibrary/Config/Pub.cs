using System.Text;

namespace Larpx.ResourceSpider.BaseLibrary.Config
{
    /// <summary>
    /// 公用全局配置
    /// </summary>
    public class Pub
    {
        /// <summary>
        /// 是否调试模式
        /// </summary>
        public bool IsDebug;
        /// <summary>
        /// 全局编码
        /// </summary>
        public Encoding Encoding = Encoding.UTF8;
        ///// <summary>
        ///// 程序工作主目录
        ///// </summary>
        //public string WorkPath = AutoCSer.PubPath.ApplicationPath;

        /// <summary>
        /// 是否 window 服务模式
        /// </summary>
        public bool IsService;

        /// <summary>
        /// 默认全局配置
        /// </summary>
        public static readonly Pub Default;
        static Pub()
        {
            Default = UnionLoader.GetUnion(typeof(Pub)).Pub ?? new Pub();
        }
    }
}

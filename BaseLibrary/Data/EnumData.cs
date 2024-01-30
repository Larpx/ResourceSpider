namespace Larpx.ResourceSpider.BaseLibrary.Data
{
    public class EnumData
    {
        /// <summary>
        /// meta类型
        /// </summary>
        public enum MetaType
        {
            Charset,
            Viewport,
            Keywords,
            Description,
            Renderer,
            X_UA_Compatible,
            Cache_Control,
            Other
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public enum DatabaseType
        {
            MySql = 0,
            SqlServer = 1,
            Sqlite = 2,
            Oracle = 3,
            PostgreSQL = 4
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public enum ResourceType
        {
            Title = 0,
            Banner = 1,
            Detail = 2
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public enum ProcessedType
        {
            Pending,          // 等待
            Success,          // 成功
            Fail              // 失败
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public enum URLType
        {
            URL,
            ed2k,
            magnet,
            other
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public enum ResourceDataType
        {
            Image = 0,
            Vedio = 1,
            Word = 2,
            Other = 3
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public enum ResourceDataStatus
        {
            Ban,                      // 禁止
            Normal,                   // 正常
            undercarriage             // 下架
        }

        /// <summary>
        /// 选择器类型
        /// </summary>
        public enum SelectableType
        {
            Text,
            Html,
            Json
        }
    }
}

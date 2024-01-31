namespace Larpx.ResourceSpider.DotnetSpiderEx.AgentCenter
{
    public class AgentCenterOptions
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 数据库名
        /// </summary>
        public string Database { get; set; } = "Larpx.ResourceSpider.DotnetSpiderEx";
    }
}

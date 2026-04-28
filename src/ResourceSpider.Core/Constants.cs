namespace ResourceSpider.Core;

/// <summary>
/// 系统全局常量定义，集中管理所有重复使用的固定字符串
/// </summary>
public static class Constants
{
    /// <summary>
    /// API 路由前缀
    /// </summary>
    public static class ApiRoutes
    {
        public const string Agent = "api/agent";
        public const string AgentRegister = "api/agent/register";
        public const string AgentHeartbeat = "api/agent/heartbeat";
        public const string AgentUnregister = "api/agent/unregister";
        public const string AgentTasks = "api/agent/tasks";
        public const string AgentPullTasks = "api/agent/tasks/pull";
        public const string AgentTaskContent = "api/agent/tasks/content";
        public const string AgentReportTask = "api/agent/tasks/report";
        public const string AgentPullExpression = "api/agent/expressions/pull";
        public const string AgentActiveExpressions = "api/agent/expressions/active";
        public const string AgentExpressionAvailability = "api/agent/expressions/availability";
        public const string AgentStoreResults = "api/agent/results/store";
        public const string AgentStepReport = "api/agent/tasks/step/report";
        public const string AgentResourcesPull = "api/agent/resources/pull";
        public const string AgentPrefetch = "api/agent/tasks/prefetch";
        public const string AgentStatus = "api/agent/status";
        public const string Tasks = "api/tasks";
        public const string Auth = "api/auth";
        public const string AuthLogin = "api/auth/login";
        public const string AuthRegister = "api/auth/register";
        public const string Results = "api/results";
        public const string Proxies = "api/proxies";
        public const string Statistics = "api/statistics";
        public const string System = "api/system";
        public const string Config = "api/config";
        public const string Expressions = "api/expressions";
        public const string AgentGroups = "api/agents/groups";
        public const string Agents = "api/agents";
        public const string CollectionResults = "api/collection-results";
    }

    /// <summary>
    /// SignalR Hub 相关常量
    /// </summary>
    public static class Hub
    {
        public const string SpiderHubPath = "/hubs/spider";
        public const string MethodTaskAssign = "TaskAssign";
        public const string MethodConfigUpdate = "ConfigUpdate";
        public const string MethodControlCommand = "ControlCommand";
        public const string MethodJoinAgentGroup = "JoinAgentGroup";
        public const string MethodLeaveAgentGroup = "LeaveAgentGroup";
        public const string MethodAck = "Ack";
    }

    /// <summary>
    /// 默认值常量
    /// </summary>
    public static class Defaults
    {
        public const string DefaultHttpMethod = "GET";
        public const string DefaultProtocol = "HTTP";
        public const string DefaultOutputFormat = "csv";
        public const string DefaultTaskType = "SinglePage";
        public const string DefaultCollectionMode = "HttpClient";
        public const string DefaultSelectorType = "XPath";
        public const string DefaultLogLevel = "Info";
        public const string DefaultUserRole = "Operator";
        public const string DefaultAgentMode = "Local";
        public const string DefaultBrowserType = "Chromium";
        public const string DefaultWaitUntil = "NetworkIdle";
        public const int DefaultHeartbeatInterval = 30;
        public const int DefaultMaxConcurrentTasks = 5;
        public const int DefaultMaxConcurrentRequests = 10;
        public const int DefaultRetryCount = 3;
        public const int DefaultRetryDelayMs = 1000;
        public const int DefaultConnectionTimeout = 30;
        public const int DefaultRequestTimeout = 60;
        public const int DefaultPriority = 5;
        public const int DefaultMaxRetry = 3;
        public const int DefaultRequestedQueueCount = 1000;
        public const int DefaultEmptySleepTime = 60;
        public const int DefaultRefreshProxy = 30;
        public const int DefaultBatchSize = 4;
        public const int DefaultMaxInstances = 5;
        public const int DefaultMaxLifetimeMinutes = 30;
        public const int DefaultPlaywrightTimeout = 30000;
        public const int DefaultViewportWidth = 1920;
        public const int DefaultViewportHeight = 1080;
        public const int DefaultStartPage = 1;
        public const int DefaultPageIncrement = 1;
        public const int DefaultScrollWaitTime = 2000;
        public const int DefaultMessageQueueCapacity = 10000;
        public const int DefaultRateLimitPermitLimit = 100;
        public const int DefaultRateLimitWindowSeconds = 60;
        public const double DefaultSpeed = 1.0;
        public const double DefaultProxyHealthScore = 1.0;
        public const double DefaultProxyMinHealthScore = 0.3;
        public const double DefaultProxyWarnHealthScore = 0.5;
    }

    /// <summary>
    /// 执行状态字符串
    /// </summary>
    public static class ExecutionStatus
    {
        public const string Success = "Success";
        public const string Failed = "Failed";
        public const string Running = "Running";
        public const string Pending = "Pending";
        public const string Cancelled = "Cancelled";
    }

    /// <summary>
    /// 转换规则类型
    /// </summary>
    public static class TransformTypes
    {
        public const string Trim = "trim";
        public const string Replace = "replace";
        public const string RegexReplace = "regexreplace";
        public const string LowerCase = "lowercase";
        public const string UpperCase = "uppercase";
    }

    /// <summary>
    /// 系统变量模板
    /// </summary>
    public static class SystemVariables
    {
        public const string TaskId = "{{TASK_ID}}";
        public const string StepId = "{{STEP_ID}}";
        public const string AgentId = "{{AGENT_ID}}";
        public const string Timestamp = "{{TIMESTAMP}}";
        public const string PageNum = "{{PAGE_NUM}}";
        public const string RandomInt = "{{RANDOM_INT}}";
        public const string Uuid = "{{UUID}}";
    }

    /// <summary>
    /// 代理验证 URL
    /// </summary>
    public static class ProxyValidation
    {
        public const string DefaultTestUrl = "http://httpbin.org/ip";
        public const int DefaultTimeoutMs = 10000;
    }

    /// <summary>
    /// Redis 键前缀
    /// </summary>
    public static class RedisKeys
    {
        public const string TaskQueuePrefix = "task:queue:";
        public const string RequestDedupPrefix = "request:dedup:";
        public const string AgentOnlinePrefix = "agent:online:";
        public const string SchedulerStatePrefix = "scheduler:state:";
        public const string ProxyPool = "proxy:pool";
    }

    /// <summary>
    /// HTTP 请求头
    /// </summary>
    public static class HttpHeaders
    {
        public const string AgentId = "X-Agent-Id";
        public const string Authorization = "Authorization";
        public const string BearerPrefix = "Bearer ";
        public const string ContentType = "Content-Type";
        public const string UserAgent = "User-Agent";
    }

    /// <summary>
    /// 分页占位符
    /// </summary>
    public static class Pagination
    {
        public const string PagePlaceholder = "{page}";
    }

    /// <summary>
    /// 本地 Agent ID 前缀
    /// </summary>
    public static class Agent
    {
        public const string LocalAgentIdPrefix = "agent-local-";
        public const string LocalAgentNamePrefix = "Local Agent";
    }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public static class FileExtensions
    {
        public const string Json = ".json";
        public const string Csv = ".csv";
        public const string Txt = ".txt";
    }

    /// <summary>
    /// 日志类别
    /// </summary>
    public static class LogCategories
    {
        public const string Agent = "Agent";
        public const string Task = "Task";
        public const string Scheduler = "Scheduler";
        public const string Downloader = "Downloader";
        public const string Parser = "Parser";
        public const string Proxy = "Proxy";
        public const string Storage = "Storage";
        public const string Communication = "Communication";
    }
}

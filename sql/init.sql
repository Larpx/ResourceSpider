-- ResourceSpider Database Initialization Script
-- MySQL 8.0+

CREATE DATABASE IF NOT EXISTS ResourceSpider DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE ResourceSpider;

-- Agent Table
CREATE TABLE IF NOT EXISTS agents (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    AgentId VARCHAR(64) UNIQUE NOT NULL COMMENT 'Agent唯一标识',
    AgentName VARCHAR(128) NOT NULL COMMENT 'Agent名称',
    AgentToken VARCHAR(256) NOT NULL COMMENT '认证Token',
    IpAddress VARCHAR(45) NOT NULL COMMENT 'IP地址',
    Port INT NOT NULL COMMENT '端口',
    Capabilities JSON COMMENT '能力配置',
    Status TINYINT DEFAULT 1 COMMENT '状态: 0-离线, 1-在线, 2-忙碌',
    CpuUsage DECIMAL(5,2) COMMENT 'CPU使用率',
    MemoryUsage DECIMAL(5,2) COMMENT '内存使用率',
    TaskCount INT DEFAULT 0 COMMENT '任务数量',
    LastHeartbeat DATETIME COMMENT '最后心跳时间',
    Tags JSON COMMENT '标签',
    GroupId VARCHAR(64) COMMENT '分组ID',
    OS VARCHAR(100) COMMENT '操作系统',
    Version VARCHAR(50) COMMENT '版本号',
    Config JSON COMMENT '配置',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_status (Status),
    INDEX idx_last_heartbeat (LastHeartbeat),
    INDEX idx_group_id (GroupId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Agent信息表';

-- Agent Group Table
CREATE TABLE IF NOT EXISTS agent_groups (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    GroupId VARCHAR(64) UNIQUE NOT NULL COMMENT '分组唯一标识',
    GroupName VARCHAR(128) NOT NULL COMMENT '分组名称',
    Description VARCHAR(512) COMMENT '描述',
    AgentIds JSON COMMENT 'Agent ID列表',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Agent分组表';

-- Task Table
CREATE TABLE IF NOT EXISTS tasks (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    TaskId VARCHAR(64) UNIQUE NOT NULL COMMENT '任务唯一标识',
    TaskName VARCHAR(256) NOT NULL COMMENT '任务名称',
    TaskType VARCHAR(64) NOT NULL DEFAULT 'SinglePage' COMMENT '任务类型: SinglePage/Paginated/MultiStage',
    Priority INT DEFAULT 5 COMMENT '优先级: 1-10',
    Status TINYINT DEFAULT 0 COMMENT '状态: 0-待执行, 1-执行中, 2-已完成, 3-失败, 4-暂停, 5-等待恢复, 6-已取消',
    RequestConfig JSON NOT NULL COMMENT '请求配置',
    ScheduleConfig JSON COMMENT '调度配置',
    RetryPolicy JSON COMMENT '重试策略',
    AntiCrawlConfig JSON COMMENT '反爬策略',
    GlobalConfig JSON COMMENT '全局配置',
    ConfigVersion INT DEFAULT 1 COMMENT '配置版本号',
    Tags JSON COMMENT '标签',
    AgentGroupId VARCHAR(64) COMMENT '指定Agent分组',
    AssignedAgentId VARCHAR(64) COMMENT '分配的Agent',
    ExpressionId VARCHAR(64) COMMENT '关联的表达式ID',
    Progress DECIMAL(5,2) DEFAULT 0 COMMENT '完成进度百分比',
    TotalRequests INT DEFAULT 0 COMMENT '总请求数',
    CompletedRequests INT DEFAULT 0 COMMENT '已完成请求数',
    FailedRequests INT DEFAULT 0 COMMENT '失败请求数',
    StartTime DATETIME COMMENT '开始时间',
    EndTime DATETIME COMMENT '结束时间',
    CreatedBy VARCHAR(64) COMMENT '创建者',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_status (Status),
    INDEX idx_assigned_agent (AssignedAgentId),
    INDEX idx_expression_id (ExpressionId),
    INDEX idx_created_at (CreatedAt),
    INDEX idx_agent_group (AgentGroupId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='任务表';

-- Task Step Table
CREATE TABLE IF NOT EXISTS task_steps (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    StepId VARCHAR(64) UNIQUE NOT NULL COMMENT '步骤唯一标识',
    TaskId VARCHAR(64) NOT NULL COMMENT '任务ID',
    StepOrder INT NOT NULL COMMENT '步骤顺序',
    StepName VARCHAR(100) NOT NULL COMMENT '步骤名称',
    CollectionMode VARCHAR(64) NOT NULL DEFAULT 'HttpClient' COMMENT '采集模式: HttpClient/Playwright/BrowserAutomation',
    AgentGroupId VARCHAR(64) COMMENT '指定Agent分组',
    RequestConfig JSON NOT NULL COMMENT '请求配置',
    ExtractionRules JSON NOT NULL COMMENT '提取规则',
    VariableMappings JSON COMMENT '变量映射',
    PaginationConfig JSON COMMENT '分页配置',
    OutputConfig JSON COMMENT '输出配置',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_task_id (TaskId),
    INDEX idx_step_order (StepOrder)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='任务步骤表';

-- Task Execution Table
CREATE TABLE IF NOT EXISTS task_executions (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    ExecutionId VARCHAR(64) UNIQUE NOT NULL COMMENT '执行唯一标识',
    TaskId VARCHAR(64) NOT NULL COMMENT '任务ID',
    AgentId VARCHAR(64) NOT NULL COMMENT '执行Agent',
    Status TINYINT DEFAULT 0 COMMENT '状态: 0-待执行, 1-执行中, 2-已完成, 3-失败, 4-已取消',
    ConfigSnapshot JSON COMMENT '配置快照',
    StartedAt DATETIME COMMENT '开始时间',
    CompletedAt DATETIME COMMENT '完成时间',
    TotalPages INT DEFAULT 0 COMMENT '总页数',
    SuccessCount INT DEFAULT 0 COMMENT '成功数',
    FailCount INT DEFAULT 0 COMMENT '失败数',
    ErrorMessage TEXT COMMENT '错误信息',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_task_id (TaskId),
    INDEX idx_agent_id (AgentId),
    INDEX idx_status (Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='任务执行记录表';

-- Task Request Table
CREATE TABLE IF NOT EXISTS task_requests (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    TaskId VARCHAR(64) NOT NULL COMMENT '任务ID',
    RequestId VARCHAR(128) NOT NULL COMMENT '请求唯一标识',
    Url VARCHAR(2048) NOT NULL COMMENT '请求URL',
    Method VARCHAR(16) DEFAULT 'GET' COMMENT '请求方法',
    Headers JSON COMMENT '请求头',
    Body TEXT COMMENT '请求体',
    Status TINYINT DEFAULT 0 COMMENT '状态: 0-待执行, 1-执行中, 2-成功, 3-失败, 4-超时, 5-跳过',
    RetryCount INT DEFAULT 0 COMMENT '重试次数',
    MaxRetry INT DEFAULT 3 COMMENT '最大重试次数',
    Result TEXT COMMENT '响应结果',
    Error VARCHAR(1024) COMMENT '错误信息',
    ErrorType VARCHAR(64) COMMENT '错误类型',
    ErrorCode VARCHAR(32) COMMENT '错误代码',
    Duration INT COMMENT '耗时(毫秒)',
    AssignedAgentId VARCHAR(64) COMMENT '分配的Agent',
    Recovered TINYINT DEFAULT 0 COMMENT '是否已恢复',
    RecoveredAt DATETIME COMMENT '恢复时间',
    ProcessedAt DATETIME COMMENT '处理时间',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_task_status (TaskId, Status),
    INDEX idx_request_id (RequestId),
    INDEX idx_status (Status),
    INDEX idx_agent (AssignedAgentId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='任务请求表';

-- Expression Table
CREATE TABLE IF NOT EXISTS expressions (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    ExpressionId VARCHAR(64) UNIQUE NOT NULL COMMENT '表达式唯一标识',
    Name VARCHAR(128) NOT NULL COMMENT '表达式名称',
    Description VARCHAR(512) COMMENT '表达式描述',
    SelectorType VARCHAR(32) NOT NULL DEFAULT 'XPath' COMMENT '选择器类型: XPath/CssSelector/JsonPath/Regex/Environment',
    ContainerExpression VARCHAR(1024) COMMENT '容器选择表达式',
    Status TINYINT DEFAULT 1 COMMENT '状态: 1-可用, 2-失效, 3-废弃, 4-测试中',
    SuccessCount INT DEFAULT 0 COMMENT '成功次数',
    FailureCount INT DEFAULT 0 COMMENT '失败次数',
    ConsecutiveFailures INT DEFAULT 0 COMMENT '连续失败次数',
    LastValidatedAt DATETIME COMMENT '最后验证时间',
    LastUsedAt DATETIME COMMENT '最后使用时间',
    ExpiredAt DATETIME COMMENT '失效时间',
    CreatedBy VARCHAR(64) COMMENT '创建者',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_status (Status),
    INDEX idx_selector_type (SelectorType),
    INDEX idx_consecutive_failures (ConsecutiveFailures)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='表达式配置表';

-- Expression Field Table
CREATE TABLE IF NOT EXISTS expression_fields (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    FieldId VARCHAR(64) NOT NULL COMMENT '字段唯一标识',
    ExpressionId VARCHAR(64) NOT NULL COMMENT '所属表达式ID',
    FieldName VARCHAR(128) NOT NULL COMMENT '字段名称',
    SelectorType VARCHAR(32) NOT NULL DEFAULT 'XPath' COMMENT '选择器类型',
    Expression VARCHAR(1024) NOT NULL COMMENT '选择表达式',
    AttributeName VARCHAR(128) COMMENT 'HTML属性名',
    IsRequired TINYINT DEFAULT 0 COMMENT '是否必填',
    DefaultValue VARCHAR(256) COMMENT '默认值',
    Formatter VARCHAR(64) COMMENT '格式化器名称',
    FormatterArgs VARCHAR(512) COMMENT '格式化器参数',
    SortOrder INT DEFAULT 0 COMMENT '排序顺序',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_expression_id (ExpressionId),
    INDEX idx_field_name (FieldName)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='表达式字段表';

-- Crawl Result Table
CREATE TABLE IF NOT EXISTS crawl_results (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    ResultId VARCHAR(64) NOT NULL COMMENT '结果唯一标识',
    ExecutionId VARCHAR(64) NOT NULL COMMENT '执行ID',
    TaskId VARCHAR(64) NOT NULL COMMENT '任务ID',
    StepId VARCHAR(64) COMMENT '步骤ID',
    ExtractedData JSON NOT NULL COMMENT '提取数据',
    SourceUrl VARCHAR(2000) COMMENT '来源URL',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_task_id (TaskId),
    INDEX idx_execution_id (ExecutionId),
    INDEX idx_step_id (StepId),
    INDEX idx_created_at (CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='采集结果表';

-- Collection Result Table (legacy)
CREATE TABLE IF NOT EXISTS collection_results (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    ResultId VARCHAR(64) NOT NULL COMMENT '结果唯一标识',
    TaskId VARCHAR(64) NOT NULL COMMENT '任务ID',
    ExpressionId VARCHAR(64) COMMENT '使用的表达式ID',
    AgentId VARCHAR(64) COMMENT '采集Agent ID',
    SourceUrl VARCHAR(2048) COMMENT '来源URL',
    Fields JSON NOT NULL COMMENT '采集字段数据',
    FieldExpressionMap JSON COMMENT '字段与表达式的映射',
    CollectedAt DATETIME COMMENT '采集时间',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_task_id (TaskId),
    INDEX idx_expression_id (ExpressionId),
    INDEX idx_agent_id (AgentId),
    INDEX idx_collected_at (CollectedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='采集结果表(旧)';

-- Expression Availability Table
CREATE TABLE IF NOT EXISTS expression_availability (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    ExpressionId VARCHAR(64) NOT NULL COMMENT '表达式ID',
    AgentId VARCHAR(64) NOT NULL COMMENT 'Agent ID',
    IsAvailable TINYINT DEFAULT 1 COMMENT '是否可用',
    FailureReason VARCHAR(1024) COMMENT '失败原因',
    LastCheckedAt DATETIME COMMENT '最后检查时间',
    LastSuccessAt DATETIME COMMENT '最后成功时间',
    LastFailureAt DATETIME COMMENT '最后失败时间',
    ConsecutiveFailures INT DEFAULT 0 COMMENT '连续失败次数',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY uk_expression_agent (ExpressionId, AgentId),
    INDEX idx_is_available (IsAvailable),
    INDEX idx_agent_id (AgentId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='表达式可用性表';

-- Config Version Table
CREATE TABLE IF NOT EXISTS config_versions (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    VersionId VARCHAR(64) UNIQUE NOT NULL COMMENT '版本唯一标识',
    TaskId VARCHAR(64) NOT NULL COMMENT '任务ID',
    Version INT NOT NULL COMMENT '版本号',
    ConfigContent JSON NOT NULL COMMENT '配置内容',
    ChangeDescription TEXT COMMENT '变更说明',
    CreatedBy VARCHAR(64) COMMENT '创建者',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_task_id (TaskId),
    INDEX idx_version (TaskId, Version)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='配置版本表';

-- System Log Table
CREATE TABLE IF NOT EXISTS system_logs (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    Level VARCHAR(20) NOT NULL DEFAULT 'Info' COMMENT '日志级别',
    Category VARCHAR(100) NOT NULL COMMENT '分类',
    Message VARCHAR(500) NOT NULL COMMENT '消息',
    Detail JSON COMMENT '详情',
    UserId VARCHAR(64) COMMENT '用户ID',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_level (Level),
    INDEX idx_category (Category),
    INDEX idx_created_at (CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='系统日志表';

-- User Table
CREATE TABLE IF NOT EXISTS users (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    UserId VARCHAR(64) UNIQUE NOT NULL COMMENT '用户唯一标识',
    Username VARCHAR(128) UNIQUE NOT NULL COMMENT '用户名',
    PasswordHash VARCHAR(256) NOT NULL COMMENT '密码哈希',
    Role VARCHAR(64) NOT NULL DEFAULT 'Operator' COMMENT '角色: Admin/Operator/Viewer',
    Status TINYINT DEFAULT 1 COMMENT '状态: 0-禁用, 1-启用',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='用户表';

-- Proxy Table
CREATE TABLE IF NOT EXISTS proxies (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    ProxyId VARCHAR(64) UNIQUE NOT NULL COMMENT '代理唯一标识',
    Host VARCHAR(255) NOT NULL COMMENT '代理地址',
    Port INT NOT NULL COMMENT '代理端口',
    Protocol VARCHAR(16) DEFAULT 'HTTP' COMMENT '协议类型',
    Username VARCHAR(128) COMMENT '用户名',
    Password VARCHAR(256) COMMENT '密码',
    Status TINYINT DEFAULT 0 COMMENT '状态: 0-未检测, 1-可用, 2-不可用',
    SuccessCount INT DEFAULT 0 COMMENT '成功次数',
    FailureCount INT DEFAULT 0 COMMENT '失败次数',
    LastCheckedAt DATETIME COMMENT '最后检测时间',
    NextCheckAt DATETIME COMMENT '下次检测时间',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_status (Status),
    INDEX idx_next_check (NextCheckAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='代理表';

-- Statistics Table
CREATE TABLE IF NOT EXISTS statistics (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    AgentId VARCHAR(64) NOT NULL COMMENT 'Agent ID',
    StatDate DATE NOT NULL COMMENT '统计日期',
    TotalRequests INT DEFAULT 0 COMMENT '总请求数',
    SuccessRequests INT DEFAULT 0 COMMENT '成功请求数',
    FailedRequests INT DEFAULT 0 COMMENT '失败请求数',
    AvgDuration DECIMAL(10,2) COMMENT '平均耗时',
    DataVolume BIGINT DEFAULT 0 COMMENT '数据量',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uk_agent_date (AgentId, StatDate),
    INDEX idx_stat_date (StatDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='统计表';

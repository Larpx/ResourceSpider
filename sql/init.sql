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
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_status (Status),
    INDEX idx_last_heartbeat (LastHeartbeat)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Agent信息表';

-- Task Table
CREATE TABLE IF NOT EXISTS tasks (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    TaskId VARCHAR(64) UNIQUE NOT NULL COMMENT '任务唯一标识',
    TaskName VARCHAR(256) NOT NULL COMMENT '任务名称',
    TaskType VARCHAR(64) NOT NULL COMMENT '任务类型',
    Priority INT DEFAULT 5 COMMENT '优先级: 1-10',
    Status TINYINT DEFAULT 0 COMMENT '状态: 0-待执行, 1-执行中, 2-已完成, 3-失败, 4-暂停',
    RequestConfig JSON NOT NULL COMMENT '请求配置',
    ScheduleConfig JSON COMMENT '调度配置',
    RetryPolicy JSON COMMENT '重试策略',
    AssignedAgentId VARCHAR(64) COMMENT '分配的Agent',
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
    INDEX idx_created_at (CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='任务表';

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

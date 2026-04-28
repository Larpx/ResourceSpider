# ResourceSpider

ResourceSpider 是一个轻量、灵活、高性能、跨平台的分布式网络爬虫框架，采用 Agent-Server 架构，支持本地模式和分布式模式。

## 功能特性

- **分布式架构**: Agent-Server 架构，支持多节点协同采集
- **双模式运行**: 支持本地模式（独立运行）和在线模式（连接服务端），灵活切换
- **多种采集方式**: HttpClient、Playwright 无头浏览器、Chrome DevTools Protocol
- **数据解析**: XPath、CSS 选择器、JSON Path、正则表达式解析器
- **表达式系统**: 可配置的提取表达式，支持字段级提取规则和格式化器
- **多步骤任务**: 支持多步骤采集任务，步骤间变量传递和分页处理
- **灵活存储**: 支持 CSV、TXT、JSON 文件存储和数据库存储
- **代理池**: 内置代理池管理，支持代理检测、健康评分和轮换
- **任务调度**: 广度优先/深度优先调度器，支持请求去重（HashSet/布隆过滤器/Redis）
- **消息队列**: 可切换内存/RabbitMQ 消息队列
- **RESTful API**: 完整的任务管理、Agent 管理、表达式管理、统计接口
- **Web 管理界面**: Blazor Web UI 提供可视化的任务管理、Agent 监控、数据统计
- **实时通信**: SignalR 支持 Agent 实时任务分配和控制指令
- **安全认证**: JWT Bearer Token 认证，BCrypt 密码哈希
- **配置版本**: 任务配置版本管理，支持回滚
- **健康检查**: 数据库、Redis 连接健康监控
- **运行时监控**: 实时监控 Agent 运行状态、任务执行进度、系统资源使用
- **容器化部署**: 提供 Docker 和 docker-compose 配置

## 技术栈

| 组件 | 技术选型 |
|------|----------|
| 运行时 | .NET 10.0 |
| 服务端框架 | ASP.NET Core |
| Web UI | Blazor Server |
| ORM | SqlSugar.Core |
| 缓存 | StackExchange.Redis |
| 消息队列 | MassTransit + RabbitMQ / InMemory |
| HTTP 客户端 | IHttpClientFactory |
| 无头浏览器 | Playwright.NET |
| HTML 解析 | HtmlAgilityPack + AngleSharp |
| JSON 处理 | System.Text.Json + Newtonsoft.Json |
| JSONPath | JsonPath.Net |
| 日志 | Serilog |
| 认证 | JWT Bearer + BCrypt |
| 实时通信 | SignalR + MessagePack |
| API 文档 | NSwag (OpenAPI/Swagger) |
| 健康检查 | AspNetCore.HealthChecks |
| 数据库 | MySQL 8.0+ |
| 测试框架 | xUnit + Shouldly + Moq |

## 项目结构

```
ResourceSpider/
├── src/
│   ├── ResourceSpider.Core/              # 核心层：接口、模型、枚举、常量、异常
│   │   ├── DataFlow/                     # 数据流管道（上下文、接口、选项）
│   │   ├── Enums/                        # 枚举定义（任务状态、采集模式等）
│   │   ├── Exceptions/                   # 异常体系
│   │   ├── Interfaces/                   # 核心接口（下载器、解析器、调度器等）
│   │   ├── Models/                       # 数据模型（任务、请求、响应、代理等）
│   │   ├── Selector/                     # 选择器接口定义
│   │   └── Constants.cs                  # 全局常量（API路由、默认值等）
│   │
│   ├── ResourceSpider.Infrastructure/    # 基础设施层：核心接口的实现
│   │   ├── DataFlow/                     # 数据流管道实现（解析器、存储、格式化器）
│   │   ├── Downloader/                   # 下载器（HttpClient、Playwright、CDP）
│   │   ├── Duplicate/                    # 去重器（HashSet、布隆过滤器、Redis）
│   │   ├── HtmlAgilityPack.Css/          # CSS 选择器引擎
│   │   ├── MessageQueue/                 # 消息队列（内存、RabbitMQ）
│   │   ├── Parser/                       # 解析器（XPath、CSS、JSON、表达式驱动）
│   │   ├── Proxy/                        # 代理池（验证、检测、健康评分）
│   │   ├── Scheduler/                    # 调度器（BFS、DFS）
│   │   ├── Selector/                     # 选择器实现（XPath、CSS、JSON、Regex）
│   │   ├── Spider/                       # 爬虫框架（Spider基类、构建器）
│   │   ├── Storage/                      # 存储（文件CSV/TXT/JSON、数据库）
│   │   └── Utils/                        # 工具（哈希轮定时器、请求指纹）
│   │
│   ├── ResourceSpider.Server/            # 服务端：ASP.NET Core Web API + Blazor UI
│   │   ├── Components/                   # Blazor 组件
│   │   │   ├── Layout/                   # 布局组件（AdminLayout、MainLayout）
│   │   │   ├── Pages/                    # 页面组件（Dashboard、Tasks、Agents等）
│   │   │   └── Services/                 # Blazor 服务（API客户端、通知、状态管理）
│   │   ├── Controllers/                  # API 控制器（14个）
│   │   ├── DTOs/                         # 数据传输对象
│   │   ├── Entities/                     # 数据库实体（SqlSugar）
│   │   ├── Filters/                      # 过滤器（ApiResponse自动包装）
│   │   ├── Hubs/                         # SignalR Hub
│   │   ├── Middleware/                   # 中间件（限流、安全头、异常处理）
│   │   ├── Observability/                # 可观测性（健康检查、运行时监控）
│   │   ├── Repositories/                 # 数据仓储层（15个）
│   │   └── Services/                     # 业务服务层（12个）
│   │
│   ├── ResourceSpider.Agent/             # Agent：采集执行节点
│   │   ├── Config/                       # 配置选项（本地/在线模式）
│   │   ├── Modes/                        # 运行模式（本地/在线）
│   │   └── Services/                     # 服务（任务执行、结果上报、SignalR、API客户端）
│   │
│   ├── ResourceSpider.Tests.Unit/        # 单元测试
│   └── ResourceSpider.Tests.Integration/ # 集成测试
│
├── deploy/
│   ├── Dockerfile.Server                 # 服务端 Docker 镜像
│   ├── Dockerfile.Agent                  # Agent Docker 镜像
│   └── docker-compose.yml               # Docker Compose 编排
│
├── sql/
│   └── init.sql                          # MySQL 数据库初始化脚本
│
└── Doc/
    └── 分布式爬虫系统需求文档（整合版）.md
```

## 快速开始

### 前置条件

- .NET 10.0 SDK
- MySQL 8.0+
- Redis 7.0+（可选，用于分布式去重和缓存）
- Docker & Docker Compose（可选，用于容器化部署）

### 本地开发

1. 克隆仓库
   ```bash
   git clone <repository-url>
   cd ResourceSpider
   ```

2. 还原 NuGet 包
   ```bash
   dotnet restore
   ```

3. 初始化数据库
   ```bash
   mysql -u root -p < sql/init.sql
   ```

4. 启动服务端
   ```bash
   cd src/ResourceSpider.Server
   dotnet run
   ```
   服务端默认监听 `http://localhost:5000`

   访问以下地址：
   - **Web 管理界面**: http://localhost:5000
   - **API 文档 (Swagger)**: http://localhost:5000/swagger
   - **健康检查**: http://localhost:5000/health

5. 启动 Agent（本地模式）
   ```bash
   cd src/ResourceSpider.Agent
   dotnet run
   ```
   Agent 默认以本地模式运行，扫描 `./tasks` 目录下的 JSON 任务文件

### Docker 部署

```bash
cd deploy
docker-compose up -d
```

服务包含：
- **mysql**: MySQL 8.0 数据库
- **redis**: Redis 7 缓存
- **rabbitmq**: RabbitMQ 消息队列（含管理界面）
- **server**: 服务端 API + Web UI
- **agent**: 采集 Agent

访问地址：
- **Web 管理界面**: http://localhost:5000
- **API 文档**: http://localhost:5000/swagger
- **健康检查**: http://localhost:5000/health
- **RabbitMQ 管理界面**: http://localhost:15672 (guest/guest)

## 配置说明

### 服务端配置 (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ResourceSpider;Uid=root;Pwd=root;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long",
    "ExpiryHours": 24
  },
  "MessageQueue": {
    "Type": "InMemory"
  },
  "Downloader": {
    "ConnectionTimeout": 30,
    "RequestTimeout": 60,
    "MaxConcurrentRequests": 10,
    "RetryCount": 3,
    "RetryDelayMs": 1000
  },
  "Playwright": {
    "BrowserType": "Chromium",
    "Headless": true,
    "MaxInstances": 5,
    "MaxLifetimeMinutes": 30
  }
}
```

### Agent 配置 (appsettings.json)

```json
{
  "Agent": {
    "Mode": "Local",
    "LocalConfig": {
      "TaskFilePath": "./tasks",
      "ResultOutputPath": "./results",
      "OutputFormat": "csv",
      "MaxConcurrentTasks": 5
    },
    "ServerConfig": {
      "ServerUrl": "http://localhost:5000",
      "AgentId": "agent-001",
      "AgentName": "Agent 001",
      "AgentToken": "",
      "HeartbeatInterval": 30
    }
  }
}
```

**模式说明**：
- `Local`: 本地模式，扫描本地任务文件执行，结果存储到本地文件
- `Online`: 在线模式，连接服务端拉取任务，通过 SignalR 接收实时指令

## Web 管理界面

ResourceSpider 提供基于 Blazor Server 的 Web 管理界面，支持以下功能：

### 主要页面

| 页面 | 功能 |
|------|------|
| 仪表盘 | 系统概览、Agent 状态、任务统计、实时监控 |
| 任务管理 | 任务创建、编辑、执行、暂停、恢复、删除、配置版本管理 |
| Agent 管理 | Agent 列表、状态监控、分组管理、在线状态 |
| 表达式管理 | 表达式配置、字段提取规则、可用性检测 |
| 代理管理 | 代理池管理、健康检测、代理测试 |
| 运行时监控 | 实时日志、任务执行进度、系统资源监控 |
| 系统设置 | 用户管理、系统配置、日志查看 |

### 实时功能

- **实时监控**: 通过 SignalR 实时推送 Agent 状态、任务进度、系统日志
- **实时日志**: Web 界面实时显示服务端和 Agent 运行日志
- **实时通知**: 任务完成、Agent 上线/下线、异常告警等通知

## API 接口

### 认证

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/auth/login | POST | 用户登录 |
| /api/auth/register | POST | 用户注册 |

### Agent 通信

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/agent/register | POST | Agent 注册 |
| /api/agent/heartbeat | POST | 心跳上报 |
| /api/agent/unregister | POST | Agent 注销 |
| /api/agent/tasks/pull | POST | 拉取待执行任务 |
| /api/agent/tasks/report | POST | 上报任务结果 |
| /api/agent/expressions/pull | POST | 拉取表达式配置 |
| /api/agent/expressions/active | POST | 拉取活跃表达式 |
| /api/agent/expressions/availability | POST | 上报表达式可用性 |
| /api/agent/results/store | POST | 存储采集结果 |

### 任务管理

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/tasks | POST | 创建任务 |
| /api/tasks | GET | 任务列表（分页） |
| /api/tasks/{taskId} | GET | 任务详情 |
| /api/tasks/{taskId} | PUT | 更新任务 |
| /api/tasks/{taskId} | DELETE | 删除任务 |
| /api/tasks/{taskId}/pause | PUT | 暂停任务 |
| /api/tasks/{taskId}/resume | PUT | 恢复任务 |
| /api/tasks/{taskId}/stop | PUT | 停止任务 |
| /api/tasks/{taskId}/execute | POST | 触发执行 |
| /api/tasks/{taskId}/executions | GET | 执行记录 |
| /api/tasks/{taskId}/config-versions | GET | 配置版本 |

### 表达式管理

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/expressions | POST | 创建表达式 |
| /api/expressions | GET | 表达式列表 |
| /api/expressions/{id} | GET | 表达式详情 |
| /api/expressions/{id} | PUT | 更新表达式 |
| /api/expressions/{id} | DELETE | 删除表达式 |
| /api/expressions/{id}/config | GET | 获取配置 |
| /api/expressions/invalidate-expired | POST | 失效过期表达式 |

## 代理管理

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/admin/proxies | GET | 代理列表 |
| /api/admin/proxies | POST | 添加代理 |
| /api/admin/proxies/{proxyId} | DELETE | 删除代理 |
| /api/admin/proxies/test | POST | 代理测试 |

### 数据统计

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/statistics/agent | GET | Agent 统计 |
| /api/statistics/task/{taskId} | GET | 任务统计 |
| /api/statistics/system | GET | 系统统计 |
| /api/statistics/trend | GET | 趋势数据 |

### 采集结果

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/results | GET | 结果列表 |
| /api/results/{id} | GET | 结果详情 |
| /api/results/export | POST | 导出结果（CSV/JSON/Excel） |

## 运行测试

```bash
# 运行所有测试
dotnet test

# 仅运行单元测试
dotnet test src/ResourceSpider.Tests.Unit

# 仅运行集成测试
dotnet test src/ResourceSpider.Tests.Integration
```

## 环境变量

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `ConnectionStrings__DefaultConnection` | MySQL 连接字符串 | - |
| `ConnectionStrings__Redis` | Redis 连接字符串 | localhost:6379 |
| `Jwt__Secret` | JWT 密钥 | - |
| `Agent__Mode` | Agent 运行模式 | Local |
| `Agent__ServerConfig__ServerUrl` | 服务端地址 | http://localhost:5000 |

## 健康检查

系统提供健康检查接口，用于监控服务状态：

| 端点 | 说明 |
|------|------|
| /health | 综合健康检查 |
| /health/ready | 就绪检查 |
| /health/live | 存活检查 |

健康检查包括：
- 数据库连接状态
- Redis 连接状态
- 系统资源使用情况

## 监控与日志

### 日志系统

- **服务端日志**: `logs/server-<date>.txt`
- **Agent 日志**: `logs/agent-<date>.txt`
- **日志级别**: Debug / Information / Warning / Error
- **日志轮转**: 按天轮转，保留 30 天

### 运行时监控

通过 SignalR 实时推送以下监控数据：

- **Agent 状态**: 在线/离线、心跳时间、执行任务数
- **任务进度**: 执行中、已完成、失败的任务数量
- **系统资源**: CPU 使用率、内存使用量、网络流量
- **实时日志**: 服务端和 Agent 的实时日志流

### Web 界面监控

访问 Web 管理界面的"运行时监控"页面，可以：
- 查看所有 Agent 的实时状态
- 监控任务执行进度
- 查看实时日志流
- 查看系统资源使用情况

## 架构设计

### 系统整体架构

```
┌─────────────────────────────────────────────────────────────┐
│                     Web 管理界面 (Blazor)                     │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │ Dashboard│  │  Tasks   │  │  Agents  │  │  Monitor  │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
└───────────────────────────┬─────────────────────────────────┘
                            │ SignalR + REST API
┌───────────────────────────┴─────────────────────────────────┐
│                      ResourceSpider Server                    │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │Controllers│  │ Services │  │Repositories│  │ SignalR Hub│ │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
└───────────────────────────┬─────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
   ┌────▼────┐        ┌────▼────┐        ┌────▼────┐
   │  MySQL  │        │  Redis  │        │RabbitMQ │
   └─────────┘        └─────────┘        └─────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
   ┌────▼────┐        ┌────▼────┐        ┌────▼────┐
   │ Agent 1 │        │ Agent 2 │        │ Agent N │
   └─────────┘        └─────────┘        └─────────┘
```

### Agent-Server 通信流程

```
┌─────────┐    注册/心跳     ┌─────────┐
│  Agent  │ ──────────────→ │  Server │
│         │ ←────────────── │         │
│         │   任务分配/控制   │         │
│         │                 │         │
│  SignalR│ ←────────────── │  SignalR│
│  Client │   实时消息推送    │  Hub    │
│         │ ──────────────→ │         │
│         │   结果上报       │         │
└─────────┘                 └─────────┘
```

### 数据流管道

```
Request → Scheduler → Downloader → Parser → Storage
              ↑           ↓
         Deduplicator  Response
```

### 任务执行流程

```
SpiderTask
├── 单步任务: 提取请求 → 调度 → 下载 → 解析 → 存储
└── 多步任务: 步骤1 → 变量映射 → 步骤2 → ... → 存储
                  ↑                    ↑
            分页处理              表达式配置
```

### 监控数据流

```
┌─────────┐    日志/状态      ┌─────────┐    SignalR     ┌──────────┐
│  Agent  │ ──────────────→ │  Server │ ───────────→ │   Web UI │
└─────────┘                 └─────────┘              └──────────┘
     │                           │
     │                           │
     └─────── RuntimeOutputBroadcastService ──────┘
```

## 许可证

MIT License

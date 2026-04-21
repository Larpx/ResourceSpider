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
- **实时通信**: SignalR 支持 Agent 实时任务分配和控制指令
- **安全认证**: JWT Bearer Token 认证，BCrypt 密码哈希
- **配置版本**: 任务配置版本管理，支持回滚
- **容器化部署**: 提供 Docker 和 docker-compose 配置

## 技术栈

| 组件 | 技术选型 |
|------|----------|
| 运行时 | .NET 10.0 |
| 服务端框架 | ASP.NET Core |
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
│   ├── ResourceSpider.Server/            # 服务端：ASP.NET Core Web API
│   │   ├── Controllers/                  # API 控制器（14个）
│   │   ├── DTOs/                         # 数据传输对象
│   │   ├── Entities/                     # 数据库实体（SqlSugar）
│   │   ├── Filters/                      # 过滤器（ApiResponse自动包装）
│   │   ├── Hubs/                         # SignalR Hub
│   │   ├── Middleware/                   # 中间件（限流、安全头、异常处理）
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
- **server**: 服务端 API
- **agent**: 采集 Agent

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

### 代理管理

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/proxies | GET | 代理列表 |
| /api/proxies | POST | 添加代理 |
| /api/proxies/{proxyId} | DELETE | 删除代理 |
| /api/proxies/test | POST | 代理测试 |

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

## 架构设计

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

## 许可证

MIT License

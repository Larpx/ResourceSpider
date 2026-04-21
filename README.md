# ResourceSpider

ResourceSpider 是一个轻量、灵活、高性能、跨平台的分布式网络爬虫框架，采用 Agent-Server 架构，支持本地模式和分布式模式。

## 功能特性

- **分布式架构**: Agent-Server 架构，支持多节点协同采集
- **双模式运行**: 支持本地模式和在线模式，灵活切换
- **多种采集方式**: HTTP 客户端、Playwright 无头浏览器
- **数据解析**: XPath、CSS 选择器、JSON Path 解析器
- **灵活存储**: 支持 CSV、TXT、JSON 文件存储和数据库存储
- **代理池**: 内置代理池管理，支持代理检测和轮换
- **任务调度**: 广度优先/深度优先调度器，支持请求去重
- **消息队列**: 可切换内存/RabbitMQ 消息队列
- **RESTful API**: 完整的任务管理、Agent 管理、统计接口
- **容器化部署**: 提供 Docker 和 docker-compose 配置

## 技术栈

| 组件 | 技术选型 |
|------|----------|
| 服务端框架 | ASP.NET Core 11.0 |
| ORM | SqlSugar.Core |
| 缓存 | StackExchange.Redis |
| 消息队列 | MassTransit + RabbitMQ/InMemory |
| HTTP 客户端 | HttpClient |
| 无头浏览器 | Playwright.NET |
| 日志 | Serilog |
| 认证 | JWT Bearer |
| 测试框架 | xUnit + FluentAssertions + Moq |

## 项目结构

```
ResourceSpider/
├── src/
│   ├── ResourceSpider.Core/           # 核心接口和模型
│   ├── ResourceSpider.Infrastructure/ # 基础设施实现
│   ├── ResourceSpider.Server/         # 服务端 API
│   ├── ResourceSpider.Agent/          # Agent 采集框架
│   ├── ResourceSpider.Tests.Unit/     # 单元测试
│   └── ResourceSpider.Tests.Integration/ # 集成测试
├── deploy/
│   ├── Dockerfile.Server
│   ├── Dockerfile.Agent
│   └── docker-compose.yml
├── sql/
│   └── init.sql
└── Doc/
    ├── 开发计划.md
    └── 需求文档.md
```

## 快速开始

### 前置条件

- .NET 10.0 SDK
- MySQL 8.0+
- Redis 7.0+ (可选)
- Docker & Docker Compose (可选)

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

3. 启动服务端
   ```bash
   cd src/ResourceSpider.Server
   dotnet run
   ```

4. 启动 Agent
   ```bash
   cd src/ResourceSpider.Agent
   dotnet run
   ```

### Docker 部署

```bash
cd deploy
docker-compose up -d
```

## 配置说明

### 服务端配置 (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ResourceSpider;Uid=root;Pwd=root;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Secret": "your-secret-key",
    "ExpiryHours": 24
  }
}
```

### Agent 配置

```json
{
  "Agent": {
    "Mode": "Local",
    "LocalConfig": {
      "TaskFilePath": "./tasks",
      "ResultOutputPath": "./results",
      "OutputFormat": "csv"
    },
    "ServerConfig": {
      "ServerUrl": "http://localhost:5000",
      "AgentId": "agent-001",
      "HeartbeatInterval": 30
    }
  }
}
```

## API 接口

### Agent 管理

| 接口 | 方法 | 说明 |
|------|------|------|
| POST /api/agent/register | POST | Agent 注册 |
| POST /api/agent/heartbeat | POST | 心跳上报 |
| POST /api/agent/unregister | POST | Agent 注销 |

### 任务管理

| 接口 | 方法 | 说明 |
|------|------|------|
| POST /api/tasks | POST | 创建任务 |
| GET /api/tasks | GET | 任务列表 |
| GET /api/tasks/{taskId} | GET | 任务详情 |
| PUT /api/tasks/{taskId}/pause | PUT | 暂停任务 |
| PUT /api/tasks/{taskId}/resume | PUT | 恢复任务 |
| DELETE /api/tasks/{taskId} | DELETE | 删除任务 |

### 代理管理

| 接口 | 方法 | 说明 |
|------|------|------|
| GET /api/proxies | GET | 代理列表 |
| POST /api/proxies | POST | 添加代理 |
| DELETE /api/proxies/{proxyId} | DELETE | 删除代理 |
| POST /api/proxies/test | POST | 代理测试 |

### 数据统计

| 接口 | 方法 | 说明 |
|------|------|------|
| GET /api/statistics/agent | GET | Agent 统计 |
| GET /api/statistics/task/{taskId} | GET | 任务统计 |
| GET /api/statistics/system | GET | 系统统计 |
| GET /api/statistics/trend | GET | 趋势数据 |

## 运行测试

```bash
dotnet test
```

## 环境变量

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `DB_CONNECTION_STRING` | MySQL 连接字符串 | - |
| `REDIS_CONNECTION_STRING` | Redis 连接字符串 | localhost:6379 |
| `JWT_SECRET` | JWT 密钥 | - |
| `LOG_LEVEL` | 日志级别 | Information |

## 许可证

MIT License

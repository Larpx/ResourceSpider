# ResourceSpider

[中文](README.md) | [English](README.en.md)

**文档**：[用户使用说明](docs/用户使用说明.md) · [开发说明](docs/开发说明.md) · [需求文档](docs/分布式爬虫系统需求文档（整合版）.md)

ResourceSpider 是一个轻量、灵活、跨平台的分布式网络爬虫框架，采用 Agent–Server 架构，支持本机独立运行与多节点协同采集。

## 能做什么

- 分布式采集：服务端统一调度，多 Agent 协同
- 双模式：本地任务文件 / 连接服务端在线执行
- 多种采集方式：普通 HTTP、无头浏览器、Chrome DevTools Protocol
- 灵活解析与多步骤任务、文件或数据库存储
- Web 管理界面与实时监控

## 程序怎样工作（简要）

1. 启动服务端（管理界面 + 接口）。
2. 创建或配置采集任务。
3. 启动 Agent 执行任务，并在界面查看进度与结果。

## 快速开始

```bash
git clone https://gitee.com/DLarpx/ResourceSpider.git
cd ResourceSpider
dotnet restore
mysql -u root -p < mysql/init.sql
dotnet run --project src/ResourceSpider.Server
```

另开终端：

```bash
dotnet run --project src/ResourceSpider.Agent
```

Docker：`cd deploy && docker compose up -d`

更多步骤与配置见 [用户使用说明](docs/用户使用说明.md) / [开发说明](docs/开发说明.md)。

## 项目说明

| 项 | 说明 |
|----|------|
| 产品名 | ResourceSpider |
| 默认命名空间 | `Larpx.PersonalTools.ResourceSpider.*` |
| 解决方案 | `ResourceSpider.sln` |
| 主要目录 | `src/` 源码 · `docs/` 文档 · `scripts/` 脚本 · `mysql/` 库脚本 · `deploy/` 部署 · `Models/` 模型 |

## License

[MIT License](LICENSE)

# ResourceSpider

[中文](README.md) | [English](README.en.md)

**Docs**: [User Guide (中文)](docs/用户使用说明.md) · [Developer Guide (中文)](docs/开发说明.md) · [Requirements (中文)](docs/分布式爬虫系统需求文档（整合版）.md)

ResourceSpider is a lightweight, cross-platform distributed web crawling framework with an Agent–Server architecture. It supports standalone local runs and multi-node collaboration.

## What it does

- Distributed crawling with a central server and multiple agents
- Local mode (task files) and online mode (connected to the server)
- Multiple download approaches and flexible parsing / multi-step tasks
- File or database storage, plus a web admin UI with live monitoring

## How it works (3 steps)

1. Start the server (admin UI + APIs).
2. Create or configure crawl tasks.
3. Start agents to execute tasks and monitor progress in the UI.

## Quick start

```bash
git clone https://gitee.com/DLarpx/ResourceSpider.git
cd ResourceSpider
dotnet restore
mysql -u root -p < mysql/init.sql
dotnet run --project src/ResourceSpider.Server
```

In another terminal:

```bash
dotnet run --project src/ResourceSpider.Agent
```

Docker: `cd deploy && docker compose up -d`

## Project notes

| Item | Value |
|------|--------|
| Product | ResourceSpider |
| Default namespace | `Larpx.PersonalTools.ResourceSpider.*` |
| Solution | `ResourceSpider.sln` |
| Layout | `src/` · `docs/` · `scripts/` · `mysql/` · `deploy/` · `Models/` |

## License

[MIT License](LICENSE)

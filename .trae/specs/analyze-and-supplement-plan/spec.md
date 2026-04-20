# 开发计划分析与补充 Spec

## Why

当前开发计划文档已经非常全面，涵盖了架构设计、API接口、数据库设计、安全机制、异常处理等核心内容。但经过深入分析，仍存在一些关键领域需要进一步细化和补充，特别是：

1. 解析器实现规范（XPath/CSS选择器）尚未详细定义
2. 免费NuGet包的具体选型和限制未明确
3. 数据清洗与转换流程缺失
4. Portal后台管理的用户权限体系未涉及
5. 任务模板系统和复用机制未定义
6. 配置文件的完整结构未标准化

## What Changes

### 新增内容

- **1.6 数据解析器规范**：XPath、CSS选择器、自定义解析器的接口设计和使用示例
- **1.7 免费技术栈约束**：明确所有使用的NuGet包必须是免费的，提供具体的免费包清单
- **1.8 配置文件标准结构**：定义appsettings.json的完整结构和各配置项说明
- **十五、数据清洗与转换**：新增章节，定义数据预处理、格式化、验证规则
- **十六、Portal后台管理系统**：新增章节，包含用户权限、角色管理、操作审计
- **十七、任务模板系统**：新增章节，支持任务配置的保存、复用和共享
- **十八、API版本控制**：新增章节，定义API版本管理策略
- **十九、限流与配额管理**：新增章节，细化请求频率控制和资源分配

## Impact

- Affected specs: 技术栈要求(1.4)、Agent采集方法(1.3)、核心模块设计(二)
- Affected code: Agent解析模块、服务端中间件、Portal前端页面

## ADDED Requirements

### Requirement: 数据解析器支持

The system SHALL provide built-in support for XPath and CSS selectors for HTML/XML data extraction.

#### Scenario: 使用XPath提取数据
- **WHEN** 用户在任务配置中选择XPath解析器
- **THEN** 系统应能通过XPath表达式从HTML响应中提取指定字段的数据

#### Scenario: 使用CSS选择器提取数据
- **WHEN** 用户在任务配置中选择CSS选择器解析器
- **THEN** 系统应能通过CSS选择器表达式从HTML响应中提取指定元素的数据

#### Scenario: 自定义解析器注册
- **WHEN** 开发者实现了IParser接口的自定义解析器
- **THEN** 系统应能通过依赖注入自动注册该解析器并在任务中使用

### Requirement: 免费NuGet包约束

All NuGet packages used in the project MUST be free and open-source (MIT/Apache 2.0 license).

#### Scenario: 验证包许可证
- **WHEN** 添加新的NuGet包到项目
- **THEN** 该包必须具有兼容的开源许可证，不得包含商业许可或付费功能

#### Scenario: 替代付费方案
- **WHEN** 需要的功能只有付费包提供
- **THEN** 必须寻找开源替代方案或自行实现该功能

### Requirement: Portal用户权限管理

The system SHALL implement role-based access control (RBAC) for the Portal admin panel.

#### Scenario: 角色分配
- **WHEN** 管理员创建新用户
- **THEN** 可以为用户分配不同角色（管理员、普通用户、只读用户）

#### Scenario: 操作审计
- **WHEN** 用户执行敏感操作（删除任务、修改配置）
- **THEN** 系统应记录操作日志，包括操作人、时间、操作内容和IP地址

### Requirement: 任务模板系统

The system SHALL allow users to create reusable task templates.

#### Scenario: 创建任务模板
- **WHEN** 用户完成一个复杂的任务配置
- **THEN** 可以将此配置保存为模板供后续快速创建类似任务

#### Scenario: 应用任务模板
- **WHEN** 用户选择一个已存在的任务模板
- **THEN** 系统应自动填充任务配置，允许用户在此基础上修改

## MODIFIED Requirements

### Requirement: 技术栈细化（1.4）

更新技术栈表格，增加"许可证"列，明确标注每个库的开源协议。

### Requirement: 核心模块设计（二）

在DataFlow模块描述中，增加对内置解析器（XPathParser、CssSelectorParser）的详细说明。

## REMOVED Requirements

无

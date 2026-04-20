# Tasks

- [ ] Task 1: 分析开发计划文档，识别缺失内容并整理补充清单
  - [ ] SubTask 1.1: 审查技术栈部分，识别免费NuGet包约束的缺失
  - [ ] SubTask 1.2: 审查解析器支持要求（XPath/CSS选择器），确认是否需要详细规范
  - [ ] SubTask 1.3: 审查Portal后台管理功能，检查用户权限体系是否缺失
  - [ ] SubTask 1.4: 审查配置管理部分，检查配置文件结构定义是否完整
  - [ ] SubTask 1.5: 审查数据处理流程，检查数据清洗转换规范是否缺失

- [ ] Task 2: 编写数据解析器规范章节（1.6）
  - [ ] SubTask 2.1: 定义IParser接口设计
  - [ ] SubTask 2.2: 编写XPath解析器使用示例和最佳实践
  - [ ] SubTask 2.3: 编写CSS选择器解析器使用示例
  - [ ] SubTask 2.4: 定义自定义解析器注册机制

- [ ] Task 3: 编写免费技术栈约束章节（1.7）
  - [ ] SubTask 3.1: 列出所有使用的NuGet包及其许可证信息
  - [ ] SubTask 3.2: 标注可能的付费替代方案
  - [ ] SubTask 3.3: 编写许可证验证规则

- [ ] Task 4: 编写配置文件标准结构章节（1.8）
  - [ ] SubTask 4.1: 定义Agent端appsettings.json结构
  - [ ] SubTask 4.2: 定义Server端appsettings.json结构
  - [ ] SubTask 4.3: 定义环境变量与配置文件的优先级关系

- [ ] Task 5: 编写数据清洗与转换章节（十五）
  - [ ] SubTask 5.1: 定义数据预处理规则（空值处理、格式化、编码转换）
  - [ ] SubTask 5.2: 定义数据验证规则（类型校验、范围校验、正则校验）
  - [ ] SubTask 5.3: 定义数据转换规则（字段映射、值映射、聚合计算）

- [ ] Task 6: 编写Portal后台管理系统章节（十六）
  - [ ] SubTask 6.1: 定义用户角色和权限矩阵
  - [ ] SubTask 6.2: 定义操作审计日志规范
  - [ ] SubTask 6.3: 定义登录认证和会话管理

- [ ] Task 7: 编写任务模板系统章节（十七）
  - [ ] SubTask 7.1: 定义任务模板的数据结构
  - [ ] SubTask 7.2: 定义模板创建、编辑、删除操作
  - [ ] SubTask 7.3: 定义模板共享和导入导出机制

- [ ] Task 8: 编写API版本控制章节（十八）
  - [ ] SubTask 8.1: 定义API版本策略（URL/Header）
  - [ ] SubTask 8.2: 定义版本兼容性和废弃策略

- [ ] Task 9: 编写限流与配额管理章节（十九）
  - [ ] SubTask 9.1: 定义多级限流策略（全局/API/Agent级别）
  - [ ] SubTask 9.2: 定义配额分配和超额处理机制

- [ ] Task 10: 更新技术栈表格，增加许可证列
  - [ ] SubTask 10.1: 更新1.4.1核心框架表格
  - [ ] SubTask 10.2: 补充各库的开源许可证信息

- [ ] Task 11: 更新核心模块设计，补充解析器详细说明
  - [ ] SubTask 11.1: 在DataFlow描述中增加内置解析器说明
  - [ ] SubTask 11.2: 增加解析器接口示例代码

# Task Dependencies

- [Task 2, 3, 4] depends on [Task 1]
- [Task 5, 6, 7] depends on [Task 1]
- [Task 10] depends on [Task 3]
- [Task 11] depends on [Task 2]

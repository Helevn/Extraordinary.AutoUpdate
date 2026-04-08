# 贡献指南（CONTRIBUTING.md）

感谢你想为 Extraordinary.AutoUpdate 做贡献。为确保协作高效、可维护，请在提交 Issue 或 Pull Request 前阅读本文件并遵循其中规则。

## 目录
- 报告 Issue
- 分支与工作流
- 提交信息规范
- 代码风格与格式化
- 本地开发与运行
- 单元测试 与 CI
- Pull Request 要求
- 代码审查 与 合并
- 安全与发布注意事项
- 联系方式

---

## 报告 Issue
请在创建 Issue 前：
1. 搜索现有 Issue，避免重复提交。
2. 描述复现步骤、预期行为与实际行为。
3. 提供环境信息（操作系统、.NET SDK 版本、日志摘录、截图或网络请求样例）。
4. 如涉及 API，附上请求/响应示例。

良好的 Issue 模板示例：
- 标题：简短且描述性
- 描述：重现步骤、期望、实际、日志、环境

---

## 分支与工作流
- 主分支：`master`（始终为可发布状态）。
- 功能分支：`feature/<描述>`（新增功能）；修复分支：`fix/<描述>`。
- 发布分支（可选）：`release/*`。
- 在开始工作前，请从 `master` 拉取最新代码并基于 `master` 创建分支。

---

## 提交信息规范
请使用语义化且简洁的提交信息：

格式：

常用 type：
- `feat`：新特性
- `fix`：修复 bug
- `docs`：文档变更
- `chore`：构建/工具变更
- `refactor`：重构
- `test`：测试相关

---

## 代码风格与格式化
- 目标平台：__.NET 10__，语言：__C# 14__。
- 请遵循仓库根目录的 `.editorconfig`。提交前运行格式化：
  - Visual Studio: 使用 __Format Document__（或快捷键）。
  - CLI: `dotnet format`
- 命名惯例：类型与方法使用 PascalCase，局部变量与参数使用 camelCase。
- 避免未使用的 using、保持每个方法职责单一、控制方法长度。

---

## 本地开发与运行
- 使用 Visual Studio 或 `dotnet` CLI 进行开发。
- 运行服务端：
  - 在项目根目录：`dotnet run --project Extraordinary.WebApi`
  - 或在 Visual Studio 中将 `Extraordinary.WebApi` 设为启动项目并运行。
- 确保 `wwwroot/packages` 存在且可写（上传功能依赖）。

---

## 单元测试 与 CI
- 请为新增功能或修复添加单元测试，放在 `tests/`（或现有测试项目）中。
- CI（如 GitHub Actions）至少应包含：
  - `dotnet build`
  - `dotnet test`
  - 代码格式检查（`dotnet format --verify-no-changes`）
- 所有 PR 必须通过 CI 检查后才能合并。

---

## Pull Request 要求
PR 描述请包含：
- 变更概述
- 关联 Issue（若有）
- 变更的验证步骤（如何本地复现/测试）
- 是否有破坏性变更或需迁移
- 测试覆盖情况

建议在 `.github/PULL_REQUEST_TEMPLATE.md` 提供模板。

---

## 代码审查 与 合并
- PR 需至少一名审阅者通过。
- 审阅重点：功能正确性、错误处理、安全（文件上传必须防止路径遍历）、边界条件、测试覆盖。
- 小而频繁的提交便于审查；合并前请 squash 或按团队约定整理提交。

---

## 安全与发布注意事项
- 上传接口仅允许 `.zip`、`.exe`、`.msi`，并进行 MD5 校验；在生产环境建议添加鉴权与防病毒扫描。
- 发布时请在 CI 中执行安全扫描与依赖漏洞检查。
- 推荐使用 `IHttpClientFactory` 注入 HttpClient，统一控制超时与重试策略。

---

## 联系方式
遇到问题请通过 Issue 提交，或按仓库说明联系维护者。贡献者应遵守友善礼仪与代码行为准则。

---

感谢你的贡献与审阅。若需要，我可以为你生成 PR 模板或把此文件写入仓库并创建一个 commit（可选）。```
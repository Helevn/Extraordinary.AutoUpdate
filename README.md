# Extraordinary.AutoUpdate

一个用于在局域网或公网环境中自动分发与更新 Windows 应用的轻量级方案。包含服务端（文件管理、上传、版本配置生成）与客户端（版本查询、下载、校验、解压、启动）两部分。

## 功能概览

- 服务端：
  - 上传安装包（支持 `.zip`、`.exe`、`.msi`），自动计算 MD5 并以 `版本-MD5` 重命名保存；
  - 列出服务器上的可用包并提供下载接口；
  - 自动生成并保存 `OriginConfig.json`（包含 PackageName、AppVersion、AppMD5Version）。
- 客户端：
  - 查询服务端最新版本信息（优先 `/api/files/version`）；
  - 下载更新包、校验 MD5、解压覆盖并可自动启动程序；
  - 支持进度通知与错误提示。

## 快速开始

### 服务端部署（建议）

1. 部署到 IIS 或使用 Kestrel 运行，确保 `wwwroot` 可写，尤其是 `wwwroot/packages` 目录。
2. 通过管理界面上传安装包，或将包放到 `wwwroot/packages` 并调用生成配置的功能。
3. 确认 `OriginConfig.json` 位于 `wwwroot` 根目录，示例内容：

    ```json
    {
      "PackageName": "YourAppName",
      "AppVersion": "1.0.0",
      "AppMD5Version": "d41d8cd98f00b204e9800998ecf8427e"
    }
    ```

4. 如需跨域访问（不同域名/端口），在服务端启用 CORS。

### 客户端配置示例

在客户端运行目录创建 `LocalConfig.json`（或按你项目约定的位置）：

```json
{
  "PackageName": "YourAppName",
  "ServerUrl": "http://your-server-url"
}
```

工作流程：
- 客户端读取本地配置；
- 访问 `GET {ServerUrl}/api/files/version` 获取最新版本信息（失败回退到 `/OriginConfig.json`）；
- 若版本或 MD5 不一致，下载 `PackageName`，校验 MD5，解压并替换安装目录；
- 根据 `Self_Starting` 与 `Kill_App` 配置停止并重启程序。

## 后端 API（摘要）

- `GET /api/files/list`  
  返回服务器包列表（JSON 数组）。元素示例：  
  `{ "Name": "MyApp-1.0.2-<md5>.zip", "Size": "48.7 MB", "Modified": "2023-10-26 14:28:15" }`

- `GET /api/files/download/{fileName}`  
  下载指定文件的二进制流。

- `POST /api/files/upload`  
  上传文件（multipart/form-data）。字段：`file`（必需）、`version`（可选）。上传成功会保存文件并生成/更新 `OriginConfig.json`。

- `GET /api/files/version`  
  返回简化的版本信息，客户端优先使用，示例响应：

# Response

- `GET/api/files/config` 
返回完整 `OriginConfig.json`（用于管理界面或排查）。

## 常见问题与排查

- “前端/客户端无法显示版本信息”
  - 在浏览器或客户端检查 Network，确认 `/api/files/version` 返回 200 且为正确的 JSON；
  - 如使用不同域名/端口，检查 CORS 配置。
- “上传报错或文件损坏”
  - 检查 `wwwroot/packages` 权限；
  - 查看服务端日志，确认 MD5 计算与 Save 操作正常。
- “下载慢或中断”
  - 检查网络带宽、服务器磁盘 IO 与客户端网络条件；
  - 在生产中推荐启用 HttpClientFactory、合理的超时与重试策略。

## 开发者指南

- 目标框架：.NET 10，C# 14；
- 编码与提交规范请参考仓库中的 `CONTRIBUTING.md`（如无请新建并说明分支与 PR 流程）；
- 推荐改进：
  - 将 HttpClient 注入（IHttpClientFactory）以复用连接并统一超时/重试策略；
  - 为前端管理界面拆分静态资源（CSS/JS）并添加单元/集成测试；
  - 增加鉴权（如 Basic/Auth token）和上传文件的反病毒扫描（生产建议）。

## 贡献与许可证

- 欢迎提交 Issue 与 Pull Request。请遵循 `CONTRIBUTING.md` 说明。
- 建议在仓库中添加 LICENSE（推荐 MIT），并在此处注明许可条款。

## 版本与发布

- 发布脚本

```
pwsh .\publish.ps1
```

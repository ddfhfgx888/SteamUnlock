# SteamHelper

一个面向 Windows 的单文件 Steam 配置部署 CLI。程序会自动检测 Steam 目录、安装或检查 OpenSteamTools 组件，并根据 AppID 下载和部署游戏配置。

> [!IMPORTANT]
> 请仅处理你有权使用的游戏和配置内容。项目与 Valve、Steam、OpenSteamTools 以及配置服务提供方无隶属或背书关系。

## 功能

- 从注册表及常见路径检测 Steam 安装目录
- 安装前清理 Steam 根目录中的 `xinput1_*.*` 和 `dwmapi.*`
- 将内嵌的 `OpenSteamTool.dll`、`dwmapi.dll`、`xinput1_4.dll` 部署到 Steam 根目录
- 使用 SHA-256 检查三个组件的完整性
- 根据 AppID 查询并下载 ZIP 配置包
- 将 `.json`、`.manifest`、`.lua` 文件部署到 `Steam/config/lua`
- 支持交互菜单和命令行参数

## 系统要求

- Windows 10/11 x64
- PowerShell 5.1 或更高版本（仅编译需要）
- .NET Framework 4.x
- 网络连接
- Steam 位于受保护目录时需要管理员权限

## 快速开始

从 [Releases](../../releases) 下载 `SteamHelper.exe`，然后运行：

```powershell
.\SteamHelper.exe
```

首次使用选择 `1` 安装/修复组件，再选择 `2` 输入 Steam AppID。部署后如未生效，请完全退出并重启 Steam。

### 命令行模式

```powershell
.\SteamHelper.exe install
.\SteamHelper.exe activate APPID
```

## 从源码构建

仓库内 `vendor/OpenSteamTools` 应包含以下运行时文件：

```text
OpenSteamTool.dll
dwmapi.dll
xinput1_4.dll
```

运行构建脚本：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

输出文件位于 `dist/SteamHelper.exe`。三个 DLL 会作为资源嵌入最终 EXE。

## 项目结构

```text
.
|-- .github/              GitHub 模板与 Actions
|-- docs/                 技术与使用文档
|-- src/                  C# 源代码
|-- vendor/OpenSteamTools 运行时依赖
|-- build.ps1             本地构建脚本
|-- CHANGELOG.md          版本记录
|-- CONTRIBUTING.md       贡献指南
`-- SECURITY.md           安全问题报告方式
```

## 文档

- [工作原理](docs/ARCHITECTURE.md)
- [故障排除](docs/TROUBLESHOOTING.md)
- [贡献指南](CONTRIBUTING.md)
- [版本记录](CHANGELOG.md)

## 第三方组件

本项目发行文件会嵌入 [OpenSteamTool](https://github.com/OpenSteam001/OpenSteamTool) 运行时组件。其版权与许可归上游项目及贡献者所有，详见 [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md)。

## 致谢

- 感谢 [OpenSteam001/OpenSteamTool](https://github.com/OpenSteam001/OpenSteamTool) 提供核心运行时组件。
- 感谢 [NachoNeko](https://nachoneko.netlify.app/) 提供游戏配置查询与下载服务。

## 许可证

本仓库自有源代码采用 [MIT License](LICENSE)。内嵌的 OpenSteamTool 组件采用上游的 [GNU GPL v3.0](https://github.com/OpenSteam001/OpenSteamTool/blob/main/LICENSE)，不包含在本仓库 MIT 授权范围内。

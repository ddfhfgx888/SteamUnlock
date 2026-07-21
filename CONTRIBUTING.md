# 贡献指南

感谢你参与改进项目。

## 开发流程

1. Fork 仓库并从 `main` 创建功能分支。
2. 保持改动范围集中，不要提交 `dist/` 或临时文件。
3. 在 Windows PowerShell 中运行 `./build.ps1`。
4. 测试交互菜单、`install` 及 `activate APPID` 参数。
5. 更新相关文档和 `CHANGELOG.md`。
6. 提交 Pull Request，说明行为变化和验证步骤。

## 代码约定

- 源代码保持兼容 .NET Framework 4.x 自带编译器。
- 新增文本文件默认使用 UTF-8。
- 文件操作必须限制在检测到的 Steam 目录或系统临时目录中。
- 网络请求必须设置超时并向用户显示清晰错误。
- 不要在仓库中提交令牌、Cookie、个人路径或下载得到的游戏配置。

## 第三方依赖

更新 `vendor/OpenSteamTools` 时，请同时更新 `THIRD_PARTY_NOTICES.md`，核对上游版本、来源、哈希和许可证要求。

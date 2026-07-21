# Third-Party Notices

## OpenSteamTools runtime components

本项目的构建产物嵌入以下第三方文件：

- `OpenSteamTool.dll`
- `dwmapi.dll`
- `xinput1_4.dll`

这些文件来自 [OpenSteam001/OpenSteamTool](https://github.com/OpenSteam001/OpenSteamTool) 1.4.8 Release，版权及许可归其上游作者和贡献者所有。

- 上游仓库：https://github.com/OpenSteam001/OpenSteamTool
- 上游许可证：[GNU General Public License v3.0](https://github.com/OpenSteam001/OpenSteamTool/blob/main/LICENSE)
- 使用版本：1.4.8 Release

这些第三方文件不受本仓库 MIT License 的重新授权。分发包含这些组件的构建产物时，应遵守上游 GNU GPL v3.0 的相应要求并保留许可证与版权声明。

当前仓库文件哈希：

```text
CC086189E9AE5F6FEC1B9839110FD5EC5836E86989CB5F15CAB80BB813DF44F8  dwmapi.dll
B2ED24E0B4E2D0DAE4CAA8817ED4C0C34AF8FDF056F356FD697AE1356EA22581  OpenSteamTool.dll
730D6E3C1216228392CF336127AD663564CAFDB560FAF3AE8BDFCC8CA6F38A27  xinput1_4.dll
```

发布 GitHub Release 前，请核验上游版本、许可证要求以及上述 SHA-256 是否仍与发布文件一致。

## NachoNeko configuration service

感谢 [NachoNeko](https://nachoneko.netlify.app/) 提供游戏配置查询与下载服务。本项目仅按照其公开页面所使用的接口进行请求，该服务独立于本仓库，其内容、可用性及条款由服务提供方负责。

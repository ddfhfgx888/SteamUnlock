# 工作原理

## 安装流程

1. 从注册表和常见安装路径定位包含 `steam.exe` 的目录。
2. 要求用户完全退出 Steam。
3. 删除 Steam 根目录现有的 `xinput1_*.*` 与 `dwmapi.*`。
4. 从程序资源中释放三个 OpenSteamTools DLL。
5. 使用 SHA-256 比较内嵌资源和落盘文件。

## 激活流程

1. 检查三个运行时文件是否存在且哈希一致。
2. 验证用户输入的 AppID 为正整数。
3. 通过 [NachoNeko](https://nachoneko.netlify.app/) 页面所使用的服务查询游戏信息和短期下载令牌。
4. 将 ZIP 下载至系统临时目录。
5. 只提取 `.json`、`.manifest`、`.lua`，并使用文件名扁平化写入 `Steam/config/lua`。
6. 清理临时目录并提示用户重启 Steam。

## 单文件实现

构建脚本使用 Windows 自带的 .NET Framework C# 编译器，将三个 DLL 作为 `Embedded.*` 资源写入 EXE，因此最终用户只需下载一个文件。

# 故障排除

## 未检测到 Steam

按提示输入包含 `steam.exe` 的 Steam 根目录，例如 `C:\Program Files (x86)\Steam`。

## 提示没有写入权限

完全退出 Steam，然后右键程序选择“以管理员身份运行”。

## 文件正在被占用

从系统托盘退出 Steam，并在任务管理器中确认 `steam.exe` 已结束，再重新安装组件。

## 查询或下载失败

检查网络、系统时间和 TLS 设置，稍后再次尝试。第三方服务不可用时请求会显示 HTTP 或网络错误。

## 提示 AppID 未收录

确认输入的是 Steam 商店页面中的数字 AppID。服务端没有对应记录时不会创建本地配置。

## 显示成功但 Steam 中未生效

完全退出并重启 Steam，随后检查 `Steam/config/lua` 中是否存在对应 AppID 的配置文件。

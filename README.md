# DSH Tray Launcher

一个为 Windows 编写的轻量级 DeepSeek Harness 托盘启动器。这是我的第一个正式版本 `v1.0.0`。

> 本项目是个人作品，并非 DeepSeek 官方产品，也不代表 DeepSeek 官方认可或背书。

![DSH Tray Launcher icon](tray/whale-preview.png)

## 功能

- 从系统托盘启动、停止和重启本机 DSH Web 服务
- 只管理由启动器实际创建并记录的 Node.js 进程树
- 打开 DSH Web 界面并显示当前运行状态
- 检查 GitHub 与 npm 上的 DSH 更新版本
- 创建桌面快捷方式和配置当前用户开机启动
- 单实例运行，避免重复托盘进程
- 自包含单文件发布，无需另外安装 .NET Runtime
- 多尺寸 Windows 图标：16、24、32、48、64、128、256 像素

## 环境要求

- Windows 10/11 x64
- 已安装 Node.js
- 已全局安装 `@deepseek-ai/dsh`

```powershell
npm install -g @deepseek-ai/dsh
```

## 使用便携版

1. 从 Releases 下载 `DSH-Tray-Launcher-v1.0.0-win-x64-portable.zip`。
2. 解压到一个长期保留的目录。
3. 运行 `dsh-tray.exe`。
4. 如需桌面入口，在托盘菜单中选择“创建桌面图标”。

启动器会使用 `127.0.0.1:3080` 启动 DSH Web 服务。退出启动器时，只会停止状态文件中记录且启动时间匹配的受管进程树。

## 从源码构建

需要 .NET 10 SDK：

```powershell
dotnet build .\tray\dsh-tray.csproj
dotnet publish .\tray\dsh-tray.csproj -c Release -r win-x64 --self-contained true
```

## 隐私与网络

启动器不收集遥测。版本检查会访问 DeepSeek Harness GitHub 仓库和 npm 镜像；更新操作只有在用户确认后才执行。

## 许可证与声明

启动器源代码采用 MIT License。DeepSeek、DeepSeek Harness、相关名称及鲸鱼图标的权利归各自权利人所有，详见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。

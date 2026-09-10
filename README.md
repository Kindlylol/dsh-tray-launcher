# DSH Tray Launcher

一个为 Windows 原生 Node.js 环境编写的轻量级 DeepSeek Harness 托盘启动器。当前版本为 `v1.0.4`。

> 本项目是个人作品，并非 DeepSeek 官方产品，也不代表 DeepSeek 官方认可或背书。

![DSH Tray Launcher icon](tray/whale-preview.png)

## 功能

- 从系统托盘启动、停止和重启本机 DSH Web 服务
- 只管理由启动器实际创建并记录的 Node.js 进程树
- 打开 DSH Web 界面并显示当前运行状态
- 通过 DSH API 判断后台是否真正可用，区分“进程存活”和“服务健康”
- 检查 GitHub 与 npm 上的 DSH 更新版本
- 创建桌面快捷方式和配置当前用户开机启动
- 单实例运行，避免重复托盘进程
- 自包含单文件发布，无需另外安装 .NET Runtime
- 多尺寸 Windows 图标：16、24、32、48、64、128、256 像素
- EXE 内保留黑鲸鱼主图标和蓝鲸鱼备用图标，快捷方式“更改图标”时可选

## Windows 版环境要求

- Windows 10/11 x64
- 已安装 Node.js
- 已全局安装 `@deepseek-ai/dsh`

```powershell
npm install -g @deepseek-ai/dsh
```

## Windows 版使用方法

1. 从 [Releases](https://github.com/Kindlylol/dsh-tray-launcher/releases) 下载 `*-win-x64-portable.zip`。
2. 解压到一个长期保留的目录。
3. 运行 `dsh-tray.exe`。
4. 如需桌面入口，在托盘菜单中选择“创建桌面图标”。
5. 右键托盘图标可以打开界面、重启服务、查看日志、创建快捷方式和设置开机自启。

新建快捷方式默认使用黑鲸鱼图标。需要改回旧蓝鲸鱼时，在快捷方式属性中选择“更改图标”，浏览到同一个 `dsh-tray.exe` 后选择第二个图标。

启动器会在 Windows 中使用 `127.0.0.1:3080` 启动 DSH Web 服务。退出启动器时，只会停止状态文件中记录且启动时间匹配的受管进程树。

## 建议的 DSH 更新步骤

**建议更新时先切换到核心模式检测更新；更新完成、核心正常启动后，再尝试切换到插件模式。**

1. 右键托盘图标，选择“切换到核心模式（无第三方插件）”，等待核心服务就绪。
2. 在核心模式下点击“检测更新”，按提示更新 DSH 本体。
3. 等待新版本安装、认证和 API 验收完成，确认核心模式正常启动。
4. 如需第三方插件，再选择“尝试插件模式（失败自动回核心）”。
5. 如果插件模式失败，继续使用自动恢复的核心模式，打开“插件诊断与修复命令”，按实际来源修复插件后再尝试。

本体更新优先于第三方插件。插件不兼容不应阻止使用已验收的新本体；插件模式能启动也不代表每项插件功能都已验证。

以上“检测更新”更新的是 **DSH 本体**。更新 **Windows 托盘程序** 请从 Releases 下载新版便携包，退出旧托盘后解压运行，并重新创建桌面快捷方式。

### v1.0.4：本体优先与插件隔离

- 每次打开托盘默认启动核心模式。核心 profile 为 `dsh-tray-core-<DSH版本>`，只组合官方 `dsh-base`、`dsh-web-app`，与 `web` 的第三方插件分开；仍使用原 `.dsh` 中的会话、凭据和设置。
- 托盘检查核心清单和补丁；发现全局补丁非空、核心补丁非空或核心中装入依赖就拒绝启动，不会假称完成隔离。不要在核心 profile 安装插件。
- 新版启动 URL 的令牌仅保存在托盘内存，用于换取认证 Cookie 和打开浏览器；新写入的日志会隐藏 URL token。旧日志不会被清除或重写。
- 对带 `dsh-api-gateway` 的新版验收 `settings/describe`；旧运行时继续验收 `host.describe`。要求 HTTP 成功、请求 ID 匹配且 `result.ok=true`；首页和监听端口不能代替 RPC 验收。
- 更新先安装到 `%LOCALAPPDATA%\DSH Tray Launcher\runtimes` 的新目录，通过核心启动验收后才保存运行时选择。安装失败保留原服务；候选启动失败恢复原运行时并尝试核心启动。旧安装及失败候选均保留，不自动删除。
- 更新完成保持核心模式。选择“尝试插件模式”才启动原 `web`；失败自动回核心，并提供本次日志和手动修复命令。不会仅凭 npm 安装成功就判断插件兼容，不自动批量启用或升级第三方插件。
- 插件启动通过仅代表整套组合能启动且 RPC 可用，不能证明各插件业务功能正常。组合冲突需查看诊断；没有充分证据时不把所有插件标为损坏。
- 托盘管理的独立运行时与命令行全局 `dsh` 分开。插件修复命令针对原 `web` profile，修复后须通过托盘重新试运行。独立目录不会自动追随全局 npm 更新。

该逻辑复用 [DSH 官方 profile 架构](https://github.com/deepseek-ai/deepseek-harness/blob/master/docs/architecture.md)。运行时认证和 RPC 适配以安装包源码及真实进程验收为依据。暂不承诺未来 DSH 的协议不再变化；未知协议会验收失败并保留旧安装。

本地验证：`pwsh -File tools/test-recovery.ps1`；加入 `-Install` 会在测试目录真实安装 DSH，并验证安装事务。测试使用独立 home，不读取原会话与凭据，需空闲的 3080 端口。

## 日志与问题反馈

- Windows 托盘日志：`%LOCALAPPDATA%\DSH Tray Launcher\dsh-tray.log`
- Windows 服务日志：`%LOCALAPPDATA%\DSH Tray Launcher\dsh-service.log`
- 使用前请附上版本、发布包名称、Windows 版本和相关日志片段（不要上传账号、密钥或私人数据）。
- Bug 和兼容性问题请提交到 [GitHub Issues](https://github.com/Kindlylol/dsh-tray-launcher/issues)。

## 作者与贡献

- Windows 托盘启动器：`Kindlylol`
- WSL2 用户请使用独立项目：[dsh-tray-launcher-wsl](https://github.com/Kindlylol/dsh-tray-launcher-wsl)。

## 从源码构建

需要 .NET 10 SDK：

```powershell
dotnet build .\tray\dsh-tray.csproj
dotnet publish .\tray\dsh-tray.csproj -c Release -r win-x64 --self-contained true
```

## 隐私与网络

启动器不收集遥测。版本检查会访问 DeepSeek Harness GitHub 仓库和 npm 官方仓库；更新操作只有在用户确认后才执行。

## 许可证与声明

启动器源代码采用 MIT License。DeepSeek、DeepSeek Harness、相关名称及鲸鱼图标的权利归各自权利人所有，详见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。

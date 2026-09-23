# DSH Tray Launcher v1.0.7-beta.1

本地测试版，未发布 GitHub，不替换 v1.0.6 稳定版。本项目是个人作品，并非 DeepSeek 官方产品。

## 本次解决什么

某个第三方 bundle 阻止插件模式启动时，不必先依赖 DSH 网页修复。托盘提供独立 Windows 修复窗口，可选择一个插件、查看最后一次插件启动诊断、暂时停用并重试，或重新启用并重试。每次只尝试一次插件启动，失败回到核心；已验收的新本体不会因插件失败回退。

原始堆栈中出现包名不代表它就是故障源。仅明确的包级不兼容诊断会标为候选，仍需用户确认；其他情况显示“原因未确定”。不自动推断依赖、不批量更新、不卸载、不强制兼容。

## Beta 与稳定版隔离

| 项目 | Beta | 稳定版 |
|---|---|---|
| 端口 | 3187 | 3080 |
| 托盘状态 | `%LOCALAPPDATA%\DSH Tray Launcher Beta` | `%LOCALAPPDATA%\DSH Tray Launcher` |
| DSH 数据 | Beta 状态目录下的 `home` | 原用户 `.dsh` |
| 桌面图标 | DeepSeek Harness Beta | DeepSeek Harness |
| 自动启动 | 禁用 | 保持原设置 |

Beta 每次打开先显示修复窗口，不自动启动后台。点击“启动 Beta 插件环境”才启动；关闭修复窗口后也可通过托盘启动核心或插件模式。模式选择保存在 Beta 自己的配置中。

首次运行只读引用稳定版所选 DSH 运行时（若存在），不复制会话、插件、凭据或设置。运行时文件不会被修复操作修改；更新安装到 Beta 自己的 runtimes。没有稳定版运行时的电脑可使用已安装的全局 DSH。独立数据目录不是安全沙箱：测试插件仍有当前用户权限，勿运行不可信插件。

## 如何试用

1. 解压本 Beta 到独立目录，运行 dsh-tray.exe；不要覆盖稳定版。
2. 初始列表为空是正常的，正式插件不会自动导入。
3. 点击“启动 Beta 插件环境”，在打开的 Beta 网页中按需配置测试账户、安装少量待测插件。不要手工把正式凭据或整个 `.dsh` 拷进去。
4. 发生启动故障时打开托盘“插件修复”。选择插件后点击“停用所选并重试”，确认后执行。其余插件启动成功时托盘显示“部分插件模式”。
5. 插件修复后选择“重新启用并重试”。若仍失败会回核心并保留用户本次启用选择，不会无限重试或悄悄卸载。
6. 如网页在其他端口已经打开，注意使用 Beta 自动打开的 3187 地址，不要误操作稳定版。

## 两种安装包

| 文件名后缀 | .NET 要求 |
|---|---|
| `portable-compressed.zip` | 内置 .NET，无需额外安装 |
| `light-requires-dotnet10.zip` | .NET 10 Desktop Runtime x64，建议最新 10.0.x |

两版功能一致，均需要 Node.js/npm 和可用的 DSH 本体。轻量版缺少运行时时使用 .NET 原生提示，随包附微软下载入口；未在无 .NET 的干净 Windows 上验证按钮跳转。

## 备份与限制

- 只修改 Beta `home/profiles/web/package.json` 的 bundle 选择及 `dshTrayRecovery` 停用记录，不改依赖、lockfile 或补丁。
- 每次修改使用同目录原子替换，原文件保存为 `package.json.tray-backup-*`。单项恢复用“重新启用”；完整人工恢复需先退出 Beta 后台，核对备份再替换 package.json。
- 读取到提交前检查 manifest、profile patch 与 lockfile 指纹，发现外部变化拒绝覆盖。锁可防止本工具并发，但不代表官方包管理器遵守此锁；请勿同时编辑或执行包管理命令。非协作写入在最后检查与替换之间仍存在极短竞争窗口。
- 无效 JSON、重复启用项、停用记录冲突、链接/junction 修复路径会拒绝修改。
- 某些 profile patch 仍可能直接加载被移出 bundle 的插件；本版不会解析或重写任意补丁。这种情况可能仍需核心模式人工排查。
- API 就绪仅证明组合能启动，不证明全部插件业务功能正常。
- 本机 DSH 0.1.6-alpha.2 尚无已核实的新版兼容预检接口，本 Beta 不自行实现版本豁免，也不擅自升级 DSH 来获取新接口。

## 构建与验证

```powershell
./build-portable.ps1 -Version 1.0.7-beta.1 -Flavor portable
./build-portable.ps1 -Version 1.0.7-beta.1 -Flavor light
```

测试入口：`--contract-tests <新目录>`、`--repair-tests <新目录>`；真实隔离测试为 `--repair-integration <新目录> <现有DSH包的package.json>`，使用 3187，测试结束停止自己创建的服务。UI 夹具为 `--repair-ui-fixture <新目录>`，只改夹具清单，不启动真实 DSH。

见随包 BETA-VALIDATION.md 的验证范围。正式源配置与私有验证日志不会打入发布包。

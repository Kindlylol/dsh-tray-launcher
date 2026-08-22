# DeepSeek Harness 托盘工具（dsh-tray）项目书

**版本**：v0.1（交付给接手开发者）
**日期**：2026-08-22
**状态**：功能基本完成，存在 1 个已知 Bug（图标渲染偏移）+ 1 个体验优化点（菜单样式）

---

## 1. 项目概述

为 DeepSeek Harness（DSH）开发一个 Windows 系统托盘小工具，用于管理本机的 DSH 服务：
启动/停止/重启服务、打开 Web UI、检测并执行版本更新、查看日志、创建桌面图标、开机自启。

**产品定位**：常驻系统托盘的轻量守护工具，替代命令行启动方式，让非技术用户也能一键管理 DSH。

---

## 2. 技术栈与环境

| 项 | 值 |
|---|---|
| 语言 | C#（.NET 10，Windows Forms） |
| 框架 | net10.0-windows，UseWindowsForms |
| 发布 | **Self-contained 单文件**（win-x64，内含 .NET 运行时，目标机零依赖） |
| exe 大小 | ~116 MB |
| 环境 | Windows 10/11 x64，.NET SDK 10.0.400（编译用） |
| 管理对象 | 全局 npm 包 `@deepseek-ai/dsh`（`npm install -g` 安装的 `dsh` 命令） |
| DSH 配置 | `%USERPROFILE%\.dsh` |
| 端口 | 3080（`dsh web` 默认） |

---

## 3. 功能需求（已实现）

### 3.1 托盘本体
- 无窗口、无控制台黑框（WinExe + 无主窗体）
- 系统托盘显示黑色鲸鱼图标（DeepSeek Harness 官方 favicon SVG 渲染）
- 左键单击：无响应（需求如此）
- 双击：打开默认浏览器访问 `http://127.0.0.1:3080`
- 单实例（重复启动时新实例立即退出，现有实例继续运行）

### 3.2 右键菜单
| 菜单项 | 功能 |
|---|---|
| 服务状态（只读） | 显示"服务: 运行中 (v0.1.1-rc.2)"或"已停止" |
| 打开 DSH 主界面 | 浏览器打开 Web UI |
| 重启 DSH 服务 | 仅停止启动器记录且启动时间匹配的受管进程树 → 重启 `dsh web` → 等待就绪（最多 40s）→ 弹窗提示成功/失败；3080 被非受管进程占用时拒绝启动 |
| 检测更新 | 对比 GitHub tags / npm dist-tags 与本地版本；有更新则询问，确认后停服 → `npm install -g`（带进度条窗口）→ 完成提示 → 重启服务 |
| 查看日志 | notepad 打开 `dsh-tray.log` |
| 开机自启 | 注册/取消 HKCU `Run` 键 `DSHTray` |
| 创建桌面图标 | 在桌面创建 `DeepSeek Harness.lnk`（自动识别 OneDrive 同步桌面） |
| 退出 | 停止 DSH 服务 + 退出托盘 |

### 3.3 启动行为
- 托盘启动时若 DSH 服务未运行 → 自动启动 `dsh web` → 等就绪（最多 40s）→ 打开浏览器**一次**

### 3.4 日志
- `dsh-tray.log`：托盘自身日志（写在与 exe 同目录）
- `dsh-service.log`：dsh web 服务输出重定向

---

## 4. 文件结构

```
DeepSeek Harness\
├── dsh-tray.exe              # 发布产物（自包含单文件，116MB）
├── README-tray.md            # 使用说明
├── tray\                     # 源码目录
│   ├── Program.cs            # 全部代码（936 行，单文件）
│   ├── dsh-tray.csproj       # 项目文件
│   ├── app.manifest          # 兼容性声明
│   ├── whale.ico             # 图标源文件（7 尺寸，已嵌入 exe）
│   ├── whale.svg             # 官方鲸鱼 SVG 源
│   ├── whale-preview.png     # 渲染调试用
│   ├── make-icon.ps1         # 早期 PS 图标脚本（已废弃，可删）
│   └── bin\Release\...       # 构建产物
```

---

## 5. 构建与发布

```bash
# NuGet.Config 的 packageSources 可能为空，需显式指定源：
cd tray
dotnet publish -c Release --source https://api.nuget.org/v3/index.json
# 产物：tray\bin\Release\net10.0-windows\win-x64\publish\dsh-tray.exe
# 拷贝到部署目录即可（自包含，无配套 dll）
```

**注意**：
- NuGet.Config（`%APPDATA%\NuGet\NuGet.Config`）当前 `<packageSources>` 为空，必须加 `--source` 才能还原 runtime pack
- `SelfContained + PublishSingleFile` 需要从 nuget.org 下载 win-x64 runtime 包（网络可达）

---

## 6. 已知问题与待办（★ 重点）

### 6.1 ★ Bug：图标渲染整体偏上（用户已反馈，需修复）

**现象**：exe 图标和生成的桌面图标中，黑色鲸鱼整体偏上、超出图标范围，只能看到下边缘。

**精确数据**（256x256 渲染实测）：
- 画布：256x256，画布中心 Y = 128
- 鲸鱼内容：Y 范围 14~204，内容中心 Y = 109 → **偏上 19px**，且 X 中心 122 vs 128 略偏左 6px
- 16px 小尺寸下偏移更明显（内容可能溢出画布顶部）

**根因分析**（`Program.cs` 中 `SvgPath.Parse` 的归一化逻辑）：
1. `GraphicsPath.GetBounds()` 返回的是**贝塞尔曲线控制点的包围盒**，比实际曲线填充范围**更大**（GDI+ 特性）
2. 归一化用 `GetBounds()` 计算缩放和平移，把"控制点包围盒"居中，但实际填充的鲸鱼曲线更小、位置更靠上 → 视觉上偏上
3. 早期版本还有 M 命令处理 bug（从原点画线），已修复（bounds 从错误的 32x24 修正为正确的 48.8x36.6），但**居中偏移仍未解决**

**建议修复方向**（按优先级）：
1. **方案 A（推荐）**：不再依赖 GDI+ 的 `GetBounds()`。用**逐像素扫描**渲染后的 Bitmap 计算实际内容包围盒，再整体平移居中。具体：先渲染一版 → 遍历像素找实际非透明区域 → 计算 `TranslateTransform` 平移量 → 重绘。
2. **方案 B**：改用成熟 SVG 渲染库（如 `Svg.Skia`）替代手写 SVG path 解析器，图标质量与居中由库保证。
3. **方案 C**：找 DeepSeek 官方品牌资源里的现成 PNG/ICO 直接使用（`dsh-web-frontend/dist/favicon.svg` 是唯一找到的源），用 ImageMagick/inkscape 等工具离线转成多尺寸 ICO，绕开运行时渲染。

**注意**：鲸鱼 SVG 是单色路径（`fill="#000"`），深色主题下不可见的问题已由 `@media (prefers-color-scheme: dark)` 处理（官方 SVG 里有），但运行时渲染未处理深色模式——若需支持可加系统主题检测。

### 6.2 体验优化：右键菜单样式（用户希望 Win11 新式菜单）

**现状**：已恢复默认 `ContextMenuStrip` + `ToolStripSystemRenderer`（跟随系统主题，整行可点，速度正常）。

**用户期望**：微信/Codex 那种 Windows 11 新式菜单——**圆角、亚克力毛玻璃背景、hover 整行高亮**。

**根因**：WinForms 的 `ContextMenuStrip` 无论用什么 Renderer 都无法实现 Win11 新式菜单（圆角+毛玻璃），那是 UWP/WinUI 或自绘才能做到的。

**建议方案**（三选一，需与用户确认）：
1. **WPF + `System.Windows.Controls.ContextMenu`**：改技术栈为 WPF，WPF 的菜单在 Win11 下更接近系统风格，但仍不是完整 Win11 新式菜单。
2. **自绘弹出窗口**：用无边框 Form 模拟 Win11 菜单（圆角 + DWM 模糊 + 自绘 hover），工作量中等，可控性最高。
3. **保持现状**：如果速度与可点性没问题，可接受当前系统渲染样式。

### 6.3 其他说明
- `dsh-cost-meter` 插件当前在 profile 里（用户之前装 web-ui-all 时带回来的），如不需要可在 `~/.dsh/profiles/web/package.json` 移除
- 更新检测的 `GetRemoteVersionAsync` 用 `git ls-remote` 可能因网络慢超时，已有 npm dist-tags 兜底

---

## 7. 代码结构（Program.cs，936 行）

| 行区间（约） | 方法 | 说明 |
|---|---|---|
| 40 | `Main` | 入口；DPI 设置；测试模式分发（--selftest/--gen-ico 等） |
| 138 | `BuildMenu` | 右键菜单构建 |
| 182 | `RenderWhale` | 渲染单尺寸鲸鱼 Bitmap（★ 图标 bug 所在） |
| 198 | `GenIco` | 生成多尺寸 ICO（16~256，PNG 帧格式） |
| 251 | `MakeIcon` | 托盘运行时图标（32x32） |
| 278 | `RunSelfTest` | 自检（图标/版本/端口/路径） |
| 344 | `IsServiceRunning` | TCP 探测 3080 端口 |
| 364 | `RestartService` | 重启流程（停→起→等就绪→弹窗） |
| 404 | `StopService` | 校验状态文件中的 PID 与进程启动时间后，停止受管进程树 |
| 435 | `StartService` | 直接执行 `node ...\@deepseek-ai\dsh\lib\bin.js web --host 127.0.0.1 --port 3080 --no-open` |
| 455 | `CheckForUpdatesAsync` | 版本检测（git tags → npm dist-tags） |
| 501 | `GetLocalVersion` | 读 npm 包 package.json / dsh --version |
| 610 | `PerformUpdate` | 停服→npm install（进度条）→重启 |
| 655 | `CreateDesktopShortcut` | WScript.Shell 创建 .lnk（OneDrive 桌面自适应） |
| 701 | `ToggleAutostart` | HKCU Run 开关 |
| 733 | `OpenLogs` | notepad 打开日志 |
| 760 | `ExitApp` | 停服 + 退出 |

**SvgPath 解析器**（`Program.cs` 后半部）：手写 SVG path 解析（M/L/C/Z + 相对坐标 + 隐式重复命令），已修复 M 命令起点 bug。

---

## 8. 测试模式（保留，方便调试）

| 参数 | 功能 |
|---|---|
| `--selftest` | 输出图标/版本/端口/路径自检到 `dsh-tray-selftest.txt` |
| `--test-restart` | 静默执行重启流程（不弹窗，日志见 dsh-tray.log） |
| `--test-stop` | 静默停止服务 |
| `--test-update` | 静默版本对比 |
| `--gen-ico` | 重新生成 whale.ico + whale-preview.png 到输出目录 |

---

## 9. 验收标准

- [ ] 图标：exe 图标、桌面快捷方式图标、托盘图标均为黑色鲸鱼，**内容居中、不越界、各尺寸清晰**
- [ ] 双击托盘打开浏览器；单击无响应
- [ ] 重启服务成功弹窗；服务确实重启且 3080 可访问
- [ ] 检测更新能正确对比本地/远程版本（当前均为 0.1.1-rc.2）
- [ ] 创建桌面图标在 OneDrive 桌面正常生成 .lnk 且带鲸鱼图标
- [ ] 退出后 3080 端口释放、托盘消失
- [ ] 开机自启注册/取消生效
- [ ] 右键菜单响应迅速、整行可点、文字清晰（DPI 125%/150% 下均正常）

---

## 10. 交接注意事项

1. **图标 bug（6.1）是当前最需要修复的**——用户已明确不满，优先处理
2. 菜单样式（6.2）需先与用户确认选哪个方案再动手
3. 修改后发布：`dotnet publish -c Release --source https://api.nuget.org/v3/index.json`（务必带 `--source`）
4. 发布产物是单个 exe（116MB），直接覆盖部署目录的 `dsh-tray.exe` 即可
5. 改代码后建议先跑 `--selftest` 和 `--test-restart` 回归，再交付

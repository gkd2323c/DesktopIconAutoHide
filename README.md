# DesktopIconAutoHide

一个 Windows 托盘小工具：当你处于桌面且鼠标静止一段时间后，自动隐藏桌面图标；鼠标移动后自动恢复。

## 功能特性

- 仅在“当前焦点为桌面”时触发自动隐藏
- 可配置静止隐藏时间（秒）
- 支持中英文界面，且可手动切换语言
- 支持可选开机启动（登录 Windows 后自动运行）
- 配置持久化到程序目录下的 `settings.json`
- 支持发布为单文件 `exe`

## 环境要求

- Windows 10/11
- .NET SDK 8.0（开发构建时需要）

## 本地开发

```powershell
dotnet build
dotnet run
```

运行后会在系统托盘出现图标，右键可打开设置、立即隐藏/显示、退出。

## 配置文件

程序会在 `exe` 所在目录读写：

```text
settings.json
```

当前配置字段：

- `IdleSeconds`: 鼠标静止秒数（0-3600，`0` 表示关闭自动隐藏）
- `LanguageMode`: `auto` / `zh` / `en`
- `AutoStartEnabled`: `true` / `false`

## 发布

### 常规发布（非单文件）

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

### 单文件发布（推荐）

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-singlefile.ps1
```

默认输出目录：

```text
.\artifacts\singlefile\win-x64\DesktopIconAutoHide.exe
```

### 一键推送到 GitHub

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\push-github.ps1 -Message "feat: your message"
```

不传 `-Message` 时会自动生成提交信息。

### GitHub Actions 自动发布

- `pull_request`：仅执行还原、构建、打包校验
- `push main`：在构建成功后自动创建 Release，并上传单文件 `exe`
- Release 标签格式：`auto-v<run_number>`

## 项目结构

```text
.
├─ AppSettings.cs
├─ DesktopIconController.cs
├─ LocalizedText.cs
├─ Program.cs
├─ SettingsForm.cs
├─ TrayApplicationContext.cs
├─ scripts/
│  └─ publish-singlefile.ps1
├─ DesktopIconAutoHide.csproj
└─ DesktopIconAutoHide.sln
```

## 发布到 GitHub（示例）

```powershell
git init
git add .
git commit -m "feat: initial release"
git branch -M main
git remote add origin <你的仓库地址>
git push -u origin main
```

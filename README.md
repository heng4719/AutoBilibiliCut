# videoCut

一个基于 `WPF + .NET 8` 的 Windows 桌面工具，支持：

- 使用 `yt-dlp` 下载在线视频到本地
- 读取本地视频信息
- 使用 `FFmpeg` 批量导出视频切片

## 开发环境

开发这个项目前，需要先准备：

1. Windows 系统
2. `.NET 8 SDK`
3. `FFmpeg`
4. `yt-dlp`

## 依赖文件放置目录

项目使用自带工具，不依赖系统全局安装。

### FFmpeg

需要准备这两个文件：

- `ffmpeg.exe`
- `ffprobe.exe`

放置到：

- `tools/ffmpeg/bin/ffmpeg.exe`
- `tools/ffmpeg/bin/ffprobe.exe`

### yt-dlp

需要准备这个文件：

- `yt-dlp.exe`

放置到：

- `tools/yt-dlp/yt-dlp.exe`

## 运行项目

在项目根目录执行：

```powershell
dotnet run
```

## 打包项目

如果要生成安装包，还需要安装：

- `Inno Setup 6`

打包命令：

```powershell
.\scripts\publish.ps1
.\scripts\build-installer.ps1
```

### 打包流程说明

1. 执行 `.\scripts\publish.ps1`
   - 生成发布目录
   - 输出位置为：`artifacts/publish/win-x64/`
   - 发布结果会自动包含项目自带的：
     - `tools/ffmpeg/bin/`
     - `tools/yt-dlp/`

2. 执行 `.\scripts\build-installer.ps1`
   - 调用 `Inno Setup 6`
   - 将发布目录打包成安装程序
   - 安装包输出位置为：`artifacts/installer/`

### 一条命令打包

也可以直接执行：

```powershell
.\scripts\build-installer.ps1
```

说明：

- 如果 `artifacts/publish/win-x64/` 不存在，脚本会先自动执行发布，再继续打包
- 如果该目录已经存在，脚本会直接使用现有发布文件生成安装包

### 修改代码后重新打包

如果你修改过代码，建议按这个顺序重新打包，确保安装包使用的是最新代码：

```powershell
.\scripts\publish.ps1
.\scripts\build-installer.ps1
```

更详细的打包说明见：

- [安装打包说明](/F:/WorkSpace/videoCut/docs/安装打包说明.md)

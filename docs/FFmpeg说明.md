# FFmpeg 使用说明

本项目不要求在电脑里单独安装 FFmpeg。

你只需要将以下两个文件放到项目目录中：

- `tools/ffmpeg/bin/ffmpeg.exe`
- `tools/ffmpeg/bin/ffprobe.exe`

目录不存在时可手动创建。

## 建议目录结构

```text
videoCut/
  tools/
    ffmpeg/
      bin/
        ffmpeg.exe
        ffprobe.exe
```

## 说明

1. 程序读取视频时使用 `ffprobe.exe`
2. 程序导出切片时使用 `ffmpeg.exe`
3. 构建或运行项目时，这两个文件会自动复制到输出目录

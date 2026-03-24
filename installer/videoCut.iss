#define AppName "视频下载与切片工具"
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef ProjectRoot
  #error ProjectRoot is required.
#endif
#ifndef PublishDir
  #error PublishDir is required.
#endif
#ifndef OutputDir
  #define OutputDir "artifacts\installer"
#endif

[Setup]
AppId={{C4F0D742-3F76-4BA8-8A7F-7A2D80D44334}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=videoCut
DefaultDirName={autopf}\VideoCut
DefaultGroupName={#AppName}
UninstallDisplayIcon={app}\videoCut.exe
OutputDir={#OutputDir}
OutputBaseFilename=videoCut-setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
SetupIconFile={#ProjectRoot}\assets\icon\app.ico
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加任务："; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\videoCut.exe"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\videoCut.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\videoCut.exe"; Description: "启动{#AppName}"; Flags: nowait postinstall skipifsilent

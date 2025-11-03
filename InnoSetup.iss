; 脚本由 Inno Setup 脚本向导生成。
; 有关创建 Inno Setup 脚本文件的详细信息，请参阅帮助文档！

#define MyAppName "说怪话Complainer奇美拉桌面通知器"
#define MyAppVersion "1.2"
#define MyAppPublisher "ttwe77"
#define MyAppURL "https://github.com/ttwe77/Complainer-Honkai-StarRail"
#define MyAppExeName "Complainer.exe"

[Setup]
; 注意：AppId 的值唯一标识此应用程序。不要在其他应用程序的安装程序中使用相同的 AppId 值。
; (若要生成新的 GUID，请在 IDE 中单击 "工具|生成 GUID"。)
AppId={{16E5CF03-E2F3-4B35-8D4C-3D5F2A707F14}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
; "ArchitecturesAllowed=x64compatible" 指定
; 安装程序只能在 x64 和 Windows 11 on Arm 上运行。
ArchitecturesAllowed=x64compatible
; "ArchitecturesInstallIn64BitMode=x64compatible" 要求
; 在 X64 或 Windows 11 on Arm 上以 "64-位模式" 进行安装，
; 这意味着它应该使用本地 64 位 Program Files 目录
; 和注册表的 64 位视图。
ArchitecturesInstallIn64BitMode=x64compatible
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=A:\Public\LICENSE.txt
; 移除以下行以在管理安装模式下运行 (为所有用户安装)。
PrivilegesRequired=lowest
OutputDir=A:\Public\新建文件夹 (4)
OutputBaseFilename=mysetup
SetupIconFile=H:\0Project\Complainer\Complainer\icon.ico
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl"
Name: "chinesetraditional"; MessagesFile: "compiler:Languages\ChineseTraditional.isl"
Name: "english"; MessagesFile: "compiler:Languages\English.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "H:\0Project\Complainer\Complainer\bin\Release\net8.0-windows10.0.17763.0\说怪话ComplainerV1.2\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "H:\0Project\Complainer\Complainer\bin\Release\net8.0-windows10.0.17763.0\说怪话ComplainerV1.2\Complainer.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "H:\0Project\Complainer\Complainer\bin\Release\net8.0-windows10.0.17763.0\说怪话ComplainerV1.2\config.ini"; DestDir: "{app}"; Flags: ignoreversion
Source: "H:\0Project\Complainer\Complainer\bin\Release\net8.0-windows10.0.17763.0\说怪话ComplainerV1.2\icon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "H:\0Project\Complainer\Complainer\bin\Release\net8.0-windows10.0.17763.0\说怪话ComplainerV1.2\Microsoft.Windows.SDK.NET.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "H:\0Project\Complainer\Complainer\bin\Release\net8.0-windows10.0.17763.0\说怪话ComplainerV1.2\WinRT.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "H:\0Project\Complainer\Complainer\bin\Release\net8.0-windows10.0.17763.0\说怪话ComplainerV1.2\Complainer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "H:\0Project\Complainer\Complainer\bin\Release\net8.0-windows10.0.17763.0\说怪话ComplainerV1.2\icons\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "H:\0Project\Complainer\Complainer\bin\Release\net8.0-windows10.0.17763.0\说怪话ComplainerV1.2\notices\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "H:\0Project\Complainer\Complainer\bin\Release\net8.0-windows10.0.17763.0\说怪话ComplainerV1.2\Complainer.deps.json"; DestDir: "{app}"; Flags: ignoreversion
; 注意：不要在任何共享系统文件上使用 "Flags: ignoreversion" 

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


; ============================================================================
;  Wallpaper Engine Extractor — Inno Setup 安装脚本
;
;  分发思路：只包含「共享 runtime」版本（dist\framework-dep 单 EXE ≈ 9.4 MB），
;  安装时检测用户机器上的 .NET 10 Desktop Runtime：
;    · 已装（≥ 10.0.0）→ 直接继续
;    · 缺失 → 弹窗询问，用户确认后才下载并安装 .NET Desktop Runtime，
;             装完重新检测，通过才继续；用户拒绝或安装失败则终止安装
;
;  最低版本判定：只要求主版本 = 10 且次版本 ≥ 0（即 10.0.0 起都算通过），
;  不限制补丁号 —— 10.0.0 / 10.0.13 / 10.1.x / 11.x 都能跑。
; ============================================================================

#define MyAppName "Wallpaper Engine Extractor"
#define MyAppVersion "0.2.0"
#define MyAppPublisher "dujunxi1993"
#define MyAppURL "https://github.com/DuJunxi1993/WallpaperEngine-Extractor"
#define MyAppExeName "WallpaperEngine_Extractor.exe"

; 可在编译命令行覆盖（用于免提权测试）：
;   iscc /DMyPrivilegesRequired=lowest /DMyOutputBaseFilename=xxx_test installer.iss
#ifndef MyPrivilegesRequired
  #define MyPrivilegesRequired "admin"
#endif
#ifndef MyOutputBaseFilename
  #define MyOutputBaseFilename "WallpaperEngine_Extractor_Setup"
#endif

; .NET Desktop Runtime 下载地址与 SHA-256（取自官方 release-metadata 10.0 频道）
; 允许编译命令行覆盖，仅用于测试缺失分支：
;   iscc /DForceMissingDotNet=1 /DMyDotNetRuntimeUrl=https://invalid/x.exe installer.iss
#ifndef MyDotNetRuntimeUrl
  #define MyDotNetRuntimeUrl "https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/10.0.13/windowsdesktop-runtime-10.0.13-win-x64.exe"
#endif
#define DotNetRuntimeHash "f5f0268b3d89ec3b9eb952b620330360b35c63593ead45816aeb26e934a4b7b6"
#define DotNetRuntimeFile "windowsdesktop-runtime-10.0.13-win-x64.exe"
#define MinDotNetVersion "10.0.0"

[Setup]
AppId={{8E4C7F2A-3B1D-4A6E-9F5C-2D7B8A1E6C93}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableDirPage=no
DisableProgramGroupPage=yes
OutputBaseFilename={#MyOutputBaseFilename}
OutputDir=Output
Compression=lzma2/max
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired={#MyPrivilegesRequired}
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
SetupIconFile=..\src\WallpaperEngine_Extractor\app.ico
WizardStyle=modern
; 安装包本身不动系统；.NET 运行时由 [Code] 在用户确认后单独安装

[Languages]
; Inno Setup 6 官方安装不带中文 .isl，这里用英文基础包 + 下方 [Messages] 覆盖关键文案，
; 保证用户可见的向导流程为中文（自定义弹窗本身已是中文）。
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\dist\framework-dep\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\dist\framework-dep\WallpaperEngine_Extractor.pdb"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Messages]
english.SetupAppTitle={#MyAppName} 安装程序
english.SetupWindowTitle={#MyAppName} 安装程序
english.WelcomeLabel1=欢迎安装 [name]
english.WelcomeLabel2={#MyAppName} 安装向导将引导您完成安装。%n%n本程序需要 .NET {#MinDotNetVersion} Desktop Runtime 或更高版本，安装前会自动检测；若缺失会询问您是否立即安装。
english.WizardSelectDir=选择安装位置
english.SelectDirDesc=本程序将安装到以下文件夹。
english.SelectDirLabel3=安装程序会将 [name] 安装到以下文件夹。
english.SelectDirBrowseLabel=点击「下一步」继续。如需选择其他文件夹，点击「浏览」。
english.WizardSelectTasks=选择附加任务
english.SelectTasksDesc=您希望安装程序执行哪些附加任务？
english.SelectTasksLabel2=请选择安装 [name] 时要执行的附加任务，然后点击「下一步」。
english.WizardReady=准备安装
english.PreparingDesc=安装程序正准备安装 [name] 到您的电脑。
english.ReadyLabel1=点击「安装」开始安装。
english.ReadyLabel2a=点击「安装」开始安装，或点击「上一步」检查或更改设置。
english.WizardInstalling=正在安装
english.InstallingLabel=正在安装 [name]，请稍候...
english.FinishedHeadingLabel=[name] 安装完成
english.FinishedLabel=安装程序已完成 [name] 的安装。
english.ButtonNext=下一步(&N) >
english.ButtonBack=< 上一步(&B)
english.ButtonInstall=安装(&I)
english.ButtonFinish=完成(&F)
english.ButtonCancel=取消
english.ButtonBrowse=浏览(&R)...
english.ButtonOK=确定
english.ButtonYes=是(&Y)
english.ButtonNo=否(&N)
english.ExitSetupTitle=退出安装
english.ExitSetupMessage=安装尚未完成，确定要退出吗？%n%n如果现在就退出，程序将不会被安装。
english.ConfirmUninstall=确定要完全移除 %1 及其所有组件吗？
english.UninstallAppFullTitle=卸载 %1
english.UninstallStatusLabel=正在从您的电脑中移除 %1，请稍候...
english.UninstalledAll=%1 已成功从您的电脑中移除。

[CustomMessages]
; 这些是 Inno 的 [CustomMessages] 条目（被 {cm:...} 引用），不能写在 [Messages] 里
CreateDesktopIcon=创建桌面快捷方式(&D)
AdditionalIcons=附加图标：
LaunchProgram=运行 %1

[Code]
var
  NeedInstallDotNet: Boolean;
  DotNetDownloadPage: TDownloadWizardPage;

// Windows 换行（Inno Pascal 没有 sLineBreak）
function NL(): String;
begin
  Result := Chr(13) + Chr(10);
end;

// ---- 版本解析：X.Y.Z ----
function ParseVersion(const S: String; out Major, Minor, Patch: Integer): Boolean;
var
  Rest, Sub: String;
  P, Code: Integer;
begin
  Result := False;
  Major := 0; Minor := 0; Patch := 0;

  P := Pos('.', S);
  if P = 0 then Exit;
  Sub := Copy(S, 1, P - 1);
  Code := StrToIntDef(Sub, -1);
  if Code < 0 then Exit;
  Major := Code;

  Rest := Copy(S, P + 1, Length(S) - P);
  P := Pos('.', Rest);
  if P = 0 then Exit;
  Sub := Copy(Rest, 1, P - 1);
  Code := StrToIntDef(Sub, -1);
  if Code < 0 then Exit;
  Minor := Code;

  Rest := Copy(Rest, P + 1, Length(Rest) - P);
  if Pos('.', Rest) <> 0 then Exit;   // 只接受三段式
  Code := StrToIntDef(Rest, -1);
  if Code < 0 then Exit;
  Patch := Code;

  Result := True;
end;

function VersionAtLeast(const S: String; MinMajor, MinMinor, MinPatch: Integer): Boolean;
var
  Ma, Mi, Pa: Integer;
begin
  Result := False;
  if not ParseVersion(S, Ma, Mi, Pa) then Exit;
  if Ma > MinMajor then begin Result := True; Exit; end;
  if Ma < MinMajor then Exit;
  if Mi > MinMinor then begin Result := True; Exit; end;
  if Mi < MinMinor then Exit;
  Result := Pa >= MinPatch;
end;

// ---- 在指定目录下查找满足最低版本的子目录，返回目录名，找不到返回 '' ----
function FindVersionDir(const RootDir: String; MinMajor, MinMinor, MinPatch: Integer): String;
var
  FindRec: TFindRec;
begin
  Result := '';
  if not DirExists(RootDir) then Exit;

  if FindFirst(RootDir + '\*', FindRec) then
  try
    repeat
      if (FindRec.Name <> '.') and (FindRec.Name <> '..')
         and ((FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0) then
      begin
        if VersionAtLeast(FindRec.Name, MinMajor, MinMinor, MinPatch) then
        begin
          Result := FindRec.Name;
          Exit;
        end;
      end;
    until not FindNext(FindRec);
  finally
    FindClose(FindRec);
  end;
end;

// ---- 检测 .NET Desktop Runtime ----
// 先扫标准安装位置 C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App
// （官方安装器与 dotnet-install 都落在这里），再兜底 DOTNET_ROOT。
function DetectedDotNetVersion(): String;
var
  Root: String;
begin
  Result := '';
#ifdef ForceMissingDotNet
  // 测试用：强制走「运行时缺失」分支，正式编译不会定义该符号
  Exit;
#endif

  Result := FindVersionDir(ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App'), 10, 0, 0);
  if Result <> '' then Exit;

  Root := GetEnv('DOTNET_ROOT');
  if Root <> '' then
    Result := FindVersionDir(Root + '\shared\Microsoft.WindowsDesktop.App', 10, 0, 0);
end;

function IsDotNetDesktopInstalled(): Boolean;
begin
  Result := DetectedDotNetVersion() <> '';
end;

// ---- 下载进度回调 ----
function OnDotNetDownloadProgress(const Url, FileName: String;
  const Progress, ProgressMax: Int64): Boolean;
begin
  if DotNetDownloadPage <> nil then
    DotNetDownloadPage.SetProgress(Progress, ProgressMax);
  Result := True;   // 返回 False 会取消下载
end;

// ---- 下载并静默安装 .NET Desktop Runtime ----
function InstallDotNetRuntime(): Boolean;
var
  InstallerPath: String;
  ResultCode: Integer;
begin
  Result := False;
  InstallerPath := ExpandConstant('{tmp}\{#DotNetRuntimeFile}');

  Log('Downloading .NET Desktop Runtime: {#MyDotNetRuntimeUrl}');
  DotNetDownloadPage := CreateDownloadPage(
    '正在下载 .NET Desktop Runtime',
    '本程序需要 .NET {#MinDotNetVersion} Desktop Runtime（或更高）。正在自动下载安装包（约 57 MB），请稍候。',
    @OnDotNetDownloadProgress);
  DotNetDownloadPage.Show;
  try
    try
      DownloadTemporaryFile('{#MyDotNetRuntimeUrl}', '{#DotNetRuntimeFile}',
        '{#DotNetRuntimeHash}', @OnDotNetDownloadProgress);
    except
      Log('Download exception.');
      MsgBox('下载 .NET Desktop Runtime 失败，请检查网络后重试。' + NL() + NL() +
             '也可以手动安装后重新运行本安装程序：' + NL() +
             'https://dotnet.microsoft.com/download/dotnet/10.0',
             mbError, MB_OK);
      Exit;
    end;
  finally
    DotNetDownloadPage.Hide;
    DotNetDownloadPage := nil;
  end;

  if not FileExists(InstallerPath) then
  begin
    Log('Downloaded installer missing: ' + InstallerPath);
    MsgBox('下载的 .NET 安装包不存在，安装无法继续。', mbError, MB_OK);
    Exit;
  end;

  Log('Running .NET installer quietly...');
  if not Exec(InstallerPath, '/install /quiet /norestart', '', SW_HIDE,
              ewWaitUntilTerminated, ResultCode) then
  begin
    Log('Failed to launch .NET installer.');
    MsgBox('无法启动 .NET Desktop Runtime 安装程序。', mbError, MB_OK);
    Exit;
  end;

  Log(Format('.NET installer exit code: %d', [ResultCode]));
  // 0 = 成功；3010 = 成功但需重启；1638 = 已存在更高版本
  Result := (ResultCode = 0) or (ResultCode = 3010) or (ResultCode = 1638);
  if not Result then
    MsgBox(Format('.NET Desktop Runtime 安装程序返回错误码 %d。', [ResultCode]), mbError, MB_OK);
end;

// ---- 启动时检测 ----
function InitializeSetup(): Boolean;
var
  Version: String;
  Answer: Integer;
begin
  Result := False;
  NeedInstallDotNet := False;

  Version := DetectedDotNetVersion();
  if Version <> '' then
  begin
    Log('.NET Desktop Runtime detected: ' + Version);
    Result := True;
    Exit;
  end;

  Log('.NET Desktop Runtime not found (need >= {#MinDotNetVersion}).');

  Answer := MsgBox(
    '本程序需要 .NET {#MinDotNetVersion} Desktop Runtime（或更高版本）才能运行。' + NL() +
    NL() + '当前系统未检测到该运行时。' + NL() +
    NL() + '点击「是」将自动下载并安装 .NET 10 Desktop Runtime（约 60 MB），' + NL() +
    '安装完成后继续安装本程序。' + NL() +
    NL() + '点击「否」退出安装。',
    mbConfirmation, MB_YESNO);

  if Answer = IDYES then
  begin
    NeedInstallDotNet := True;
    Result := True;   // 进入向导，在 PrepareToInstall 阶段执行安装
  end
  else
  begin
    MsgBox(
      '安装已取消。' + NL() + NL() +
      '请先安装 .NET 10 Desktop Runtime 后重新运行本安装程序：' + NL() +
      'https://dotnet.microsoft.com/download/dotnet/10.0',
      mbInformation, MB_OK);
    Result := False;
  end;
end;

// ---- 真正开始复制文件前，先确保运行时可用 ----
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';

  if not NeedInstallDotNet then
  begin
    // 用户机器上已有，或已在上一次运行中装好
    if not IsDotNetDesktopInstalled() then
      Result := '未检测到 .NET {#MinDotNetVersion} Desktop Runtime，安装无法继续。';
    Exit;
  end;

  if not InstallDotNetRuntime() then
  begin
    Result := '安装 .NET Desktop Runtime 失败。' + NL() +
              '请手动安装后重新运行本安装程序：' + NL() +
              'https://dotnet.microsoft.com/download/dotnet/10.0';
    Exit;
  end;

  // 重新检测，确认安装生效
  if not IsDotNetDesktopInstalled() then
  begin
    Result := '.NET Desktop Runtime 安装后仍未检测到（可能需要重启）。' + NL() +
              '请重启后重新运行本安装程序。';
    Exit;
  end;

  Log('.NET Desktop Runtime installed: ' + DetectedDotNetVersion());
end;




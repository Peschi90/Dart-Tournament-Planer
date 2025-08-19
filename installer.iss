; installer.iss

[Setup]
AppId={{D0E5E8F9-9B3E-4E23-AFAB-1234567890AB}}
AppName=Dart Tournament Planer
AppVersion={#MyAppVersion}
AppPublisher=Peschi90
AppPublisherURL=https://github.com/Peschi90/Dart-Tournament-Planer
DefaultDirName={autopf}\Dart Tournament Planer
DefaultGroupName=Dart Tournament Planer
UninstallDisplayIcon={app}\DartTournamentPlaner.exe
OutputBaseFilename=Setup-DartTournamentPlaner-{#MyAppVersion}
OutputDir=.\Output
Compression=lzma
SolidCompression=yes
WizardStyle=modern
LicenseFile=LICENSE.md

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Nur Dateien aus dem Build-Ordner einbinden
Source: "release\DartTournamentPlaner\bin\Release\net9.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Dart Tournament Planer"; Filename: "{app}\DartTournamentPlaner.exe"
Name: "{commondesktop}\Dart Tournament Planer"; Filename: "{app}\DartTournamentPlaner.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\DartTournamentPlaner.exe"; Description: "{cm:LaunchProgram,Dart Tournament Planer}"; Flags: nowait postinstall skipifsilent

[Code]
function IsDotNet9Installed(): Boolean;
begin
  { Prüfen, ob .NET 9 Desktop Runtime installiert ist (einfacher Ordnercheck) }
  if DirExists(ExpandConstant('{pf}\dotnet\shared\Microsoft.WindowsDesktop.App\9.0.0')) then
    Result := True
  else
    Result := False;
end;

function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  if not IsDotNet9Installed() then
  begin
    MsgBox('Für den Dart Tournament Planer wird die ".NET Desktop Runtime 9.0" benötigt.'#13#13 +
           'Die Installationsseite von Microsoft wird nun geöffnet.',
           mbInformation, MB_OK);

    ShellExec('open',
      'https://dotnet.microsoft.com/en-us/download/dotnet/9.0/runtime',
      '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
  end;

  Result := True;
end;

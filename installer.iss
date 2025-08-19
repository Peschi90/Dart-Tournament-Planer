; Inno Setup Skript für Dart Tournament Planer
; Deutsch + Englisch, mit Lizenz, Startmenü & Desktop-Verknüpfung
; Prüft auf .NET 9 Desktop Runtime

[Setup]
AppId={{D0E5E8F9-9B3E-4E23-AFAB-1234567890AB}}
AppName=Dart Tournament Planer
AppVersion=1.0.0
AppPublisher=Peschi90
AppPublisherURL=https://github.com/Peschi90/Dart-Tournament-Planer
DefaultDirName={pf}\DartTournamentPlaner
DefaultGroupName=Dart Tournament Planer
DisableProgramGroupPage=no
LicenseFile=LICENSE.md
OutputDir=.\OutputInstaller
OutputBaseFilename=DartTournamentPlanerSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\DartTournamentPlaner.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "DartTournamentPlaner\bin\Release\net9.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Dart Tournament Planer"; Filename: "{app}\DartTournamentPlaner.exe"
Name: "{commondesktop}\Dart Tournament Planer"; Filename: "{app}\DartTournamentPlaner.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\DartTournamentPlaner.exe"; Description: "{cm:LaunchProgram,Dart Tournament Planer}"; Flags: nowait postinstall skipifsilent

[Code]
function IsDotNet9Installed(): Boolean;
var
  Success: Boolean;
  ReleaseKey: Cardinal;
begin
  { Prüfen auf .NET 9 Desktop Runtime in der Registry }
  Success := RegQueryDWordValue(
    HKLM64,
    'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost',
    'Version',
    ReleaseKey
  );

  { Achtung: "ReleaseKey" ist hier nur ein Platzhalter, .NET Core/5+/6+/7+/8+/9 haben andere Registry-Keys.
    Deshalb prüfen wir über die Existenz des Ordners im dotnet-Installationspfad. }
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
           'Sie wird nun von der offiziellen Microsoft-Webseite heruntergeladen.',
           mbInformation, MB_OK);

    ShellExec('open',
      'https://dotnet.microsoft.com/en-us/download/dotnet/9.0/runtime',
      '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
  end;

  Result := True;
end;

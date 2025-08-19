; installer.iss
[Setup]
AppName=Dart Tournament Planer
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\Dart Tournament Planer
DefaultGroupName=Dart Tournament Planer
UninstallDisplayIcon={app}\DartTournamentPlaner.exe
OutputBaseFilename=Setup-DartTournamentPlaner-{#MyAppVersion}
Compression=lzma
SolidCompression=yes

[Files]
; Nur Dateien aus dem Build-Ordner einbinden
Source: "release\DartTournamentPlaner\bin\Release\net9.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Dart Tournament Planer"; Filename: "{app}\DartTournamentPlaner.exe"
Name: "{commondesktop}\Dart Tournament Planer"; Filename: "{app}\DartTournamentPlaner.exe"

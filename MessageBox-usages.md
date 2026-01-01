# MessageBox usages (standard Windows style)

Found via `findstr /S /N "MessageBox.Show" *.cs`.

- `DartTournamentPlaner/Views/HubTournamentSyncDialog.xaml.cs:300` – `MessageBox.Show($"Fehler beim Import: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);`
- `DartTournamentPlaner/Views/License/DonationSelectionDialog.xaml.cs:227` – error MessageBox
- `DartTournamentPlaner/Views/License/DonationSelectionDialog.xaml.cs:262` – info MessageBox (PayPal link)
- `DartTournamentPlaner/Views/License/LicenseActivationDialog.cs:629` – success MessageBox
- `DartTournamentPlaner/Views/License/LicenseActivationDialog.cs:647` – MessageBox (multi-line)
- `DartTournamentPlaner/Views/License/LicenseActivationSuccessDialog.cs:686` – MessageBox
- `DartTournamentPlaner/Views/License/SimpleLicenseActivationDialog.cs:154` – MessageBox
- `DartTournamentPlaner/Views/License/SimpleLicenseActivationDialog.cs:186` – MessageBox (invalid license format)
- `DartTournamentPlaner/Views/License/SimpleLicenseActivationDialog.cs:216` – warning MessageBox
- `DartTournamentPlaner/Views/License/SimpleLicenseActivationDialog.cs:218` – info MessageBox
- `DartTournamentPlaner/Views/License/SimpleLicenseActivationSuccessDialog.cs:227` – MessageBox
- `DartTournamentPlaner/Views/License/SimpleLicenseInfoDialog.cs:34` – MessageBox (error opening info)
- `DartTournamentPlaner/Views/License/TournamentOverviewLicenseRequiredDialog.xaml.cs:90` – MessageBox
- `DartTournamentPlaner/Views/MatchResultWindow.xaml.cs:531` – MessageBox
- `DartTournamentPlaner/Views/MatchResultWindow.xaml.cs:541` – MessageBox
- `DartTournamentPlaner/Views/MatchResultWindow.xaml.cs:567` – MessageBox
- `DartTournamentPlaner/Views/MatchResultWindow.xaml.cs:773` – MessageBox
- `DartTournamentPlaner/Views/MatchResultWindow.xaml.cs:807` – MessageBox
- `DartTournamentPlaner/Views/OverviewConfigDialog.cs:368` – MessageBox
- `DartTournamentPlaner/Views/OverviewConfigDialog.cs:390` – MessageBox
- `DartTournamentPlaner/Views/PowerScoringAdvancedConfigDialog.cs:192` – MessageBox
- `DartTournamentPlaner/Views/RoundRulesWindow.xaml.cs:340` – MessageBox
- `DartTournamentPlaner/Views/RoundRulesWindow.xaml.cs:352` – MessageBox
- `DartTournamentPlaner/Views/SettingsWindow.xaml.cs:160` – MessageBox
- `DartTournamentPlaner/Views/UpdateDialog.cs:771` – MessageBox

Note: Lines marked as commented indicate legacy code currently disabled but still present.

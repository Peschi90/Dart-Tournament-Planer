using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.IO;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Service für automatische Update-Überprüfung über GitHub Releases
/// Überprüft beim Start der Anwendung nach neuen Versionen und zeigt Changelog an
/// Unterstützt automatischen Download und Installation der Setup.exe
/// </summary>
public class UpdateService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly LocalizationService? _localizationService;
    private const string GITHUB_API_URL = "https://api.github.com/repos/Peschi90/Dart-Turnament-Planer/releases/latest";
    private const int TIMEOUT_SECONDS = 10;
    private const int DOWNLOAD_TIMEOUT_SECONDS = 120; // 2 Minuten für Download

    public UpdateService(LocalizationService? localizationService = null)
    {
        _localizationService = localizationService;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS)
        };
        
        // GitHub API Header für bessere Rate Limits
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DartTournamentPlaner-UpdateChecker");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    }

    /// <summary>
    /// Überprüft asynchron nach verfügbaren Updates
    /// </summary>
    /// <returns>UpdateInfo wenn Update verfügbar, sonst null</returns>
    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== UpdateService.CheckForUpdatesAsync START ===");
            
            var currentVersion = GetCurrentVersion();
            System.Diagnostics.Debug.WriteLine($"UpdateService: Current version = {currentVersion}");
            
            // GitHub API Aufruf
            var response = await _httpClient.GetAsync(GITHUB_API_URL);
            
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateService: GitHub API call failed with status: {response.StatusCode}");
                return null;
            }
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"UpdateService: Received JSON response length: {jsonContent.Length}");
            
            // Parse GitHub Release Response
            var releaseInfo = ParseGitHubRelease(jsonContent);
            
            if (releaseInfo == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdateService: Failed to parse GitHub release info");
                return null;
            }
            
            System.Diagnostics.Debug.WriteLine($"UpdateService: Latest release = {releaseInfo.TagName} ({releaseInfo.PublishedAt})");
            
            // Vergleiche Versionen
            if (IsNewerVersion(currentVersion, releaseInfo.TagName))
            {
                System.Diagnostics.Debug.WriteLine("UpdateService: New version available!");
                
                // Suche Setup.exe in Assets
                var setupDownloadUrl = FindSetupExeDownloadUrl(releaseInfo.Assets);
                
                return new UpdateInfo
                {
                    CurrentVersion = currentVersion,
                    LatestVersion = releaseInfo.TagName,
                    ReleaseNotes = releaseInfo.Body ?? "",
                    DownloadUrl = releaseInfo.HtmlUrl ?? "",
                    SetupDownloadUrl = setupDownloadUrl, // Neue Eigenschaft für direkten Setup-Download
                    ReleaseName = releaseInfo.Name ?? releaseInfo.TagName,
                    PublishedAt = releaseInfo.PublishedAt,
                    IsPrerelease = releaseInfo.Prerelease
                };
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("UpdateService: No new version available");
                return null;
            }
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("UpdateService: Request timed out");
            return null;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateService: HTTP error: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateService: Unexpected error: {ex.Message}");
            return null;
        }
        finally
        {
            System.Diagnostics.Debug.WriteLine("=== UpdateService.CheckForUpdatesAsync END ===");
        }
    }

    /// <summary>
    /// Lädt das Update herunter und startet die Installation
    /// </summary>
    /// <param name="updateInfo">Update-Informationen</param>
    /// <param name="progress">Progress-Callback für Download-Fortschritt</param>
    /// <returns>True wenn erfolgreich heruntergeladen und gestartet</returns>
    public async Task<bool> DownloadAndInstallUpdateAsync(UpdateInfo updateInfo, IProgress<(string Status, int Percentage)>? progress = null)
    {
        if (string.IsNullOrEmpty(updateInfo.SetupDownloadUrl))
        {
            System.Diagnostics.Debug.WriteLine("UpdateService: No setup download URL available");
            return false;
        }

        string setupFilePath = "";
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== UpdateService.DownloadAndInstallUpdateAsync START ===");
            progress?.Report((_localizationService?.GetTranslation("PreparingDownload") ?? "Bereite Download vor...", 0));

            // Temporären Pfad für Setup.exe erstellen
            var tempPath = Path.GetTempPath();
            var setupFileName = $"Setup-DartTournamentPlaner-{updateInfo.LatestVersion}.exe";
            setupFilePath = Path.Combine(tempPath, setupFileName);

            // Falls Datei bereits existiert, lösche sie zuerst
            if (File.Exists(setupFilePath))
            {
                try
                {
                    File.Delete(setupFilePath);
                    System.Diagnostics.Debug.WriteLine($"UpdateService: Deleted existing setup file: {setupFilePath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateService: Could not delete existing file: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"UpdateService: Downloading to: {setupFilePath}");
            progress?.Report((_localizationService?.GetTranslation("DownloadingSetup") ?? "Lade Setup herunter...", 5));

            // Setup.exe herunterladen mit expliziter Ressourcenverwaltung
            using (var downloadClient = new HttpClient { Timeout = TimeSpan.FromSeconds(DOWNLOAD_TIMEOUT_SECONDS) })
            {
                downloadClient.DefaultRequestHeaders.Add("User-Agent", "DartTournamentPlaner-Updater");

                var response = await downloadClient.GetAsync(updateInfo.SetupDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                System.Diagnostics.Debug.WriteLine($"UpdateService: Download size: {totalBytes} bytes");

                // Explizite Stream-Verwaltung mit using-Statements
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(setupFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true))
                {
                    var buffer = new byte[8192];
                    long downloadedBytes = 0;
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        downloadedBytes += bytesRead;

                        if (totalBytes > 0)
                        {
                            var percentage = Math.Min(95, (int)(downloadedBytes * 90 / totalBytes) + 5); // 5-95%
                            progress?.Report(($"Lade Setup herunter... ({downloadedBytes:N0} / {totalBytes:N0} Bytes)", percentage));
                        }
                    }

                    // Stelle sicher, dass alle Daten geschrieben wurden
                    await fileStream.FlushAsync();
                }

                // Explizit Response-Objekt freigeben
                response.Dispose();
            }

            progress?.Report((_localizationService?.GetTranslation("DownloadCompleted") ?? "Download abgeschlossen, prüfe Datei...", 96));
            
            // Kurze Verzögerung um sicherzustellen, dass alle Handles geschlossen sind
            await Task.Delay(500);

            // Überprüfen ob Datei existiert und gültig ist
            var fileInfo = new FileInfo(setupFilePath);
            if (!fileInfo.Exists)
            {
                System.Diagnostics.Debug.WriteLine("UpdateService: Downloaded file does not exist");
                return false;
            }

            if (fileInfo.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("UpdateService: Downloaded file is empty");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"UpdateService: Downloaded file size: {fileInfo.Length} bytes");

            // Weitere Verzögerung um File-Handles sicherzustellen
            await Task.Delay(1000);
            progress?.Report((_localizationService?.GetTranslation("PreparingInstallation") ?? "Bereite Installation vor...", 98));

            // Setup.exe starten mit erweiterten Optionen
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = setupFilePath,
                UseShellExecute = true,
                Verb = "runas", // Administratorrechte anfordern
                WorkingDirectory = Path.GetDirectoryName(setupFilePath) ?? tempPath
            };

            System.Diagnostics.Debug.WriteLine($"UpdateService: Starting setup: {setupFilePath}");
            System.Diagnostics.Debug.WriteLine($"UpdateService: Working directory: {startInfo.WorkingDirectory}");
            
            progress?.Report((_localizationService?.GetTranslation("StartingInstallation") ?? "Starte Installation...", 100));

            // Versuche den Prozess zu starten
            var process = System.Diagnostics.Process.Start(startInfo);

            if (process != null)
            {
                System.Diagnostics.Debug.WriteLine("UpdateService: Setup started successfully");
                
                // Kurz warten um sicherzustellen, dass der Prozess läuft
                await Task.Delay(2000);
                
                if (!process.HasExited)
                {
                    System.Diagnostics.Debug.WriteLine("UpdateService: Setup process is running");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateService: Setup process exited immediately with code: {process.ExitCode}");
                    return false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("UpdateService: Failed to start setup - process is null");
                return false;
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateService: Access denied: {ex.Message}");
            progress?.Report((_localizationService?.GetTranslation("AdminRightsRequired") ?? "Fehler: Administratorrechte erforderlich", -1));
            return false;
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateService: Win32 error: {ex.Message} (Error Code: {ex.ErrorCode})");
            
            if (ex.ErrorCode == -2147467259) // User cancelled UAC
            {
                progress?.Report((_localizationService?.GetTranslation("InstallationCancelled") ?? "Installation abgebrochen", -1));
            }
            else
            {
                progress?.Report((_localizationService?.GetTranslation("ErrorStartingSetup") ?? "Fehler beim Starten", -1));
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateService: Download/Install error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"UpdateService: Stack trace: {ex.StackTrace}");
            progress?.Report(($"Fehler: {ex.Message}", -1));
            return false;
        }
        finally
        {
            // Cleanup: Versuche temporäre Datei zu löschen (nur wenn Installation fehlgeschlagen)
            if (!string.IsNullOrEmpty(setupFilePath) && File.Exists(setupFilePath))
            {
                try
                {
                    // Warte ein bisschen und versuche dann zu löschen
                    await Task.Delay(3000);
                    if (File.Exists(setupFilePath))
                    {
                        File.Delete(setupFilePath);
                        System.Diagnostics.Debug.WriteLine("UpdateService: Cleaned up temporary setup file");
                    }
                }
                catch
                {
                    // Ignore cleanup errors - temporäre Dateien werden automatisch bereinigt
                    System.Diagnostics.Debug.WriteLine("UpdateService: Could not clean up temporary file (will be cleaned automatically)");
                }
            }
            
            System.Diagnostics.Debug.WriteLine("=== UpdateService.DownloadAndInstallUpdateAsync END ===");
        }
    }

    /// <summary>
    /// Sucht die Setup.exe Download-URL in den GitHub Release Assets
    /// </summary>
    private string? FindSetupExeDownloadUrl(List<GitHubAssetInfo> assets)
    {
        if (assets == null || assets.Count == 0)
            return null;

        System.Diagnostics.Debug.WriteLine($"UpdateService: Searching for setup.exe in {assets.Count} assets:");
        foreach (var asset in assets)
        {
            System.Diagnostics.Debug.WriteLine($"  Asset: {asset.Name} ({asset.Size} bytes)");
        }

        // Erste Priorität: Exakter Match für Setup-DartTournamentPlaner-v*.exe
        var setupAsset = assets.FirstOrDefault(asset => 
            !string.IsNullOrEmpty(asset.Name) && 
            asset.Name.StartsWith("Setup-DartTournamentPlaner-v", StringComparison.OrdinalIgnoreCase) &&
            asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        if (setupAsset != null)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateService: Found exact setup file: {setupAsset.Name}");
            return setupAsset.DownloadUrl;
        }

        // Zweite Priorität: Setup-DartTournamentPlaner ohne v-Prefix
        setupAsset = assets.FirstOrDefault(asset => 
            !string.IsNullOrEmpty(asset.Name) && 
            asset.Name.StartsWith("Setup-DartTournamentPlaner-", StringComparison.OrdinalIgnoreCase) &&
            asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        if (setupAsset != null)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateService: Found setup file without v-prefix: {setupAsset.Name}");
            return setupAsset.DownloadUrl;
        }

        // Dritte Priorität: Beliebiges Setup-*.exe
        setupAsset = assets.FirstOrDefault(asset => 
            !string.IsNullOrEmpty(asset.Name) && 
            asset.Name.StartsWith("Setup-", StringComparison.OrdinalIgnoreCase) &&
            asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        if (setupAsset != null)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateService: Found generic setup file: {setupAsset.Name}");
            return setupAsset.DownloadUrl;
        }

        // Vierte Priorität: Beliebige .exe Datei (aber nicht zu klein - wahrscheinlich kein Setup)
        var exeAsset = assets.FirstOrDefault(asset => 
            !string.IsNullOrEmpty(asset.Name) && 
            asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
            asset.Size > 1024 * 100); // Mindestens 100KB

        if (exeAsset != null)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateService: Found fallback exe file: {exeAsset.Name} ({exeAsset.Size} bytes)");
            return exeAsset.DownloadUrl;
        }

        System.Diagnostics.Debug.WriteLine("UpdateService: No suitable setup.exe found in release assets");
        return null;
    }

    /// <summary>
    /// Ermittelt die aktuelle Version der Anwendung
    /// </summary>
    private string GetCurrentVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            
            if (version != null)
            {
                // Format: Major.Minor.Build.Revision -> Major.Minor.Build
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            
            return "1.0.0"; // Fallback
        }
        catch
        {
            return "1.0.0"; // Fallback bei Fehlern
        }
    }

    /// <summary>
    /// Parst die GitHub Release API Response
    /// </summary>
    private GitHubReleaseInfo? ParseGitHubRelease(string jsonContent)
    {
        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonContent);
            var root = jsonDocument.RootElement;

            // Parse Assets Array
            var assets = new List<GitHubAssetInfo>();
            if (root.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var assetElement in assetsElement.EnumerateArray())
                {
                    var asset = new GitHubAssetInfo
                    {
                        Name = assetElement.GetProperty("name").GetString() ?? "",
                        DownloadUrl = assetElement.GetProperty("browser_download_url").GetString() ?? "",
                        Size = assetElement.GetProperty("size").GetInt64(),
                        ContentType = assetElement.GetProperty("content_type").GetString() ?? ""
                    };
                    assets.Add(asset);
                }
            }

            return new GitHubReleaseInfo
            {
                TagName = root.GetProperty("tag_name").GetString() ?? "",
                Name = root.GetProperty("name").GetString(),
                Body = root.GetProperty("body").GetString(),
                HtmlUrl = root.GetProperty("html_url").GetString(),
                PublishedAt = root.GetProperty("published_at").TryGetDateTime(out var publishedAt) ? publishedAt : DateTime.Now,
                Prerelease = root.GetProperty("prerelease").GetBoolean(),
                Assets = assets
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ParseGitHubRelease: Error parsing JSON: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Vergleicht zwei Versionsstrings und prüft ob die neue Version neuer ist
    /// Unterstützt verschiedene Versionsformate (v1.0.0, 1.0.0, etc.)
    /// </summary>
    private bool IsNewerVersion(string currentVersion, string latestVersion)
    {
        try
        {
            // Bereinige Versionsstrings (entferne 'v' Prefix, etc.)
            var cleanCurrent = CleanVersionString(currentVersion);
            var cleanLatest = CleanVersionString(latestVersion);
            
            System.Diagnostics.Debug.WriteLine($"UpdateService: Comparing versions: {cleanCurrent} vs {cleanLatest}");
            
            // Parse zu Version-Objekten
            if (Version.TryParse(cleanCurrent, out var current) && 
                Version.TryParse(cleanLatest, out var latest))
            {
                var isNewer = latest > current;
                System.Diagnostics.Debug.WriteLine($"UpdateService: Version comparison result: {isNewer}");
                return isNewer;
            }
            
            // Fallback: String-Vergleich
            var stringCompare = string.Compare(cleanLatest, cleanCurrent, StringComparison.OrdinalIgnoreCase) > 0;
            System.Diagnostics.Debug.WriteLine($"UpdateService: Fallback string comparison: {stringCompare}");
            return stringCompare;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateService: Version comparison error: {ex.Message}");
            return false; // Bei Fehler keine Aktualisierung anbieten
        }
    }

    /// <summary>
    /// Bereinigt einen Versionsstring für den Vergleich
    /// </summary>
    private string CleanVersionString(string version)
    {
        if (string.IsNullOrEmpty(version))
            return "0.0.0";

        // Entferne 'v' am Anfang
        version = version.TrimStart('v', 'V');
        
        // Extrahiere nur die Versionsnummer (Major.Minor.Build.Revision)
        var match = Regex.Match(version, @"(\d+(?:\.\d+){0,3})");
        if (match.Success)
        {
            var versionParts = match.Value.Split('.');
            
            // Stelle sicher, dass wir mindestens 3 Teile haben (Major.Minor.Build)
            if (versionParts.Length == 1)
                return $"{versionParts[0]}.0.0";
            else if (versionParts.Length == 2)
                return $"{versionParts[0]}.{versionParts[1]}.0";
            else
                return match.Value;
        }
        
        return "0.0.0";
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Datenklasse für Update-Informationen
/// </summary>
public class UpdateInfo
{
    public string CurrentVersion { get; set; } = "";
    public string LatestVersion { get; set; } = "";
    public string ReleaseNotes { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string? SetupDownloadUrl { get; set; } = ""; // Direkte URL zur Setup.exe
    public string ReleaseName { get; set; } = "";
    public DateTime PublishedAt { get; set; }
    public bool IsPrerelease { get; set; }

    /// <summary>
    /// Prüft ob automatischer Download möglich ist
    /// </summary>
    public bool CanAutoDownload => !string.IsNullOrEmpty(SetupDownloadUrl);

    /// <summary>
    /// Formatiert die Release Notes für die Anzeige
    /// Konvertiert Markdown zu einfachem Text mit besserer Lesbarkeit
    /// </summary>
    public string GetFormattedReleaseNotes()
    {
        if (string.IsNullOrEmpty(ReleaseNotes))
            return "Keine Informationen zu den Änderungen verfügbar.";

        var formatted = ReleaseNotes;

        try
        {
            // Einfache Markdown-zu-Text Konvertierung
            // Entferne GitHub-spezifische Syntax
            formatted = Regex.Replace(formatted, @"<!--.*?-->", "", RegexOptions.Singleline);
            
            // Konvertiere Headers
            formatted = Regex.Replace(formatted, @"^###\s*(.*)", "• $1", RegexOptions.Multiline);
            formatted = Regex.Replace(formatted, @"^##\s*(.*)", "▪ $1", RegexOptions.Multiline);
            formatted = Regex.Replace(formatted, @"^#\s*(.*)", "■ $1", RegexOptions.Multiline);
            
            // Konvertiere Listen
            formatted = Regex.Replace(formatted, @"^[\s]*[-*+]\s+(.*)", "  → $1", RegexOptions.Multiline);
            
            // Entferne Links aber behalte den Text
            formatted = Regex.Replace(formatted, @"\[([^\]]+)\]\([^)]+\)", "$1");
            
            // Entferne Code-Blöcke (```code```
            formatted = Regex.Replace(formatted, @"```[^`]*```", "[Code-Änderungen]", RegexOptions.Singleline);
            formatted = Regex.Replace(formatted, @"`([^`]+)`", "$1");
            
            // Bereinige excessive Whitespace
            formatted = Regex.Replace(formatted, @"\n{3,}", "\n\n");
            formatted = formatted.Trim();

            // Falls immer noch leer, zeige Fallback
            if (string.IsNullOrWhiteSpace(formatted))
                return "Neue Version mit Verbesserungen und Fehlerbehebungen verfügbar.";

            return formatted;
        }
        catch
        {
            // Fallback bei Parsing-Fehlern
            return ReleaseNotes.Length > 500 ? ReleaseNotes.Substring(0, 500) + "..." : ReleaseNotes;
        }
    }
}

/// <summary>
/// Interne Datenklasse für GitHub Release API Response
/// </summary>
internal class GitHubReleaseInfo
{
    public string TagName { get; set; } = "";
    public string? Name { get; set; }
    public string? Body { get; set; }
    public string? HtmlUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool Prerelease { get; set; }
    public List<GitHubAssetInfo> Assets { get; set; } = new();
}

/// <summary>
/// Interne Datenklasse für GitHub Release Assets
/// </summary>
internal class GitHubAssetInfo
{
    public string Name { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public long Size { get; set; }
    public string ContentType { get; set; } = "";
}
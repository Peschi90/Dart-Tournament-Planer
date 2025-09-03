using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Management;
using System.Net.NetworkInformation;
using DartTournamentPlaner.Models.License;
using System.Linq;

namespace DartTournamentPlaner.Services.License;

/// <summary>
/// License Manager für Dart Tournament Planner
/// Basiert auf der License Server Integration Guide v1.3.0
/// </summary>
public class LicenseManager
{
    private const string LICENSE_SERVER_URL = "https://license-dtp.i3ull3t.de";
    private const int VALIDATION_INTERVAL_HOURS = 24;
    private const string REGISTRY_KEY = @"HKEY_CURRENT_USER\SOFTWARE\DartTournamentPlanner";
    private const string LICENSE_KEY_VALUE = "LicenseKey";
    private const string LAST_VALIDATION_VALUE = "LastValidation";
    
    private readonly HttpClient _httpClient;
    private DateTime _lastValidation;
    private bool _isValid;
    private LicenseData? _cachedLicenseData;
    
    public event EventHandler<LicenseStatusChangedEventArgs>? LicenseStatusChanged;
    
    public LicenseManager()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }
    
    /// <summary>
    /// Generiert eine Hardware-ID basierend auf mehreren System-Parametern
    /// </summary>
    public static string GenerateHardwareId()
    {
        var components = new List<string>();
        
        // CPU ID
        try 
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                components.Add(obj["ProcessorId"]?.ToString() ?? "");
                break; // Nur erste CPU
            }
        }
        catch { /* Fallback */ }
        
        // Motherboard Serial
        try 
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
            {
                components.Add(obj["SerialNumber"]?.ToString() ?? "");
                break;
            }
        }
        catch { /* Fallback */ }
        
        // MAC Address (erste physische Karte)
        try 
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                    continue;
                    
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    components.Add(nic.GetPhysicalAddress().ToString());
                    break;
                }
            }
        }
        catch { /* Fallback */ }
        
        // Fallback: Windows Installation ID
        if (components.Count == 0)
        {
            try 
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                components.Add(key?.GetValue("InstallDate")?.ToString() ?? Environment.MachineName);
            }
            catch 
            {
                components.Add(Environment.MachineName);
            }
        }
        
        var combined = string.Join("|", components.Where(c => !string.IsNullOrEmpty(c)));
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hash).Replace("+", "").Replace("/", "").Replace("=", "")[..32];
    }
    
    /// <summary>
    /// Validiert die aktuelle Lizenz
    /// </summary>
    public async Task<LicenseValidationResult> ValidateLicenseAsync()
    {
        // Nur validieren wenn nötig
        if (_isValid && DateTime.Now - _lastValidation < TimeSpan.FromHours(VALIDATION_INTERVAL_HOURS))
        {
            return new LicenseValidationResult 
            { 
                IsValid = true, 
                Cached = true,
                Data = _cachedLicenseData
            };
        }
        
        var licenseKey = LoadLicenseKey();
        if (string.IsNullOrEmpty(licenseKey))
        {
            return new LicenseValidationResult 
            { 
                IsValid = false, 
                ErrorType = LicenseErrorType.LicenseNotFound,
                Message = "No license key found" 
            };
        }
        
        var hardwareId = GenerateHardwareId();
        var result = await ValidateLicenseWithServerAsync(licenseKey, hardwareId);
        
        _isValid = result.IsValid;
        _lastValidation = DateTime.Now;
        _cachedLicenseData = result.Data;
        
        // Speichere letzte erfolgreiche Validierung
        if (result.IsValid)
        {
            SaveLastValidation(_lastValidation);
        }
        
        // Event auslösen
        LicenseStatusChanged?.Invoke(this, new LicenseStatusChangedEventArgs(result));
        
        return result;
    }
    
    /// <summary>
    /// Aktiviert eine neue Lizenz
    /// </summary>
    public async Task<LicenseValidationResult> ActivateLicenseAsync(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            return new LicenseValidationResult
            {
                IsValid = false,
                Message = "License key cannot be empty",
                ErrorType = LicenseErrorType.InvalidFormat
            };
        }
        
        var hardwareId = GenerateHardwareId();
        
        try
        {
            Debug.WriteLine($"🔑 Activating license: {licenseKey.Substring(0, Math.Min(9, licenseKey.Length))}...");
            Debug.WriteLine($"🖥️ Hardware ID: {hardwareId}");
            
            // Server-Validierung
            var validationResult = await ValidateLicenseWithServerAsync(licenseKey, hardwareId);
            
            if (validationResult.IsValid)
            {
                // Lizenz lokal speichern
                SaveLicenseKey(licenseKey);
                _lastValidation = DateTime.Now;
                
                Debug.WriteLine($"✅ License activated successfully");
                
                // Warnung bei wenigen Aktivierungen
                if (validationResult.Data?.RemainingActivations <= 1)
                {
                    validationResult.ShowActivationWarning = true;
                }
                
                // Event auslösen
                OnLicenseStatusChanged(validationResult);
                
                return validationResult;
            }
            else
            {
                Debug.WriteLine($"❌ License activation failed: {validationResult.Message}");
                return validationResult;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ License activation exception: {ex.Message}");
            
            return new LicenseValidationResult
            {
                IsValid = false,
                Message = ex.Message,
                ErrorType = LicenseErrorType.NetworkError
            };
        }
    }
    
    /// <summary>
    /// Validiert Lizenz mit dem Server
    /// </summary>
    private async Task<LicenseValidationResult> ValidateLicenseWithServerAsync(string licenseKey, string hardwareId)
    {
        try
        {
            var productVersion = GetProductVersion();
            
            // Konvertiere zu Server-Format (ohne DART- Präfix)
            var serverLicenseKey = ConvertToServerFormat(licenseKey);
            
            var request = new
            {
                licenseKey = serverLicenseKey,
                hardwareId = hardwareId,
                productVersion = productVersion
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Debug.WriteLine($"🌐 Validating license with server: {LICENSE_SERVER_URL}");
            Debug.WriteLine($"🔑 Client license key: {licenseKey}");
            Debug.WriteLine($"🔑 Server license key: {serverLicenseKey}");
            
            var response = await _httpClient.PostAsync($"{LICENSE_SERVER_URL}/api/v1/license/validate", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"📝 Server response: {response.StatusCode}");
            Debug.WriteLine($"📄 Response body: {responseJson}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var jsonOptions = new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true,
                        Converters = { new FeaturesJsonConverter() }
                    };
                    
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<LicenseData>>(responseJson, jsonOptions);

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        _cachedLicenseData = apiResponse.Data;
                        SaveLastValidation(DateTime.Now);

                        return new LicenseValidationResult
                        {
                            IsValid = true,
                            Message = apiResponse.Message ?? "License is valid",
                            Data = apiResponse.Data,
                            Offline = false
                        };
                    }
                    else
                    {
                        return new LicenseValidationResult
                        {
                            IsValid = false,
                            Message = apiResponse?.Message ?? "Invalid server response",
                            ErrorType = LicenseErrorType.ServerError
                        };
                    }
                }
                catch (JsonException jsonEx)
                {
                    Debug.WriteLine($"❌ JSON parsing error: {jsonEx.Message}");
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        Message = $"Invalid server response format: {jsonEx.Message}",
                        ErrorType = LicenseErrorType.ServerError
                    };
                }
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseJson, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                return new LicenseValidationResult
                {
                    IsValid = false,
                    Message = errorResponse?.Message ?? $"Server error: {response.StatusCode}",
                    ErrorType = DetermineErrorType(response.StatusCode, errorResponse?.Message ?? "")
                };
            }
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"❌ Network error: {httpEx.Message}");
            return new LicenseValidationResult
            {
                IsValid = false,
                Message = $"Network error: {httpEx.Message}",
                ErrorType = LicenseErrorType.NetworkError,
                Offline = true
            };
        }
        catch (TaskCanceledException timeoutEx)
        {
            Debug.WriteLine($"❌ Timeout error: {timeoutEx.Message}");
            return new LicenseValidationResult
            {
                IsValid = false,
                Message = "Request timeout - please check your internet connection",
                ErrorType = LicenseErrorType.NetworkError,
                Offline = true
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Unexpected error: {ex.Message}");
            return new LicenseValidationResult
            {
                IsValid = false,
                Message = $"Unexpected error: {ex.Message}",
                ErrorType = LicenseErrorType.ServerError
            };
        }
    }
    
    /// <summary>
    /// Bestimmt den Fehlertyp basierend auf HTTP Status und Nachricht
    /// </summary>
    private static LicenseErrorType DetermineErrorType(System.Net.HttpStatusCode statusCode, string message)
    {
        return statusCode switch
        {
            System.Net.HttpStatusCode.BadRequest when message.Contains("format") => LicenseErrorType.InvalidFormat,
            System.Net.HttpStatusCode.NotFound => LicenseErrorType.LicenseNotFound,
            System.Net.HttpStatusCode.Forbidden when message.Contains("expired") => LicenseErrorType.LicenseExpired,
            System.Net.HttpStatusCode.Forbidden when message.Contains("inactive") => LicenseErrorType.LicenseInactive,
            System.Net.HttpStatusCode.Forbidden when message.Contains("activation") => LicenseErrorType.MaxActivationsReached,
            _ => LicenseErrorType.ServerError
        };
    }
    
    /// <summary>
    /// Lädt den Lizenzschlüssel aus der Registry
    /// </summary>
    private static string? LoadLicenseKey()
    {
        try
        {
            return Registry.GetValue(REGISTRY_KEY, LICENSE_KEY_VALUE, null)?.ToString();
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Speichert den Lizenzschlüssel in der Registry
    /// </summary>
    private static void SaveLicenseKey(string licenseKey)
    {
        try
        {
            Registry.SetValue(REGISTRY_KEY, LICENSE_KEY_VALUE, licenseKey);
        }
        catch (Exception ex)
        {
            throw new LicenseException($"Failed to save license key: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Lädt die letzte Validierungszeit
    /// </summary>
    private static DateTime? LoadLastValidation()
    {
        try
        {
            var value = Registry.GetValue(REGISTRY_KEY, LAST_VALIDATION_VALUE, null)?.ToString();
            if (DateTime.TryParse(value, out var result))
            {
                return result;
            }
        }
        catch
        {
            // Ignore
        }
        return null;
    }
    
    /// <summary>
    /// Speichert die letzte Validierungszeit
    /// </summary>
    private static void SaveLastValidation(DateTime validationTime)
    {
        try
        {
            Registry.SetValue(REGISTRY_KEY, LAST_VALIDATION_VALUE, validationTime.ToString("O"));
        }
        catch
        {
            // Ignore - nicht kritisch
        }
    }
    
    /// <summary>
    /// Holt die aktuelle Produktversion
    /// </summary>
    private static string GetClientVersion()
    {
        try
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version?.ToString() ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }
    
    private string GetProductVersion()
    {
        return GetClientVersion();
    }
    
    /// <summary>
    /// Gibt die aktuelle Lizenz-Information zurück (ohne Server-Validierung)
    /// </summary>
    public LicenseData? GetCurrentLicenseInfo()
    {
        return _cachedLicenseData;
    }
    
    /// <summary>
    /// Prüft ob eine Lizenz vorhanden ist
    /// </summary>
    public bool HasLicense()
    {
        return !string.IsNullOrEmpty(LoadLicenseKey());
    }
    
    /// <summary>
    /// Entfernt die gespeicherte Lizenz vollständig und setzt den internen Zustand zurück
    /// </summary>
    public void RemoveLicense()
    {
        try
        {
            Debug.WriteLine($"🗑️ Starting license removal process...");
            
            // Registry-Schlüssel vollständig löschen
            try
            {
                Registry.CurrentUser.DeleteSubKey(@"SOFTWARE\DartTournamentPlanner", false);
                Debug.WriteLine($"✅ Registry key deleted successfully");
            }
            catch (ArgumentException)
            {
                Debug.WriteLine($"⚠️ Registry key not found - already removed");
            }
            
            // Internen Zustand zurücksetzen
            _lastValidation = DateTime.MinValue;
            _isValid = false;
            _cachedLicenseData = null;
            
            Debug.WriteLine($"🗑️ License removed successfully - internal state cleared");
            
            // Event auslösen mit ungültigem Status
            var invalidResult = new LicenseValidationResult
            {
                IsValid = false,
                Message = "License removed",
                ErrorType = LicenseErrorType.LicenseNotFound
            };
            
            OnLicenseStatusChanged(invalidResult);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error removing license: {ex.Message}");
            throw new InvalidOperationException($"Could not remove license: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Entfernt die Lizenz und initialisiert das System für eine neue Lizenz-Eingabe
    /// </summary>
    public async Task<bool> RemoveLicenseAndResetAsync()
    {
        try
        {
            Debug.WriteLine($"🔄 Starting complete license reset...");
            
            // Lizenz entfernen
            RemoveLicense();
            
            // Kurz warten um sicherzustellen, dass alle Events verarbeitet wurden
            await Task.Delay(100);
            
            Debug.WriteLine($"✅ License reset completed - ready for new license");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error during license reset: {ex.Message}");
            return false;
        }
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
    
    private void OnLicenseStatusChanged(LicenseValidationResult result)
    {
        try
        {
            var eventArgs = new LicenseStatusChangedEventArgs(result);
            LicenseStatusChanged?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error raising LicenseStatusChanged event: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Normalisiert einen Lizenzschlüssel (entfernt nur Leerzeichen, behält Bindestriche)
    /// </summary>
    private static string NormalizeLicenseKey(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return licenseKey;
            
        // Nur Leerzeichen entfernen, Bindestriche beibehalten, zu Großbuchstaben
        return licenseKey.Replace(" ", "").ToUpperInvariant();
    }
    
    /// <summary>
    /// Konvertiert einen Lizenzschlüssel zu Server-Format (MIT Bindestrichen!)
    /// Der Server erwartet exakt: BDF6-192D-E8BE-4178-B160-C6C3-6018-0FE3
    /// </summary>
    private static string ConvertToServerFormat(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return licenseKey;
            
        var normalized = licenseKey.Replace(" ", "").ToUpperInvariant();
        
        // Falls Bindestriche fehlen, hinzufügen
        if (!normalized.Contains("-") && normalized.Length == 32)
        {
            // Format: BDF6192DE8BE4178B160C6C360180FE3 -> BDF6-192D-E8BE-4178-B160-C6C3-6018-0FE3
            var formatted = "";
            for (int i = 0; i < normalized.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    formatted += "-";
                formatted += normalized[i];
            }
            return formatted;
        }
        
        // Server erwartet das Format MIT Bindestrichen: BDF6-192D-E8BE-4178-B160-C6C3-6018-0FE3
        return normalized;
    }
}
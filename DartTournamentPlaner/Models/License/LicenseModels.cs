using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DartTournamentPlaner.Models.License;

/// <summary>
/// Custom JsonConverter für Features - behandelt sowohl String[] als auch JSON-String
/// </summary>
public class FeaturesJsonConverter : JsonConverter<string[]>
{
    public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Normaler String-Array: ["feature1", "feature2"]
            var features = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;
                if (reader.TokenType == JsonTokenType.String)
                {
                    var feature = reader.GetString();
                    if (!string.IsNullOrEmpty(feature))
                        features.Add(feature);
                }
            }
            return features.ToArray();
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            // JSON-String: "[\"feature1\",\"feature2\"]"
            var jsonString = reader.GetString();
            if (string.IsNullOrEmpty(jsonString))
                return new string[0];
            
            try
            {
                return JsonSerializer.Deserialize<string[]>(jsonString) ?? new string[0];
            }
            catch
            {
                // Fallback: als Komma-getrennte Liste behandeln
                return jsonString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
        }
        
        return new string[0];
    }

    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}

/// <summary>
/// Lizenz-Datenmodell basierend auf der API-Dokumentation
/// </summary>
public class LicenseData
{
    public string? LicenseKey { get; set; }
    public string? CustomerName { get; set; }
    public string? ProductName { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    [JsonConverter(typeof(FeaturesJsonConverter))]
    public string[]? Features { get; set; }
    
    public int? RemainingActivations { get; set; }
    public string? Status { get; set; }
    public int? MaxActivations { get; set; }
    public int? CurrentActivations { get; set; }
    public LicenseMetadata? Metadata { get; set; }
    /// <summary>
    /// Version des License Servers (falls verfügbar)
    /// </summary>
    public string? ServerVersion { get; set; }
}

/// <summary>
/// Lizenz-Metadata (neu in v1.3.0)
/// </summary>
public class LicenseMetadata
{
    public string? ServerVersion { get; set; }
    public string? GeneratedBy { get; set; }
    public DateTime? GeneratedAt { get; set; }
}

/// <summary>
/// API Response-Wrapper für Lizenz-Server Antworten
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

/// <summary>
/// Lizenz-Validierungsergebnis
/// </summary>
public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public LicenseErrorType ErrorType { get; set; }
    public LicenseData? Data { get; set; }
    public bool Cached { get; set; }
    public bool Offline { get; set; }
    /// <summary>
    /// Gibt an, ob das Validierungs-/Aktivierungsresultat eine Warnung auslösen soll
    /// </summary>
    public bool ShowActivationWarning { get; set; }
}

/// <summary>
/// Arten von Lizenzfehlern
/// </summary>
public enum LicenseErrorType
{
    None,
    NetworkError,
    LicenseNotFound,
    LicenseExpired,
    LicenseInactive,
    MaxActivationsReached,
    InvalidFormat,
    ServerError
}

/// <summary>
/// Event-Args für Lizenz-Status-Änderungen
/// </summary>
public class LicenseStatusChangedEventArgs : EventArgs
{
    public LicenseValidationResult ValidationResult { get; }
    
    public LicenseStatusChangedEventArgs(LicenseValidationResult validationResult)
    {
        ValidationResult = validationResult;
    }
}

/// <summary>
/// Lizenz-Exception
/// </summary>
public class LicenseException : Exception
{
    public LicenseException(string message) : base(message) { }
    public LicenseException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Bekannte Feature-Flags für Dart Tournament Planner
/// </summary>
public static class LicenseFeatures
{
    public const string TOURNAMENT_MANAGEMENT = "tournament_management";
    public const string PLAYER_TRACKING = "player_tracking";
    public const string STATISTICS = "statistics";  // NEU: Statistics Feature
    public const string API_ACCESS = "api_access";  // Deprecated - verwende API_CONNECTION
    public const string API_CONNECTION = "api_connection";  // NEU: Korrekter API Feature Name
    public const string HUB_INTEGRATION = "hub_integration";  // Deprecated - verwende HUB_CONNECTION
    public const string HUB_CONNECTION = "hub_connection";  // NEU: Korrekter Hub Feature Name
    public const string MULTI_TOURNAMENT = "multi_tournament";
    public const string ADVANCED_REPORTING = "advanced_reporting";
    public const string ENHANCED_PRINTING = "print";  // KORRIGIERT: Verwendet "print" wie in der Lizenz
    public const string CUSTOM_THEMES = "custom_themes";
    public const string PREMIUM_SUPPORT = "premium_support";
    public const string TOURNAMENT_OVERVIEW = "tournament_overview";  // NEU: Tournament Overview Feature
}

/// <summary>
/// Lizenz-Status für UI-Anzeige
/// </summary>
public class LicenseStatus
{
    public bool IsLicensed { get; set; }
    public bool IsValid { get; set; }
    public bool IsExpired { get; set; }
    public bool IsOffline { get; set; }
    public string? CustomerName { get; set; }
    public string? ProductName { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string> ActiveFeatures { get; set; } = new();
    public int? RemainingActivations { get; set; }
    public string? StatusMessage { get; set; }
    public string? LicenseType => DetermineLicenseType();
    
    private string DetermineLicenseType()
    {
        if (!IsLicensed) return "Unlicensed";
        if (IsExpired) return "Expired";
        if (!IsValid) return "Invalid";
        
        // Bestimme Typ basierend auf Features
        if (ActiveFeatures.Contains(LicenseFeatures.PREMIUM_SUPPORT))
            return "Premium";
        if (ActiveFeatures.Contains(LicenseFeatures.API_ACCESS))
            return "Professional";
        
        return "Standard";
    }
}
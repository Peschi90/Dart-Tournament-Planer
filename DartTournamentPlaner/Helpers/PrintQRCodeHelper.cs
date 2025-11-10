using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using QRCoder;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DrawingColor = System.Drawing.Color;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Helper-Klasse für QR-Code Generierung in der Druckansicht
/// Erstellt QR-Codes für Matches wenn Tournament beim Hub registriert ist
/// FIXED: Enthält jetzt Tournament-ID zur eindeutigen Match-Zuordnung
/// </summary>
public class PrintQRCodeHelper
{
    private readonly HubIntegrationService? _hubService;
  private readonly string? _tournamentId;

    /// <summary>
    /// Konstruktor mit optionaler Tournament-ID
    /// </summary>
    /// <param name="hubService">Hub Service für QR-Code Verfügbarkeit</param>
    /// <param name="tournamentId">Optional: Tournament-ID (kann auch später übergeben werden)</param>
    public PrintQRCodeHelper(HubIntegrationService? hubService, string? tournamentId = null)
    {
        _hubService = hubService;
        _tournamentId = tournamentId;
        System.Diagnostics.Debug.WriteLine($"[PrintQRCodeHelper] Initialized with TournamentId: {tournamentId ?? "null"}");
    }

    /// <summary>
    /// Prüft ob QR-Codes für Matches verfügbar sind
    /// </summary>
    public bool AreQRCodesAvailable => _hubService?.IsRegisteredWithHub ?? false;

    /// <summary>
    /// Generiert QR-Code BitmapImage für ein Match
 /// </summary>
    /// <param name="match">Das Match für das der QR-Code generiert werden soll</param>
  /// <param name="tournamentId">Optional: Tournament-ID (überschreibt Constructor-Wert)</param>
    /// <param name="pixelsPerModule">Pixel pro QR-Code Modul (Standard: 8)</param>
    /// <returns>BitmapImage oder null wenn nicht verfügbar</returns>
    public BitmapImage? GenerateMatchQRCode(Match match, string? tournamentId = null, int pixelsPerModule = 8)
    {
        if (!AreQRCodesAvailable || string.IsNullOrEmpty(match.UniqueId))
        {
       System.Diagnostics.Debug.WriteLine($"[PrintQRCodeHelper] Cannot generate QR for match {match.Id} - Hub not registered or no UUID");
    return null;
    }

 // Verwende übergebene Tournament-ID oder Fallback zur Constructor-ID
        var activeTournamentId = tournamentId ?? _tournamentId;
        
    if (string.IsNullOrEmpty(activeTournamentId))
        {
     System.Diagnostics.Debug.WriteLine($"[PrintQRCodeHelper] ?? WARNING: No tournament ID available for match {match.Id}");
          // Fallback zu alter Methode (nicht empfohlen, aber für Rückwärtskompatibilität)
   var fallbackUrl = $"https://dtp.i3ull3t.de:9443/dart-scoring.html?match={match.UniqueId}&uuid=true";
            return GenerateQRCodeFromUrl(fallbackUrl, pixelsPerModule);
        }

        // ? FIXED: Tournament-ID in URL eingebaut
        var dartScoringUrl = $"https://dtp.i3ull3t.de:9443/dart-scoring.html?tournament={activeTournamentId}&match={match.UniqueId}&uuid=true";
        System.Diagnostics.Debug.WriteLine($"[PrintQRCodeHelper] ? Generating QR with Tournament ID: {activeTournamentId}, Match: {match.UniqueId}");
 return GenerateQRCodeFromUrl(dartScoringUrl, pixelsPerModule);
    }

    /// <summary>
    /// Generiert QR-Code BitmapImage für ein KO-Match
    /// </summary>
    /// <param name="match">Das KO-Match für das der QR-Code generiert werden soll</param>
    /// <param name="tournamentId">Optional: Tournament-ID (überschreibt Constructor-Wert)</param>
    /// <param name="pixelsPerModule">Pixel pro QR-Code Modul (Standard: 8)</param>
    /// <returns>BitmapImage oder null wenn nicht verfügbar</returns>
    public BitmapImage? GenerateMatchQRCode(KnockoutMatch match, string? tournamentId = null, int pixelsPerModule = 8)
    {
   if (!AreQRCodesAvailable || string.IsNullOrEmpty(match.UniqueId))
 {
            System.Diagnostics.Debug.WriteLine($"[PrintQRCodeHelper] Cannot generate QR for KO match {match.Id} - Hub not registered or no UUID");
            return null;
        }

        // Verwende übergebene Tournament-ID oder Fallback zur Constructor-ID
        var activeTournamentId = tournamentId ?? _tournamentId;
     
  if (string.IsNullOrEmpty(activeTournamentId))
    {
       System.Diagnostics.Debug.WriteLine($"[PrintQRCodeHelper] ?? WARNING: No tournament ID available for KO match {match.Id}");
            // Fallback zu alter Methode (nicht empfohlen, aber für Rückwärtskompatibilität)
        var fallbackUrl = $"https://dtp.i3ull3t.de:9443/dart-scoring.html?match={match.UniqueId}&uuid=true";
  return GenerateQRCodeFromUrl(fallbackUrl, pixelsPerModule);
        }

     // ? FIXED: Tournament-ID in URL eingebaut
        var dartScoringUrl = $"https://dtp.i3ull3t.de:9443/dart-scoring.html?tournament={activeTournamentId}&match={match.UniqueId}&uuid=true";
        System.Diagnostics.Debug.WriteLine($"[PrintQRCodeHelper] ? Generating QR with Tournament ID: {activeTournamentId}, KO Match: {match.UniqueId}");
        return GenerateQRCodeFromUrl(dartScoringUrl, pixelsPerModule);
    }

    /// <summary>
 /// Generiert QR-Code aus einer URL
    /// </summary>
    private BitmapImage? GenerateQRCodeFromUrl(string url, int pixelsPerModule)
    {
   try
        {
        using (var qrGenerator = new QRCodeGenerator())
 {
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
    
          using (var qrCode = new QRCoder.QRCode(qrCodeData))
              {
         // QR Code als Bitmap generieren
  using (var qrCodeBitmap = qrCode.GetGraphic(pixelsPerModule, DrawingColor.Black, DrawingColor.White, true))
   {
 // Konvertiere zu WPF BitmapImage
          var bitmapImage = new BitmapImage();
             using (var stream = new MemoryStream())
             {
       qrCodeBitmap.Save(stream, ImageFormat.Png);
           stream.Position = 0;
          
       bitmapImage.BeginInit();
 bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
      bitmapImage.StreamSource = stream;
     bitmapImage.EndInit();
         bitmapImage.Freeze(); // Thread-Safety für Printing
    }
      
       System.Diagnostics.Debug.WriteLine($"[PrintQRCodeHelper] Generated QR code for URL: {url}");
                 return bitmapImage;
    }
                }
       }
        }
     catch (Exception ex)
     {
         System.Diagnostics.Debug.WriteLine($"[PrintQRCodeHelper] Error generating QR code: {ex.Message}");
            return null;
        }
    }
}

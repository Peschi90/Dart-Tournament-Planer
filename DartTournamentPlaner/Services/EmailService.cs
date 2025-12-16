using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Service für den automatischen E-Mail-Versand von Lizenzanfragen
/// Verwendet MailKit für bessere Kompatibilität mit Port 465 (implicit SSL)
/// </summary>
public class EmailService
{
    private const string SmtpHost = "license-dtp.i3ull3t.de";
    private const int SmtpPort = 465;
    private const string SmtpUsername = "request@license-dtp.i3ull3t.de";
    private const string SmtpPassword = "licensereq2025";
    private const string SupportEmail = "support@license-dtp.i3ull3t.de";

    /// <summary>
    /// Sendet eine Lizenzanfrage per E-Mail
    /// </summary>
    /// <param name="subject">E-Mail-Betreff</param>
    /// <param name="body">E-Mail-Inhalt</param>
    /// <param name="userEmail">E-Mail-Adresse des Benutzers (wird in CC gesetzt)</param>
    /// <returns>True wenn erfolgreich, false bei Fehler</returns>
    public async Task<EmailSendResult> SendLicenseRequestAsync(string subject, string body, string userEmail)
    {
        // Debug-Logging
        System.Diagnostics.Debug.WriteLine($"[EmailService] Starting email send attempt...");
        System.Diagnostics.Debug.WriteLine($"[EmailService] SMTP Host: {SmtpHost}:{SmtpPort}");
        System.Diagnostics.Debug.WriteLine($"[EmailService] User Email: {userEmail}");

        try
        {
            // Erstelle MimeMessage
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Dart Tournament Planner - License Request", SmtpUsername));
            message.To.Add(new MailboxAddress("Support", SupportEmail));
            
            // CC an Benutzer (falls gültige E-Mail)
            if (!string.IsNullOrWhiteSpace(userEmail) && IsValidEmail(userEmail))
            {
                try
                {
                    message.Cc.Add(new MailboxAddress(userEmail, userEmail));
                    System.Diagnostics.Debug.WriteLine($"[EmailService] CC added: {userEmail}");
                }
                catch (Exception ccEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[EmailService] CC Error: {ccEx.Message}");
                    // Ignoriere Fehler beim CC - wichtig ist nur der Versand an Support
                }
            }
            
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            System.Diagnostics.Debug.WriteLine($"[EmailService] Message prepared, connecting to SMTP...");

            // Verwende MailKit SmtpClient
            using (var client = new SmtpClient())
            {
                // Verbindung mit SSL auf Port 465 (implicit SSL)
                System.Diagnostics.Debug.WriteLine($"[EmailService] Connecting to {SmtpHost}:{SmtpPort}...");
                await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.SslOnConnect);
                System.Diagnostics.Debug.WriteLine($"[EmailService] Connected successfully");

                // Authentifizierung
                System.Diagnostics.Debug.WriteLine($"[EmailService] Authenticating as {SmtpUsername}...");
                await client.AuthenticateAsync(SmtpUsername, SmtpPassword);
                System.Diagnostics.Debug.WriteLine($"[EmailService] Authenticated successfully");

                // E-Mail senden
                System.Diagnostics.Debug.WriteLine($"[EmailService] Sending message...");
                await client.SendAsync(message);
                System.Diagnostics.Debug.WriteLine($"[EmailService] Message sent successfully");

                // Verbindung trennen
                await client.DisconnectAsync(true);
                System.Diagnostics.Debug.WriteLine($"[EmailService] Disconnected");
            }

            System.Diagnostics.Debug.WriteLine($"[EmailService] Email sent successfully!");
            
            return new EmailSendResult
            {
                Success = true,
                Message = "E-Mail erfolgreich versendet"
            };
        }
        catch (MailKit.ServiceNotConnectedException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EmailService] ServiceNotConnected Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[EmailService] InnerException: {ex.InnerException?.Message}");
            
            return new EmailSendResult
            {
                Success = false,
                Message = "Verbindung zum E-Mail-Server konnte nicht hergestellt werden. Bitte überprüfen Sie Ihre Internetverbindung.",
                Exception = ex
            };
        }
        catch (MailKit.ServiceNotAuthenticatedException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EmailService] ServiceNotAuthenticated Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[EmailService] InnerException: {ex.InnerException?.Message}");
            
            return new EmailSendResult
            {
                Success = false,
                Message = "Authentifizierung beim E-Mail-Server fehlgeschlagen.",
                Exception = ex
            };
        }
        catch (MailKit.Security.AuthenticationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EmailService] Authentication Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[EmailService] InnerException: {ex.InnerException?.Message}");
            
            return new EmailSendResult
            {
                Success = false,
                Message = "Authentifizierung fehlgeschlagen. Bitte überprüfen Sie die Zugangsdaten.",
                Exception = ex
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EmailService] General Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[EmailService] Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[EmailService] StackTrace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"[EmailService] InnerException: {ex.InnerException?.Message}");
            
            return new EmailSendResult
            {
                Success = false,
                Message = $"Fehler beim E-Mail-Versand: {ex.Message}",
                Exception = ex
            };
        }
        finally
        {
            System.Diagnostics.Debug.WriteLine($"[EmailService] Email send attempt completed");
        }
    }

    /// <summary>
    /// Validiert eine E-Mail-Adresse
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MimeKit.MailboxAddress(email, email);
            return !string.IsNullOrWhiteSpace(addr.Address);
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Ergebnis einer E-Mail-Sendung
/// </summary>
public class EmailSendResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}

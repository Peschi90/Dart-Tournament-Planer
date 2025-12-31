using System;

namespace DartTournamentPlaner.Models;

public class AuthenticatedUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Vorname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LicenseKey { get; set; }
    public bool IsAdmin { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class AuthOperationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public AuthenticatedUser? User { get; set; }

    public static AuthOperationResult Ok(AuthenticatedUser user, string? message = null) => new()
    {
        Success = true,
        Message = message,
        User = user
    };

    public static AuthOperationResult Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}

public class UserRegistrationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordRepeat { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Vorname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LicenseKey { get; set; }
}

public class UserLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberSession { get; set; }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DartTournamentPlaner.Models;
using MySqlConnector;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Stellt Login/Registrierung gegen die zentrale MySQL-Datenbank bereit.
/// Validiert Eingaben gemäß Vorgaben und erzeugt Sessions in der Tabelle user_sessions.
/// </summary>
public class UserAuthService
{
    private readonly ConfigService _configService;
    private readonly LocalizationService _localizationService;
    private readonly string _connectionString;
    private bool _schemaEnsured;

    public AuthenticatedUser? CurrentUser { get; private set; }
    public event EventHandler<AuthenticatedUser?>? CurrentUserChanged;

    public UserAuthService(ConfigService configService, LocalizationService localizationService)
    {
        _configService = configService;
        _localizationService = localizationService;
        _connectionString = BuildConnectionString();
    }

    private string BuildConnectionString()
    {
        var password = Environment.GetEnvironmentVariable("DTP_AUTH_DB_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
        {
            password = "RSfy0~9x8pd!cUve"; // Fallback aus Dokumentation
        }

        var builder = new MySqlConnectionStringBuilder
        {
            Server = "dtp.i3ull3t.de",
            Port = 3306,
            Database = "dtp_user",
            UserID = "dtp_database_user",
            Password = password,
            CharacterSet = "utf8mb4",
            Pooling = true,
            MinimumPoolSize = 0,
            MaximumPoolSize = 10,
            SslMode = MySqlSslMode.None
        };

        return builder.ConnectionString;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
    }

    private async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        if (_schemaEnsured)
        {
            return;
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Nutzer-Tabelle
        const string usersSql = @"CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(100) NOT NULL,
    vorname VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    license_key VARCHAR(100) UNIQUE,
    admin_permission BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    last_login TIMESTAMP NULL,
    is_active BOOLEAN DEFAULT TRUE,
    INDEX idx_username (username),
    INDEX idx_email (email),
    INDEX idx_license_key (license_key)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

        // Shared Access
        const string sharedAccessSql = @"CREATE TABLE IF NOT EXISTS shared_access (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    shared_license_key VARCHAR(100) NOT NULL,
    permission_level ENUM('read', 'write', 'admin') DEFAULT 'read',
    granted_by INT NOT NULL,
    granted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (granted_by) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_user_id (user_id),
    INDEX idx_shared_license_key (shared_license_key),
    UNIQUE KEY unique_user_license (user_id, shared_license_key)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

        // Sessions
        const string sessionsSql = @"CREATE TABLE IF NOT EXISTS user_sessions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    session_token VARCHAR(255) UNIQUE NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ip_address VARCHAR(45),
    user_agent TEXT,
    INDEX idx_session_token (session_token),
    INDEX idx_user_id (user_id),
    INDEX idx_expires_at (expires_at),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

        await ExecuteNonQueryAsync(connection, usersSql, cancellationToken);
        await ExecuteNonQueryAsync(connection, sharedAccessSql, cancellationToken);
        await ExecuteNonQueryAsync(connection, sessionsSql, cancellationToken);

        // Optional: Spalte für Client-App in Sessions
        const string clientAppColumnCheck = "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = 'user_sessions' AND column_name = 'client_app';";
        await using (var checkCmd = new MySqlCommand(clientAppColumnCheck, connection))
        {
            var existing = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(cancellationToken));
            if (existing == 0)
            {
                const string alterSql = "ALTER TABLE user_sessions ADD COLUMN client_app VARCHAR(100) DEFAULT 'DartTournamentPlaner';";
                try
                {
                    await ExecuteNonQueryAsync(connection, alterSql, cancellationToken);
                }
                catch (MySqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AuthService: Could not add client_app column: {ex.Message}");
                }
            }
        }

        _schemaEnsured = true;
    }

    private static async Task ExecuteNonQueryAsync(MySqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<AuthOperationResult> RegisterAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var validation = ValidateRegistration(request);
        if (validation != null)
        {
            return AuthOperationResult.Fail(validation);
        }

        await EnsureSchemaAsync(cancellationToken);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        // Duplikate prüfen
        const string duplicateSql = "SELECT username, email FROM users WHERE username = @username OR email = @email LIMIT 1;";
        await using (var dupCmd = new MySqlCommand(duplicateSql, connection, transaction))
        {
            dupCmd.Parameters.AddWithValue("@username", request.Username);
            dupCmd.Parameters.AddWithValue("@email", request.Email);

            await using var reader = await dupCmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var usernameOrdinal = reader.GetOrdinal("username");
                var emailOrdinal = reader.GetOrdinal("email");
                var duplicateUsername = reader.IsDBNull(usernameOrdinal) ? string.Empty : reader.GetString(usernameOrdinal);
                var duplicateEmail = reader.IsDBNull(emailOrdinal) ? string.Empty : reader.GetString(emailOrdinal);
                await reader.DisposeAsync();
                await transaction.RollbackAsync(cancellationToken);

                if (string.Equals(duplicateUsername, request.Username, StringComparison.OrdinalIgnoreCase))
                {
                    return AuthOperationResult.Fail(_localizationService.GetString("UsernameAlreadyExists") ?? "Username already exists.");
                }

                return AuthOperationResult.Fail(_localizationService.GetString("EmailAlreadyExists") ?? "Email already exists.");
            }
        }

        // Passwort hashen
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        // Nutzer anlegen
        const string insertUserSql = @"INSERT INTO users (username, password_hash, name, vorname, email, license_key, admin_permission, is_active)
VALUES (@username, @passwordHash, @name, @vorname, @email, @licenseKey, 0, 1);";

        await using (var insertCmd = new MySqlCommand(insertUserSql, connection, transaction))
        {
            insertCmd.Parameters.AddWithValue("@username", request.Username);
            insertCmd.Parameters.AddWithValue("@passwordHash", passwordHash);
            insertCmd.Parameters.AddWithValue("@name", request.Name);
            insertCmd.Parameters.AddWithValue("@vorname", request.Vorname);
            insertCmd.Parameters.AddWithValue("@email", request.Email);
            insertCmd.Parameters.AddWithValue("@licenseKey", string.IsNullOrWhiteSpace(request.LicenseKey) ? DBNull.Value : request.LicenseKey);

            await insertCmd.ExecuteNonQueryAsync(cancellationToken);
            var newUserId = (int)insertCmd.LastInsertedId;

            await transaction.CommitAsync(cancellationToken);

            var loginResult = await LoginInternalAsync(newUserId, request.Username, request.Name, request.Vorname, request.Email, request.LicenseKey, false, request.Username, cancellationToken);
            return loginResult.Success
                ? loginResult
                : AuthOperationResult.Ok(new AuthenticatedUser
                {
                    Id = newUserId,
                    Username = request.Username,
                    Name = request.Name,
                    Vorname = request.Vorname,
                    Email = request.Email,
                    LicenseKey = request.LicenseKey,
                    IsAdmin = false
                }, _localizationService.GetString("RegistrationSuccess") ?? "Registration successful.");
        }
    }

    public async Task<AuthOperationResult> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return AuthOperationResult.Fail(_localizationService.GetString("AuthMissingCredentials") ?? "Please enter username and password.");
        }

        await EnsureSchemaAsync(cancellationToken);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string userSql = "SELECT id, username, password_hash, name, vorname, email, license_key, admin_permission, is_active FROM users WHERE username = @username LIMIT 1;";
        await using var cmd = new MySqlCommand(userSql, connection);
        cmd.Parameters.AddWithValue("@username", request.Username);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return AuthOperationResult.Fail(_localizationService.GetString("AuthInvalidCredentials") ?? "Invalid username or password.");
        }

        var isActive = reader.GetBoolean(reader.GetOrdinal("is_active"));
        if (!isActive)
        {
            return AuthOperationResult.Fail(_localizationService.GetString("AuthInactiveUser") ?? "This account is disabled.");
        }

        var storedHash = reader.GetString(reader.GetOrdinal("password_hash"));
        if (!BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
        {
            return AuthOperationResult.Fail(_localizationService.GetString("AuthInvalidCredentials") ?? "Invalid username or password.");
        }

        var userId = reader.GetInt32(reader.GetOrdinal("id"));
        var name = reader.GetString(reader.GetOrdinal("name"));
        var vorname = reader.GetString(reader.GetOrdinal("vorname"));
        var email = reader.GetString(reader.GetOrdinal("email"));
        var licenseKeyOrdinal = reader.GetOrdinal("license_key");
        string? licenseKey = reader.IsDBNull(licenseKeyOrdinal) ? null : reader.GetString(licenseKeyOrdinal);
        var isAdmin = reader.GetBoolean(reader.GetOrdinal("admin_permission"));

        var loginResult = await LoginInternalAsync(userId, request.Username, name, vorname, email, licenseKey, isAdmin, request.Username, cancellationToken, request.RememberSession);
        return loginResult;
    }

    private async Task<AuthOperationResult> LoginInternalAsync(
        int userId,
        string username,
        string name,
        string vorname,
        string email,
        string? licenseKey,
        bool isAdmin,
        string auditUsername,
        CancellationToken cancellationToken,
        bool rememberSession = false)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sessionToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        const string insertSessionSql = @"INSERT INTO user_sessions (user_id, session_token, expires_at, ip_address, user_agent, client_app)
VALUES (@userId, @sessionToken, @expiresAt, @ipAddress, @userAgent, @clientApp);";

        await using (var sessionCmd = new MySqlCommand(insertSessionSql, connection))
        {
            sessionCmd.Parameters.AddWithValue("@userId", userId);
            sessionCmd.Parameters.AddWithValue("@sessionToken", sessionToken);
            sessionCmd.Parameters.AddWithValue("@expiresAt", expiresAt);
            sessionCmd.Parameters.AddWithValue("@ipAddress", GetLocalIpAddress());
            sessionCmd.Parameters.AddWithValue("@userAgent", BuildUserAgent());
            sessionCmd.Parameters.AddWithValue("@clientApp", "DartTournamentPlaner");

            await sessionCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        // Last Login aktualisieren
        const string updateLastLoginSql = "UPDATE users SET last_login = CURRENT_TIMESTAMP WHERE id = @userId;";
        await using (var updateCmd = new MySqlCommand(updateLastLoginSql, connection))
        {
            updateCmd.Parameters.AddWithValue("@userId", userId);
            await updateCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        var authenticatedUser = new AuthenticatedUser
        {
            Id = userId,
            Username = username,
            Name = name,
            Vorname = vorname,
            Email = email,
            LicenseKey = licenseKey,
            IsAdmin = isAdmin,
            SessionToken = sessionToken,
            ExpiresAt = expiresAt
        };

        CurrentUser = authenticatedUser;
        CurrentUserChanged?.Invoke(this, CurrentUser);

        // Optional Session merken
        _configService.Config.AuthUsername = auditUsername;
        _configService.Config.RememberAuthSession = rememberSession;
        _configService.Config.AuthSessionToken = rememberSession ? sessionToken : null;
        await _configService.SaveConfigAsync();

        return AuthOperationResult.Ok(authenticatedUser, _localizationService.GetString("LoginSuccess") ?? "Login successful.");
    }

    public async Task<bool> TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var token = _configService.Config.AuthSessionToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        await EnsureSchemaAsync(cancellationToken);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sessionSql = @"SELECT us.user_id, us.expires_at, u.username, u.name, u.vorname, u.email, u.license_key, u.admin_permission, u.is_active
FROM user_sessions us
JOIN users u ON us.user_id = u.id
WHERE us.session_token = @token AND us.expires_at > UTC_TIMESTAMP() LIMIT 1;";

        await using var cmd = new MySqlCommand(sessionSql, connection);
        cmd.Parameters.AddWithValue("@token", token);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            await LogoutAsync(null, cancellationToken);
            return false;
        }

        if (!reader.GetBoolean(reader.GetOrdinal("is_active")))
        {
            await reader.DisposeAsync();
            await LogoutAsync(token, cancellationToken);
            return false;
        }

        var userId = reader.GetInt32(reader.GetOrdinal("user_id"));
        var expiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at"));
        var username = reader.GetString(reader.GetOrdinal("username"));
        var name = reader.GetString(reader.GetOrdinal("name"));
        var vorname = reader.GetString(reader.GetOrdinal("vorname"));
        var email = reader.GetString(reader.GetOrdinal("email"));
        var licenseKeyOrdinal = reader.GetOrdinal("license_key");
        string? licenseKey = reader.IsDBNull(licenseKeyOrdinal) ? null : reader.GetString(licenseKeyOrdinal);
        var isAdmin = reader.GetBoolean(reader.GetOrdinal("admin_permission"));

        CurrentUser = new AuthenticatedUser
        {
            Id = userId,
            Username = username,
            Name = name,
            Vorname = vorname,
            Email = email,
            LicenseKey = licenseKey,
            IsAdmin = isAdmin,
            SessionToken = token,
            ExpiresAt = expiresAt
        };

        CurrentUserChanged?.Invoke(this, CurrentUser);
        return true;
    }

    public async Task LogoutAsync(string? sessionToken = null, CancellationToken cancellationToken = default)
    {
        var token = sessionToken ?? CurrentUser?.SessionToken ?? _configService.Config.AuthSessionToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            await EnsureSchemaAsync(cancellationToken);
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string deleteSql = "DELETE FROM user_sessions WHERE session_token = @token;";
            await using var cmd = new MySqlCommand(deleteSql, connection);
            cmd.Parameters.AddWithValue("@token", token);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        CurrentUser = null;
        CurrentUserChanged?.Invoke(this, CurrentUser);

        _configService.Config.AuthSessionToken = null;
        await _configService.SaveConfigAsync();
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch
        {
            // ignore
        }

        return "127.0.0.1";
    }

    private string BuildUserAgent()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        var versionString = version != null ? version.ToString() : "1.0.0";
        return $"DartTournamentPlaner/{versionString} (.NET {Environment.Version}; {Environment.OSVersion})";
    }

    private string? ValidateRegistration(UserRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Email))
        {
            return _localizationService.GetString("AuthMissingFields") ?? "Please fill in all required fields.";
        }

        if (request.Password.Length < 8)
        {
            return _localizationService.GetString("AuthPasswordTooShort") ?? "Password must be at least 8 characters.";
        }

        if (!string.Equals(request.Password, request.PasswordRepeat, StringComparison.Ordinal))
        {
            return _localizationService.GetString("AuthPasswordsDontMatch") ?? "Passwords do not match.";
        }

        if (!Regex.IsMatch(request.Username, "^[a-zA-Z0-9_]{3,50}$"))
        {
            return _localizationService.GetString("AuthInvalidUsername") ?? "Username must be 3-50 characters (letters, digits, underscore).";
        }

        if (!Regex.IsMatch(request.Email, "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$"))
        {
            return _localizationService.GetString("AuthInvalidEmail") ?? "Invalid email address.";
        }

        return null;
    }

    public async Task<AuthOperationResult> UpdateProfileAsync(string name, string vorname, string email, string? licenseKey, CancellationToken cancellationToken = default)
    {
        if (CurrentUser is null)
        {
            return AuthOperationResult.Fail(_localizationService.GetString("AuthNotLoggedIn") ?? "Not logged in.");
        }

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(vorname) || string.IsNullOrWhiteSpace(email))
        {
            return AuthOperationResult.Fail(_localizationService.GetString("AuthMissingFields") ?? "Please fill in all required fields.");
        }

        if (!Regex.IsMatch(email, "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$"))
        {
            return AuthOperationResult.Fail(_localizationService.GetString("AuthInvalidEmail") ?? "Invalid email address.");
        }

        await EnsureSchemaAsync(cancellationToken);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string updateSql = @"UPDATE users SET name=@name, vorname=@vorname, email=@email, license_key=@licenseKey, updated_at=CURRENT_TIMESTAMP WHERE id=@id";
        await using (var cmd = new MySqlCommand(updateSql, connection))
        {
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@vorname", vorname);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@licenseKey", string.IsNullOrWhiteSpace(licenseKey) ? DBNull.Value : licenseKey);
            cmd.Parameters.AddWithValue("@id", CurrentUser.Id);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        CurrentUser.Name = name;
        CurrentUser.Vorname = vorname;
        CurrentUser.Email = email;
        CurrentUser.LicenseKey = string.IsNullOrWhiteSpace(licenseKey) ? null : licenseKey;
        CurrentUserChanged?.Invoke(this, CurrentUser);

        return AuthOperationResult.Ok(CurrentUser, _localizationService.GetString("ProfileUpdated") ?? "Profile updated.");
    }
}

using Microsoft.EntityFrameworkCore;

namespace DartTournamentPlaner.API.Services;

/// <summary>
/// Entity Framework DbContext für die API
/// Speichert temporäre API-Daten im Memory
/// </summary>
public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

    public DbSet<ApiTournamentData> Tournaments { get; set; }
    public DbSet<ApiSession> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiTournamentData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.JsonData).IsRequired();
        });

        modelBuilder.Entity<ApiSession>(entity =>
        {
            entity.HasKey(e => e.SessionId);
            entity.Property(e => e.ConnectionId).HasMaxLength(100);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
        });
    }
}

/// <summary>
/// Entität für Turnier-Daten in der API-Datenbank
/// </summary>
public class ApiTournamentData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JsonData { get; set; } = string.Empty; // Serialized tournament data
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Entität für aktive API-Sessions
/// </summary>
public class ApiSession
{
    public string SessionId { get; set; } = string.Empty;
    public string? ConnectionId { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public string? UserAgent { get; set; }
    public bool IsActive { get; set; } = true;
}
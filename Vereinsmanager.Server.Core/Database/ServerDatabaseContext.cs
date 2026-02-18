#nullable enable
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Database;

public class ServerDatabaseContext : DbContext
{
    private readonly UserContext _userContext;
    public ServerDatabaseContext(DbContextOptions<ServerDatabaseContext> options, UserContext userContext) : base(options)
    {
        _userContext = userContext;
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    // ScoreManagement
    public DbSet<AlternateVoice>  AlternateVoices { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventScore> EventScores { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<Instrument>  Instruments { get; set; }
    public DbSet<MusicFolder> MusicFolders { get; set; }
    public DbSet<MusicSheed> MusicSheeds { get; set; }
    public DbSet<ScoreMusicFolder> ScoreMusicFolders { get; set; }
    public DbSet<Voice> Voices { get; set; }
    public override int SaveChanges()
    {
        ApplyMetaData();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyMetaData();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyMetaData()
    {
        var entries = ChangeTracker.Entries<MetaData>();
        var userName = _userContext.UserName ?? "unknown";
        
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;

                // Hier kannst du den aktuellen Benutzer setzen
                entry.Entity.CreatedBy = userName;
                entry.Entity.UpdatedBy = userName;
            }
            else if (entry.State == EntityState.Modified) //todo far: only update if specific fields are changed, not if forin key is changed
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = userName;
            }
        }
    }
}
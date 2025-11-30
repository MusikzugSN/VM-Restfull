#nullable enable
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database.Base;
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
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = userName;
            }
        }
    }
}
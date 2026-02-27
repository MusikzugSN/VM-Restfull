using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Vereinsmanager.Database;
// DB Context for Migrations
public class ServerDatabaseContextFactory : IDesignTimeDbContextFactory<ServerDatabaseContext>
{
    public ServerDatabaseContext CreateDbContext(string[] args)
    {
        // Load configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ServerDatabaseContext>();
        
        var mysqlConnectionString = config.GetConnectionString("MySqlConnection");

        if (mysqlConnectionString == null)
        {
            throw new InvalidOperationException("MySqlConnection string is not configured.");
        }
        
        optionsBuilder.UseMySql(mysqlConnectionString, ServerVersion.AutoDetect(mysqlConnectionString));

        // For design-time, we don't have a real UserContext
        var fakeUserContext = new UserContext(
            new HttpContextAccessor(),
            new Lazy<ServerDatabaseContext>(() => null!),
            config
        );

        return new ServerDatabaseContext(optionsBuilder.Options, fakeUserContext);
    }
}
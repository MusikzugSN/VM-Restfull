using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Vereinsmanager.Autofac;

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

        var connectionData = config.GetSection("Database").Get<DatabaseContext>();
        
        var optionsBuilder = new DbContextOptionsBuilder<ServerDatabaseContext>();
        var connectionString =
            $"Server={connectionData?.Server ?? "localhost"}; Port={connectionData?.Port ?? "3306"}; Database={connectionData?.Database ?? "notes"}; Uid={connectionData?.User ?? "vmanager"}; Pwd={connectionData?.Password ?? ""}";
        

        if (connectionString == null)
        {
            throw new InvalidOperationException("MySqlConnection string is not configured.");
        }
        
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        // For design-time, we don't have a real UserContext
        var fakeUserContext = new UserContext(
            new HttpContextAccessor(),
            new Lazy<ServerDatabaseContext>(() => null!),
            config
        );

        return new ServerDatabaseContext(optionsBuilder.Options, fakeUserContext);
    }
}
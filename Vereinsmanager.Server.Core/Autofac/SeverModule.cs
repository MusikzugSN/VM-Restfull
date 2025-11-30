#nullable enable
using Autofac;
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Services;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Autofac;

public class SeverModule : Module
{
    private readonly IConfiguration _configuration;
    
    public SeverModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    protected override void Load(ContainerBuilder builder)
    {
        RegisterDbContext(builder);
        
        builder.RegisterType<JwtTokenService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<UserContext>().AsSelf().InstancePerLifetimeScope();
        
        builder.RegisterType<UserService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<GroupService>().AsSelf().InstancePerLifetimeScope();
        
    }

    private void RegisterDbContext(ContainerBuilder builder)
    {
        var mysqlConnectionString = _configuration.GetConnectionString("MySqlConnection");
        builder.Register(container =>
        {
            var options = new DbContextOptionsBuilder<ServerDatabaseContext>();
            
            if (!string.IsNullOrEmpty(mysqlConnectionString))
            {
                options.UseMySql(mysqlConnectionString, ServerVersion.AutoDetect(mysqlConnectionString));
            }
            else
            {
                options.UseInMemoryDatabase("Vereinsmanager.Server.InMemoryDb");
            }
            
            var userContext = container.Resolve<UserContext>();
            return new ServerDatabaseContext(options.Options, userContext);
        }).AsSelf().InstancePerLifetimeScope();
    }
}
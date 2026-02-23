#nullable enable
using Autofac;
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services;
using Vereinsmanager.Services.ScoreManagement;
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
        builder.RegisterType<RoleService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<PermissionService>().AsSelf().InstancePerLifetimeScope();
        
        //ScoreManagement
        builder.RegisterType<ScoreService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<MusicSheetService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<MusicFolderService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<VoiceService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<InstrumentService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<EventService>().AsSelf().InstancePerLifetimeScope();
        
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
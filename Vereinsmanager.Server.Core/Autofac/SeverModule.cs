#nullable enable
using Autofac;
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Services;
using Vereinsmanager.Services.Base;
using Vereinsmanager.Services.ScoreManagement;

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
        builder.RegisterType<CustomTokenService>().AsSelf().SingleInstance();
        
        builder.RegisterType<UserService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<GroupService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<RoleService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<PermissionService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<ConfigurationService>().AsSelf().InstancePerLifetimeScope();
        
        //ScoreManagement
        builder.RegisterType<ScoreService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<MusicSheetService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<MusicFolderService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<VoiceService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<InstrumentService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<EventsService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<TagService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<PrintSettingsService>().AsSelf().InstancePerLifetimeScope();
    }

    private void RegisterDbContext(ContainerBuilder builder)
    {
        var connectionData = _configuration.GetSection("Database").Get<DatabaseContext>();
        builder.Register(container =>
        {
            var options = new DbContextOptionsBuilder<ServerDatabaseContext>();

            var type = connectionData?.Provider ?? "MySql";

            if (type == "MySql")
            {
                var connectionString =
                    $"Server={connectionData?.Server ?? "localhost"}; Port={connectionData?.Port ?? "3306"}; Database={connectionData?.Database ?? "notes"}; Uid={connectionData?.User ?? "vmanager"}; Pwd={connectionData?.Password ?? ""}";
                
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
            
            var userContext = container.Resolve<UserContext>();
            return new ServerDatabaseContext(options.Options, userContext);
        }).AsSelf().InstancePerLifetimeScope();
    }
}

public record DatabaseContext(string? Provider, string? Server, string? Port, string? Database, string? User, string? Password);
#nullable enable
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services;

public record CreateConfiguration(ConfigType Type, string Value);
public record UpdateConfiguration(string? Value);


public class ConfigurationService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public ConfigurationService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    private IQueryable<Configuration> GetConfigurations()
    {
        return _dbContext.Configurations;
    }

    public Configuration[] ListConfigurations()
    {
        return GetConfigurations().ToArray();
    }

    public Configuration? GetConfiguration(ConfigType type)
    {
        return GetConfigurations().FirstOrDefault(c => c.Type == type);
    }

    public Configuration CreateOrUpdateConfiguration(CreateConfiguration dto)
    {
        var config = _dbContext.Configurations
            .FirstOrDefault(c => c.Type == dto.Type);

        if (config == null)
        {
            // Create
            config = new Configuration
            {
                Type = dto.Type,
                Value = dto.Value
            };

            _dbContext.Configurations.Add(config);
        }
        else
        {
            // Update
            config.Value = dto.Value;
        }

        _dbContext.SaveChanges();
        return config;
    }
    
    public bool DeleteConfiguration(ConfigType type)
    {
        var config = _dbContext.Configurations.FirstOrDefault(c => c.Type == type);
        if (config == null)
            return false;

        _dbContext.Configurations.Remove(config);
        _dbContext.SaveChanges();
        return true;
    }
}
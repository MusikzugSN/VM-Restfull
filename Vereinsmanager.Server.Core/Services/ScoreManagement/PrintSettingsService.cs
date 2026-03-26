using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreatePrintSettings(
    int PageCount,
    PrintMode Mode,
    DuplexMode Duplex,
    int FileFormat);

public record UpdatePrintSettings(
    int? PageCount,
    PrintMode? Mode,
    DuplexMode? Duplex,
    int? FileFormat);

public class PrintSettingsService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public PrintSettingsService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    public ReturnValue<PrintSettings[]> ListPrintSettings()
    {
        throw new NotImplementedException();
        //if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListPrintSettings))
            return ErrorUtils.NotPermitted(nameof(PrintSettings), "read all");

        return _dbContext.PrintSettings.ToArray();
    }

    public ReturnValue<PrintSettings> GetPrintSettingsById(int printConfigId)
    {
        throw new NotImplementedException();
        //if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListPrintSettings))
            return ErrorUtils.NotPermitted(nameof(PrintSettings), printConfigId.ToString());

        var loaded = _dbContext.PrintSettings
            .FirstOrDefault(x => x.PrintConfigId == printConfigId);

        if (loaded == null)
            return ErrorUtils.ValueNotFound(nameof(PrintSettings), printConfigId.ToString());

        return loaded;
    }

    public ReturnValue<PrintSettings> CreatePrintSettings(CreatePrintSettings createPrintSettings)
    {
        throw new NotImplementedException();
        //if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreatePrintSettings))
            return ErrorUtils.NotPermitted(nameof(PrintSettings), createPrintSettings.PageCount.ToString());

        var duplicate = _dbContext.PrintSettings.Any(x =>
            x.PageCount == createPrintSettings.PageCount &&
            x.Mode == createPrintSettings.Mode &&
            x.Duplex == createPrintSettings.Duplex &&
            x.FileFormat == createPrintSettings.FileFormat);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(PrintSettings),
                $"PageCount={createPrintSettings.PageCount}, Mode={createPrintSettings.Mode}, Duplex={createPrintSettings.Duplex}, FileFormat={createPrintSettings.FileFormat}");

        var toCreate = new PrintSettings
        {
            PageCount = createPrintSettings.PageCount,
            Mode = createPrintSettings.Mode,
            Duplex = createPrintSettings.Duplex,
            FileFormat = createPrintSettings.FileFormat
        };

        _dbContext.PrintSettings.Add(toCreate);
        _dbContext.SaveChanges();
        return toCreate;
    }

    public ReturnValue<PrintSettings> UpdatePrintSettings(int printConfigId, UpdatePrintSettings updatePrintSettings)
    {
        throw new NotImplementedException();
        //if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdatePrintSettings))
            return ErrorUtils.NotPermitted(nameof(PrintSettings), printConfigId.ToString());

        var loaded = _dbContext.PrintSettings
            .FirstOrDefault(x => x.PrintConfigId == printConfigId);

        if (loaded == null)
            return ErrorUtils.ValueNotFound(nameof(PrintSettings), printConfigId.ToString());

        var newPageCount = updatePrintSettings.PageCount ?? loaded.PageCount;
        var newMode = updatePrintSettings.Mode ?? loaded.Mode;
        var newDuplex = updatePrintSettings.Duplex ?? loaded.Duplex;
        var newFileFormat = updatePrintSettings.FileFormat ?? loaded.FileFormat;


        var duplicate = _dbContext.PrintSettings.Any(x =>
            x.PrintConfigId != printConfigId &&
            x.PageCount == newPageCount &&
            x.Mode == newMode &&
            x.Duplex == newDuplex &&
            x.FileFormat == newFileFormat);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(PrintSettings),
                $"PageCount={newPageCount}, Mode={newMode}, Duplex={newDuplex}, FileFormat={newFileFormat}");

        loaded.PageCount = newPageCount;
        loaded.Mode = newMode;
        loaded.Duplex = newDuplex;
        loaded.FileFormat = newFileFormat;

        _dbContext.SaveChanges();
        return loaded;
    }

    public ReturnValue<bool> DeletePrintSettings(int printConfigId)
    {
        throw new NotImplementedException();
        //if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeletePrintSettings))
            return ErrorUtils.NotPermitted(nameof(PrintSettings), printConfigId.ToString());

        var loaded = _dbContext.PrintSettings
            .FirstOrDefault(x => x.PrintConfigId == printConfigId);

        if (loaded == null)
            return ErrorUtils.ValueNotFound(nameof(PrintSettings), printConfigId.ToString());

        _dbContext.PrintSettings.Remove(loaded);
        _dbContext.SaveChanges();
        return true;
    }
}
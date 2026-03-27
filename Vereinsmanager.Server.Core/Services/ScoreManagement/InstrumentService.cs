using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Base;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateInstrument(string Name, string Type);
public record UpdateInstrument(string? Name, string? Type);

public class InstrumentService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public InstrumentService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    private IQueryable<Instrument> GetInstruments(bool includeVoices)
    {
        IQueryable<Instrument> q = _dbContext.Instruments;
        if (includeVoices)
            q = q.Include(i => i.Voices);
        return q;
    }

    public Instrument? LoadInstrumentById(int instrumentId, bool includeVoices = false)
    {
        return GetInstruments(includeVoices)
            .FirstOrDefault(i => i.InstrumentId == instrumentId);
    }

    public ReturnValue<Instrument[]> ListInstruments(bool includeVoices = false)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListVoice))
            return ErrorUtils.NotPermitted(nameof(Instrument), "read all");

        return GetInstruments(includeVoices).ToArray();
    }

    public ReturnValue<Instrument> GetInstrumentById(int instrumentId, bool includeVoices = true)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListVoice))
            return ErrorUtils.NotPermitted(nameof(Instrument), instrumentId.ToString());

        var instrument = GetInstruments(includeVoices)
            .FirstOrDefault(i => i.InstrumentId == instrumentId);

        if (instrument == null)
            return ErrorUtils.ValueNotFound(nameof(Instrument), instrumentId.ToString());

        return instrument;
    }

    public ReturnValue<Instrument> CreateInstrument(CreateInstrument dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateVoice))
            return ErrorUtils.NotPermitted(nameof(Instrument), dto.Name);

        var duplicate = _dbContext.Instruments.Any(i => i.Name == dto.Name);
        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(Instrument), dto.Name);

        var instrument = new Instrument
        {
            Name = dto.Name,
            Type = dto.Type
        };

        _dbContext.Instruments.Add(instrument);
        _dbContext.SaveChanges();
        return instrument;
    }

    public ReturnValue<Instrument> UpdateInstrument(int instrumentId, UpdateInstrument dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateVoice))
            return ErrorUtils.NotPermitted(nameof(Instrument), instrumentId.ToString());

        var instrument = _dbContext.Instruments.FirstOrDefault(i => i.InstrumentId == instrumentId);
        if (instrument == null)
            return ErrorUtils.ValueNotFound(nameof(Instrument), instrumentId.ToString());

        var newName = instrument.Name;
        var newType = instrument.Type;

        if (dto.Name is not null)
            newName = dto.Name;

        if (dto.Type is not null)
            newType = dto.Type;

        var wouldDuplicate = _dbContext.Instruments.Any(i =>
            i.InstrumentId != instrumentId &&
            i.Name == newName);

        if (wouldDuplicate)
            return ErrorUtils.AlreadyExists(nameof(Instrument), newName);

        instrument.Name = newName;
        instrument.Type = newType;

        _dbContext.SaveChanges();
        return instrument;
    }

    public ReturnValue<bool> DeleteInstrument(int instrumentId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteVoice))
            return ErrorUtils.NotPermitted(nameof(Instrument), instrumentId.ToString());

        var instrument = _dbContext.Instruments
            .FirstOrDefault(i => i.InstrumentId == instrumentId);

        if (instrument == null)
            return ErrorUtils.ValueNotFound(nameof(Instrument), instrumentId.ToString());

        var hasVoices = _dbContext.Voices.Any(v => v.InstrumentId == instrumentId);
        if (hasVoices)
            return ErrorUtils.NotPermitted(nameof(Instrument), "delete (has Voices)");

        _dbContext.Instruments.Remove(instrument);
        _dbContext.SaveChanges();
        return true;
    }
}
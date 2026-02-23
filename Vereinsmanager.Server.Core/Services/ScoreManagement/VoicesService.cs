#nullable enable
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateVoice(string Name, int InstrumentId);
public record UpdateVoice(string? Name, int? InstrumentId);

public record CreateAlternateVoice(string Alternative, int Priority);
public record UpdateAlternateVoice(string? Alternative, int? Priority);

public class VoiceService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public VoiceService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    public Voice? LoadVoiceByName(string name)
    {
        return _dbContext.Voices.FirstOrDefault(v => v.Name == name);
    }

    public Voice? LoadVoiceById(int voiceId, bool includeInstrument = false, bool includeAlternateVoices = false)
    {
        IQueryable<Voice> q = _dbContext.Voices;

        if (includeInstrument) q = q.Include(v => v.Instrument);
        if (includeAlternateVoices) q = q.Include(v => v.AlternateVoices);

        return q.FirstOrDefault(v => v.VoiceId == voiceId);
    }

    public ReturnValue<Voice[]> ListVoices(bool includeInstrument = true, bool includeAlternateVoices = true)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), "read all");

        IQueryable<Voice> q = _dbContext.Voices;

        if (includeInstrument) q = q.Include(v => v.Instrument);
        if (includeAlternateVoices) q = q.Include(v => v.AlternateVoices);

        return q.ToArray();
    }

    public ReturnValue<Voice> CreateVoice(CreateVoice createVoice)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), createVoice.Name);

        var instrumentExists = _dbContext.Instruments.Any(i => i.InstrumentId == createVoice.InstrumentId);
        if (!instrumentExists)
            return ErrorUtils.ValueNotFound(nameof(Instrument), createVoice.InstrumentId.ToString());

        var duplicate = _dbContext.Voices.Any(v =>
            v.Name == createVoice.Name && v.InstrumentId == createVoice.InstrumentId);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(Voice), $"{createVoice.Name} (InstrumentId={createVoice.InstrumentId})");
        
        var instrument = _dbContext.Instruments
            .FirstOrDefault(i => i.InstrumentId == createVoice.InstrumentId);
        
        if (instrument == null)
            return ErrorUtils.ValueNotFound(nameof(Instrument), createVoice.InstrumentId.ToString());
        
        var newVoice = new Voice
        {
            Name = createVoice.Name,
            InstrumentId = createVoice.InstrumentId,
            Instrument = instrument
        };

        _dbContext.Voices.Add(newVoice);
        _dbContext.SaveChanges();
        return newVoice;
    }

    public ReturnValue<Voice> UpdateVoice(int voiceId, UpdateVoice updateVoice)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), voiceId.ToString());

        var voice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == voiceId);
        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), voiceId.ToString());

        var newName = updateVoice.Name ?? voice.Name;
        var newInstrumentId = updateVoice.InstrumentId ?? voice.InstrumentId;
        
        var instrumentExists = _dbContext.Instruments.Any(i => i.InstrumentId == newInstrumentId);
        if (!instrumentExists)
            return ErrorUtils.ValueNotFound(nameof(Instrument), newInstrumentId.ToString());
        
        var wouldDuplicate = _dbContext.Voices.Any(v =>
            v.VoiceId != voiceId &&
            v.Name == newName &&
            v.InstrumentId == newInstrumentId);

        if (wouldDuplicate)
            return ErrorUtils.AlreadyExists(nameof(Voice), $"{newName} (InstrumentId={newInstrumentId})");

        voice.Name = newName;
        voice.InstrumentId = newInstrumentId;

        _dbContext.SaveChanges();
        return voice;
    }

    public ReturnValue<bool> DeleteVoice(int voiceId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), voiceId.ToString());

        var voice = _dbContext.Voices
            .Include(v => v.AlternateVoices)
            .FirstOrDefault(v => v.VoiceId == voiceId);

        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), voiceId.ToString());

        var hasSheets = _dbContext.MusicSheets.Any(ms => ms.VoiceId == voiceId);
        if (hasSheets)
            return ErrorUtils.NotPermitted(nameof(Voice), "delete (has MusicSheets)");

        if (voice.AlternateVoices.Count > 0)
            _dbContext.AlternateVoices.RemoveRange(voice.AlternateVoices);

        _dbContext.Voices.Remove(voice);
        _dbContext.SaveChanges();
        return true;
    }
    

    public ReturnValue<AlternateVoice[]> ListAlternateVoices(int voiceId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListVoice))
            return ErrorUtils.NotPermitted(nameof(AlternateVoice), "read all for voice");

        var voiceExists = _dbContext.Voices.Any(v => v.VoiceId == voiceId);
        if (!voiceExists)
            return ErrorUtils.ValueNotFound(nameof(Voice), voiceId.ToString());

        return _dbContext.AlternateVoices
            .Where(av => av.VoiceId == voiceId)
            .OrderBy(av => av.Priority)
            .ToArray();
    }

    public AlternateVoice? LoadAlternateVoiceById(int voiceId, int alternateVoiceId)
    {
        return _dbContext.AlternateVoices
            .FirstOrDefault(av => av.VoiceId == voiceId && av.AlternateVoiceId == alternateVoiceId);
    }

    public ReturnValue<AlternateVoice> AddAlternateVoice(int voiceId, CreateAlternateVoice dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateAlternateVoice))
            return ErrorUtils.NotPermitted(nameof(AlternateVoice), voiceId.ToString());

        var voice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == voiceId);
        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), voiceId.ToString());

        var duplicate = _dbContext.AlternateVoices.Any(av =>
            av.VoiceId == voiceId &&
            av.Priority == dto.Priority &&
            av.Alternative == dto.Alternative);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(AlternateVoice), $"{dto.Alternative} (Priority={dto.Priority})");

        var alt = new AlternateVoice
        {
            VoiceId = voiceId,
            Voice = voice,
            Alternative = dto.Alternative,
            Priority = dto.Priority
        };

        _dbContext.AlternateVoices.Add(alt);
        _dbContext.SaveChanges();
        return alt;
    }

    public ReturnValue<AlternateVoice> UpdateAlternateVoice(int voiceId, int alternateVoiceId, UpdateAlternateVoice dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateAlternateVoice))
            return ErrorUtils.NotPermitted(nameof(AlternateVoice), alternateVoiceId.ToString());

        var alt = LoadAlternateVoiceById(voiceId, alternateVoiceId);
        if (alt == null)
            return ErrorUtils.ValueNotFound(nameof(AlternateVoice), alternateVoiceId.ToString());

        var newAlternative = dto.Alternative ?? alt.Alternative;
        var newPriority = dto.Priority ?? alt.Priority;

        var duplicate = _dbContext.AlternateVoices.Any(av =>
            av.AlternateVoiceId != alternateVoiceId &&
            av.VoiceId == voiceId &&
            av.Priority == newPriority &&
            av.Alternative == newAlternative);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(AlternateVoice), $"{newAlternative} (Priority={newPriority})");

        alt.Alternative = newAlternative;
        alt.Priority = newPriority;

        _dbContext.SaveChanges();
        return alt;
    }

    public ReturnValue<bool> DeleteAlternateVoice(int voiceId, int alternateVoiceId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteAlternateVoice))
            return ErrorUtils.NotPermitted(nameof(AlternateVoice), alternateVoiceId.ToString());

        var alt = LoadAlternateVoiceById(voiceId, alternateVoiceId);
        if (alt == null)
            return ErrorUtils.ValueNotFound(nameof(AlternateVoice), alternateVoiceId.ToString());

        _dbContext.AlternateVoices.Remove(alt);
        _dbContext.SaveChanges();
        return true;
    }
}

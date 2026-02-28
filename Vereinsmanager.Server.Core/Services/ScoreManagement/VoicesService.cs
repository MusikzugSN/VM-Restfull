#nullable enable
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateVoice(string Name, int InstrumentId);
public record UpdateVoice(string? Name, int? InstrumentId);

public record CreateAlternateVoice(int Alternative, int Priority);
public record UpdateAlternateVoice(int? Alternative, int? Priority);

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

    public ReturnValue<Voice> CreateVoice(CreateVoice dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), dto.Name);

        var instrumentExists = _dbContext.Instruments.Any(i => i.InstrumentId == dto.InstrumentId);
        if (!instrumentExists)
            return ErrorUtils.ValueNotFound(nameof(Instrument), dto.InstrumentId.ToString());

        var duplicate = _dbContext.Voices.Any(v =>
            v.Name == dto.Name && v.InstrumentId == dto.InstrumentId);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(Voice), $"{dto.Name} (InstrumentId={dto.InstrumentId})");

        var instrument = _dbContext.Instruments.FirstOrDefault(i => i.InstrumentId == dto.InstrumentId);
        if (instrument == null)
            return ErrorUtils.ValueNotFound(nameof(Instrument), dto.InstrumentId.ToString());

        var newVoice = new Voice
        {
            Name = dto.Name,
            InstrumentId = dto.InstrumentId,
            Instrument = instrument
        };

        _dbContext.Voices.Add(newVoice);
        _dbContext.SaveChanges();
        return newVoice;
    }

    public ReturnValue<Voice> UpdateVoice(int voiceId, UpdateVoice dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), voiceId.ToString());

        var voice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == voiceId);
        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), voiceId.ToString());

        var newName = dto.Name ?? voice.Name;
        var newInstrumentId = dto.InstrumentId ?? voice.InstrumentId;

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

        if (dto.Alternative <= 0)
            return ErrorUtils.NotPermitted(nameof(AlternateVoice), "alternative must be a valid VoiceId");

        if (dto.Alternative == voiceId)
            return ErrorUtils.NotPermitted(nameof(AlternateVoice), "alternative cannot equal voiceId");

        var alternativeVoice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == dto.Alternative);
        if (alternativeVoice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), dto.Alternative.ToString());

        var duplicateAlt = _dbContext.AlternateVoices.Any(av =>
            av.VoiceId == voiceId &&
            av.Alternative == dto.Alternative);

        if (duplicateAlt)
            return ErrorUtils.AlreadyExists(nameof(AlternateVoice),
                $"VoiceId={voiceId} Alternative={dto.Alternative}");

        var duplicatePriority = _dbContext.AlternateVoices.Any(av =>
            av.VoiceId == voiceId &&
            av.Priority == dto.Priority);

        if (duplicatePriority)
            return ErrorUtils.AlreadyExists(nameof(AlternateVoice),
                $"VoiceId={voiceId} Priority={dto.Priority}");

        var alt = new AlternateVoice
        {
            VoiceId = voiceId,
            Voice = voice,
            Alternative = dto.Alternative,
            AlternativeVoiceNav = alternativeVoice,
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

        if (newAlternative <= 0)
            return ErrorUtils.NotPermitted(nameof(AlternateVoice), "alternative must be a valid VoiceId");

        if (newAlternative == voiceId)
            return ErrorUtils.NotPermitted(nameof(AlternateVoice), "alternative cannot equal voiceId");

        var newAlternativeVoice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == newAlternative);
        if (newAlternativeVoice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), newAlternative.ToString());

        var duplicateAlt = _dbContext.AlternateVoices.Any(av =>
            av.AlternateVoiceId != alternateVoiceId &&
            av.VoiceId == voiceId &&
            av.Alternative == newAlternative);

        if (duplicateAlt)
            return ErrorUtils.AlreadyExists(nameof(AlternateVoice),
                $"VoiceId={voiceId} Alternative={newAlternative}");

        var duplicatePriority = _dbContext.AlternateVoices.Any(av =>
            av.AlternateVoiceId != alternateVoiceId &&
            av.VoiceId == voiceId &&
            av.Priority == newPriority);

        if (duplicatePriority)
            return ErrorUtils.AlreadyExists(nameof(AlternateVoice),
                $"VoiceId={voiceId} Priority={newPriority}");

        alt.Alternative = newAlternative;
        alt.AlternativeVoiceNav = newAlternativeVoice;
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
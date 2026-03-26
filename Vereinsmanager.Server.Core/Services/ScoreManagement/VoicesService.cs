using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateVoice(string Name, int InstrumentId, List<UpdateAlternateVoice>? AlternateVoices);
public record UpdateVoice(string? Name, int? InstrumentId, List<UpdateAlternateVoice>? AlternateVoices);
public record UpdateAlternateVoice(int Alternative, int Priority, bool? Deleted);

public class VoiceService
{
    private readonly ServerDatabaseContext _databaseContext;
    private readonly Lazy<PermissionService> _permissionService;

    public VoiceService(ServerDatabaseContext databaseContext, Lazy<PermissionService> permissionService)
    {
        _databaseContext = databaseContext;
        _permissionService = permissionService;
    }

    private IQueryable<Voice> BuildVoiceQuery(bool includeAlternateVoices = false, bool includeInstrument = false)
    {
        IQueryable<Voice> voiceQuery = _databaseContext.Voices;
        
        if (includeAlternateVoices)
            voiceQuery = voiceQuery.Include(voice => voice.AlternateVoices);

        if (includeInstrument)
            voiceQuery = voiceQuery.Include(voice => voice.Instrument);
        
        return voiceQuery;
    }

    public List<Voice> LoadsVoices(int[] voiceIds, bool includeAlternateVoices)
    {
        return BuildVoiceQuery(includeAlternateVoices)
            .Where(voice => voiceIds.Contains(voice.VoiceId))
            .ToList();
    }
    
    private ReturnValue<Voice> SynchronizeAlternateVoices(Voice voice, List<UpdateAlternateVoice> incomingAlternateVoices)
    {
        var normalized = incomingAlternateVoices
            .GroupBy(x => x.Alternative)
            .Select(g => g.Last())
            .ToList();

        var active = normalized
            .Where(x => (x.Deleted ?? false) == false)
            .ToList();

        var alternativeVoiceIds = active
            .Select(x => x.Alternative)
            .ToList();

        if (alternativeVoiceIds.Distinct().Count() != alternativeVoiceIds.Count)
            return ErrorUtils.AlreadyExists(nameof(AlternateVoice), "duplicate Alternative");

        var priorities = active
            .Select(x => x.Priority)
            .ToList();

        if (priorities.Distinct().Count() != priorities.Count)
            return ErrorUtils.AlreadyExists(nameof(AlternateVoice), "duplicate Priority");

        if (active.Any(x => x.Alternative <= 0 || x.Alternative == voice.VoiceId))
            return ErrorUtils.NotPermitted(nameof(AlternateVoice), "invalid Alternative");

        var idsToDeleted = normalized
            .Where(x => x.Deleted ?? false)
            .Select(x => x.Alternative)
            .ToHashSet();

        var entriesToDelete = _databaseContext.AlternateVoices
            .Where(x => x.VoiceId == voice.VoiceId)
            .Where(x => idsToDeleted.Contains(x.AlternativeId))
            .ToList();

        if (entriesToDelete.Count > 0)
            _databaseContext.AlternateVoices.RemoveRange(entriesToDelete);

        var existingAlternativeVoices = _databaseContext.Voices
            .Where(x => alternativeVoiceIds.Contains(x.VoiceId))
            .ToList();

        if (existingAlternativeVoices.Count != alternativeVoiceIds.Count)
        {
            var foundVoiceIds = existingAlternativeVoices
                .Select(x => x.VoiceId)
                .ToHashSet();

            var missingVoiceId = alternativeVoiceIds
                .First(id => !foundVoiceIds.Contains(id));

            return ErrorUtils.ValueNotFound(nameof(Voice), missingVoiceId.ToString());
        }

        var alternativeVoiceById = existingAlternativeVoices
            .ToDictionary(x => x.VoiceId);

        var existingLinks = _databaseContext.AlternateVoices
            .Where(x => x.VoiceId == voice.VoiceId)
            .ToList();

        var existingLinkByAlternative = existingLinks
            .ToDictionary(x => x.AlternativeId);

        foreach (var requested in active)
        {
            if (existingLinkByAlternative.TryGetValue(requested.Alternative, out var existingLink))
            {
                existingLink.Priority = requested.Priority;
                continue;
            }

            _databaseContext.AlternateVoices.Add(CreateViaAlternativeId(requested));
        }

        return voice;

        AlternateVoice CreateViaAlternativeId(UpdateAlternateVoice requested)
        {
            return new AlternateVoice
            {
                VoiceId = voice.VoiceId,
                Voice = voice,
                AlternativeId = requested.Alternative,
                AlternativeVoiceNav = alternativeVoiceById[requested.Alternative],
                Priority = requested.Priority
            };
        }
    }

    public ReturnValue<Voice[]> ListVoices(bool includeAlternateVoices = false, bool includeInstrument = false)
    {
        if (!_permissionService.Value.HasPermission(PermissionType.ListVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), "read all");

        return BuildVoiceQuery(includeAlternateVoices, includeInstrument).ToArray();
    }

    public ReturnValue<Voice> GetVoiceById(int voiceId, bool includeAlternateVoices = false, bool includeInstrument = false)
    {
        if (!_permissionService.Value.HasPermission(PermissionType.ListVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), voiceId.ToString());

        Voice? voice = BuildVoiceQuery(includeAlternateVoices, includeInstrument)
            .FirstOrDefault(existingVoice => existingVoice.VoiceId == voiceId);

        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), voiceId.ToString());

        return voice;
    }

    public ReturnValue<Voice> CreateVoice(CreateVoice createVoice)
    {
        if (!_permissionService.Value.HasPermission(PermissionType.CreateVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), createVoice.Name);

        Instrument? instrument = _databaseContext.Instruments
            .FirstOrDefault(existingInstrument => existingInstrument.InstrumentId == createVoice.InstrumentId);

        if (instrument == null)
            return ErrorUtils.ValueNotFound(nameof(Instrument), createVoice.InstrumentId.ToString());

        bool duplicateExists = _databaseContext.Voices.Any(existingVoice =>
            existingVoice.Name == createVoice.Name &&
            existingVoice.InstrumentId == createVoice.InstrumentId);

        if (duplicateExists)
            return ErrorUtils.AlreadyExists(nameof(Voice),
                $"{createVoice.Name} (InstrumentId={createVoice.InstrumentId})");

        Voice newVoice = new Voice
        {
            Name = createVoice.Name,
            InstrumentId = createVoice.InstrumentId,
            Instrument = instrument
        };

        _databaseContext.Voices.Add(newVoice);

        if (createVoice.AlternateVoices != null && createVoice.AlternateVoices.Count > 0)
        {
            ReturnValue<Voice> synchronizationResult =
                SynchronizeAlternateVoices(newVoice, createVoice.AlternateVoices);

            if (!synchronizationResult.IsSuccessful())
                return synchronizationResult;
        }

        _databaseContext.SaveChanges();
        return newVoice;
    }

    public ReturnValue<Voice> UpdateVoice(int voiceId, UpdateVoice updateVoice)
    {
        if (!_permissionService.Value.HasPermission(PermissionType.UpdateVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), voiceId.ToString());

        Voice? voice = _databaseContext.Voices
            .FirstOrDefault(existingVoice => existingVoice.VoiceId == voiceId);

        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), voiceId.ToString());

        if (updateVoice.Name != null)
            voice.Name = updateVoice.Name;

        if (updateVoice.InstrumentId != null)
        {
            bool instrumentExists = _databaseContext.Instruments
                .Any(existingInstrument => existingInstrument.InstrumentId == updateVoice.InstrumentId.Value);

            if (!instrumentExists)
                return ErrorUtils.ValueNotFound(nameof(Instrument), updateVoice.InstrumentId.Value.ToString());

            voice.InstrumentId = updateVoice.InstrumentId.Value;
        }

        if (updateVoice.AlternateVoices != null)
        {
            ReturnValue<Voice> synchronizationResult =
                SynchronizeAlternateVoices(voice, updateVoice.AlternateVoices);

            if (!synchronizationResult.IsSuccessful())
                return synchronizationResult;
        }

        _databaseContext.SaveChanges();
        return voice;
    }

    public ReturnValue<bool> DeleteVoice(int voiceId)
    {
        if (!_permissionService.Value.HasPermission(PermissionType.DeleteVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), voiceId.ToString());

        Voice? voice = BuildVoiceQuery(includeAlternateVoices: true)
            .FirstOrDefault(existingVoice => existingVoice.VoiceId == voiceId);

        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), voiceId.ToString());

        bool hasMusicSheets = _databaseContext.MusicSheets
            .Any(musicSheet => musicSheet.VoiceId == voiceId);

        if (hasMusicSheets)
            return ErrorUtils.NotPermitted(nameof(Voice), "delete (has MusicSheets)");

        if (voice.AlternateVoices?.Count > 0)
            _databaseContext.AlternateVoices.RemoveRange(voice.AlternateVoices);

        _databaseContext.Voices.Remove(voice);
        _databaseContext.SaveChanges();

        return true;
    }
}
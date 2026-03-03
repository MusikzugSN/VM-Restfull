using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateVoice(string Name, int InstrumentId, List<CreateAlternateVoice>? AlternateVoices);
public record UpdateVoice(string? Name, int? InstrumentId, List<CreateAlternateVoice>? AlternateVoices);
public record CreateAlternateVoice(int Alternative, int Priority);

public class VoiceService
{
    private readonly ServerDatabaseContext _databaseContext;
    private readonly Lazy<PermissionService> _permissionService;

    public VoiceService(ServerDatabaseContext databaseContext, Lazy<PermissionService> permissionService)
    {
        _databaseContext = databaseContext;
        _permissionService = permissionService;
    }

    private IQueryable<Voice> BuildVoiceQuery(bool includeInstrument = false, bool includeAlternateVoices = false)
    {
        IQueryable<Voice> voiceQuery = _databaseContext.Voices;

        if (includeInstrument)
            voiceQuery = voiceQuery.Include(voice => voice.Instrument);

        if (includeAlternateVoices)
            voiceQuery = voiceQuery.Include(voice => voice.AlternateVoices);

        return voiceQuery;
    }

    private ReturnValue<Voice> SynchronizeAlternateVoices(Voice voice, List<CreateAlternateVoice> incomingAlternateVoices)
    {
        List<int> alternativeVoiceIds = incomingAlternateVoices
            .Select(alternateVoice => alternateVoice.Alternative)
            .ToList();

        if (alternativeVoiceIds.Distinct().Count() != alternativeVoiceIds.Count)
            return ErrorUtils.AlreadyExists(nameof(AlternateVoice), "duplicate Alternative");

        List<int> priorities = incomingAlternateVoices
            .Select(alternateVoice => alternateVoice.Priority)
            .ToList();

        if (priorities.Distinct().Count() != priorities.Count)
            return ErrorUtils.AlreadyExists(nameof(AlternateVoice), "duplicate Priority");

        if (incomingAlternateVoices.Any(alternateVoice =>
                alternateVoice.Alternative <= 0 ||
                alternateVoice.Alternative == voice.VoiceId))
            return ErrorUtils.NotPermitted(nameof(AlternateVoice), "invalid Alternative");

        List<Voice> existingAlternativeVoices = _databaseContext.Voices
            .Where(existingVoice => alternativeVoiceIds.Contains(existingVoice.VoiceId))
            .ToList();

        if (existingAlternativeVoices.Count != alternativeVoiceIds.Count)
        {
            HashSet<int> foundVoiceIds = existingAlternativeVoices
                .Select(existingVoice => existingVoice.VoiceId)
                .ToHashSet();

            int missingVoiceId = alternativeVoiceIds
                .First(requestedVoiceId => !foundVoiceIds.Contains(requestedVoiceId));

            return ErrorUtils.ValueNotFound(nameof(Voice), missingVoiceId.ToString());
        }

        Dictionary<int, Voice> alternativeVoiceById = existingAlternativeVoices
            .ToDictionary(existingVoice => existingVoice.VoiceId);

        List<AlternateVoice> existingAlternateVoiceLinks = _databaseContext.AlternateVoices
            .Where(alternateVoice => alternateVoice.VoiceId == voice.VoiceId)
            .ToList();

        Dictionary<int, AlternateVoice> existingLinkByAlternative = existingAlternateVoiceLinks
            .ToDictionary(alternateVoice => alternateVoice.Alternative);

        HashSet<int> requestedAlternativeIds = alternativeVoiceIds.ToHashSet();

        List<AlternateVoice> linksToRemove = existingAlternateVoiceLinks
            .Where(alternateVoice => !requestedAlternativeIds.Contains(alternateVoice.Alternative))
            .ToList();

        if (linksToRemove.Count > 0)
            _databaseContext.AlternateVoices.RemoveRange(linksToRemove);

        foreach (CreateAlternateVoice requestedAlternateVoice in incomingAlternateVoices)
        {
            if (existingLinkByAlternative.TryGetValue(requestedAlternateVoice.Alternative, out AlternateVoice? existingLink))
            {
                existingLink.Priority = requestedAlternateVoice.Priority;
                continue;
            }

            _databaseContext.AlternateVoices.Add(new AlternateVoice
            {
                VoiceId = voice.VoiceId,
                Voice = voice,
                Alternative = requestedAlternateVoice.Alternative,
                AlternativeVoiceNav = alternativeVoiceById[requestedAlternateVoice.Alternative],
                Priority = requestedAlternateVoice.Priority
            });
        }

        return voice;
    }

    public ReturnValue<Voice[]> ListVoices(bool includeInstrument = true, bool includeAlternateVoices = true)
    {
        if (!_permissionService.Value.HasPermission(PermissionType.ListVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), "read all");

        return BuildVoiceQuery(includeInstrument, includeAlternateVoices).ToArray();
    }

    public ReturnValue<Voice> GetVoiceById(int voiceId, bool includeInstrument = false, bool includeAlternateVoices = false)
    {
        if (!_permissionService.Value.HasPermission(PermissionType.ListVoice))
            return ErrorUtils.NotPermitted(nameof(Voice), voiceId.ToString());

        Voice? voice = BuildVoiceQuery(includeInstrument, includeAlternateVoices)
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
        _databaseContext.SaveChanges();

        if (createVoice.AlternateVoices != null && createVoice.AlternateVoices.Count > 0)
        {
            ReturnValue<Voice> synchronizationResult =
                SynchronizeAlternateVoices(newVoice, createVoice.AlternateVoices);

            if (!synchronizationResult.IsSuccessful())
                return synchronizationResult;

            _databaseContext.SaveChanges();
        }

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

        if (voice.AlternateVoices.Count > 0)
            _databaseContext.AlternateVoices.RemoveRange(voice.AlternateVoices);

        _databaseContext.Voices.Remove(voice);
        _databaseContext.SaveChanges();

        return true;
    }
}
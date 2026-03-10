using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateMusicSheet(
    int ScoreId,
    int VoiceId);

public record UpdateMusicSheet(
    int? ScoreId,
    int? VoiceId);

public class MusicSheetService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public MusicSheetService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    public ReturnValue<MusicSheet[]> ListMusicSheets(int? scoreId = null, int? voiceId = null)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), $"scoreId={scoreId}, voiceId={voiceId}");

        IQueryable<MusicSheet> query = _dbContext.MusicSheets;

        if (scoreId != null)
            query = query.Where(sheet => sheet.ScoreId == scoreId.Value);

        if (voiceId != null)
            query = query.Where(sheet => sheet.VoiceId == voiceId.Value);

        return query
            .OrderBy(sheet => sheet.MusicSheetId)
            .ToArray();
    }

    public ReturnValue<MusicSheet[]> ListMusicSheets(int folderId, int[] voiceIds)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicSheet),
                $"folderId={folderId}, voiceIds=[{string.Join(",", voiceIds)}]");

        var scoreIdsInFolder = _dbContext.ScoreMusicFolders
            .Where(x => x.MusicFolderId == folderId)
            .Select(x => x.ScoreId);

        IQueryable<MusicSheet> query = _dbContext.MusicSheets
            .Where(sheet => scoreIdsInFolder.Contains(sheet.ScoreId));

        if (voiceIds.Length > 0)
            query = query.Where(sheet => voiceIds.Contains(sheet.VoiceId));

        return query
            .OrderBy(sheet => sheet.MusicSheetId)
            .ToArray();
    }

    public ReturnValue<MusicSheet> GetMusicSheetById(int musicSheetId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), musicSheetId.ToString());

        var sheet = _dbContext.MusicSheets
            .FirstOrDefault(ms => ms.MusicSheetId == musicSheetId);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), musicSheetId.ToString());

        return sheet;
    }

    public ReturnValue<MusicSheet> CreateMusicSheet(CreateMusicSheet createMusicSheet)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), $"{createMusicSheet.ScoreId}/{createMusicSheet.VoiceId}");

        var score = _dbContext.Scores.FirstOrDefault(s => s.ScoreId == createMusicSheet.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), createMusicSheet.ScoreId.ToString());

        var voice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == createMusicSheet.VoiceId);
        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), createMusicSheet.VoiceId.ToString());

        var duplicate = _dbContext.MusicSheets.Any(ms =>
            ms.ScoreId == createMusicSheet.ScoreId &&
            ms.VoiceId == createMusicSheet.VoiceId);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicSheet), $"ScoreId={createMusicSheet.ScoreId}, VoiceId={createMusicSheet.VoiceId}");

        var sheet = new MusicSheet
        {
            ScoreId = createMusicSheet.ScoreId,
            Score = score,
            VoiceId = createMusicSheet.VoiceId,
            Voice = voice,
            FilePath = string.Empty,
            FileHash = string.Empty,
            Filesize = 0,
            PageCount = 0,
            FileModifiedDate = DateTime.UtcNow
        };

        _dbContext.MusicSheets.Add(sheet);
        _dbContext.SaveChanges();
        return sheet;
    }

    public ReturnValue<MusicSheet> UpdateMusicSheet(int musicSheetId, UpdateMusicSheet updateMusicSheet)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), musicSheetId.ToString());

        var sheet = _dbContext.MusicSheets
            .FirstOrDefault(ms => ms.MusicSheetId == musicSheetId);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), musicSheetId.ToString());

        var newScoreId = updateMusicSheet.ScoreId ?? sheet.ScoreId;
        var newVoiceId = updateMusicSheet.VoiceId ?? sheet.VoiceId;

        var wouldDuplicate = _dbContext.MusicSheets.Any(ms =>
            ms.MusicSheetId != musicSheetId &&
            ms.ScoreId == newScoreId &&
            ms.VoiceId == newVoiceId);

        if (wouldDuplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicSheet), $"ScoreId={newScoreId}, VoiceId={newVoiceId}");

        if (newScoreId != sheet.ScoreId)
        {
            var score = _dbContext.Scores.FirstOrDefault(s => s.ScoreId == newScoreId);
            if (score == null)
                return ErrorUtils.ValueNotFound(nameof(Score), newScoreId.ToString());

            sheet.ScoreId = newScoreId;
            sheet.Score = score;
        }

        if (newVoiceId != sheet.VoiceId)
        {
            var voice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == newVoiceId);
            if (voice == null)
                return ErrorUtils.ValueNotFound(nameof(Voice), newVoiceId.ToString());

            sheet.VoiceId = newVoiceId;
            sheet.Voice = voice;
        }

        _dbContext.SaveChanges();
        return sheet;
    }

    public ReturnValue<bool> DeleteMusicSheet(int musicSheetId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), musicSheetId.ToString());

        var sheet = _dbContext.MusicSheets
            .FirstOrDefault(ms => ms.MusicSheetId == musicSheetId);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), musicSheetId.ToString());

        _dbContext.MusicSheets.Remove(sheet);
        _dbContext.SaveChanges();
        return true;
    }
}
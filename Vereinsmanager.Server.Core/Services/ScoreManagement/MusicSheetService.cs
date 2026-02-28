using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateMusicSheet(
    string FilePath,
    string FileHash,
    int Filesize,
    int PageCount,
    DateTime FileModifiedDate,
    int ScoreId,
    int VoiceId);

public record UpdateMusicSheet(
    string? FilePath,
    string? FileHash,
    int? Filesize,
    int? PageCount,
    DateTime? FileModifiedDate,
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
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicSheet))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), "read all");

        IQueryable<MusicSheet> query = _dbContext.MusicSheets;

        if (scoreId != null)
            query = query.Where(sheet => sheet.ScoreId == scoreId.Value);

        if (voiceId != null)
            query = query.Where(sheet => sheet.VoiceId == voiceId.Value);

        return query
            .OrderBy(sheet => sheet.MusicSheetId)
            .ToArray();
    }

    public ReturnValue<MusicSheet> GetMusicSheetById(int musicSheetId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicSheet))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), musicSheetId.ToString());

        var sheet = _dbContext.MusicSheets.FirstOrDefault(ms => ms.MusicSheetId == musicSheetId);
        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), musicSheetId.ToString());

        return sheet;
    }

    public ReturnValue<MusicSheet> CreateMusicSheet(CreateMusicSheet createMusicSheet)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateMusicSheet))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), createMusicSheet.FilePath);

        if (createMusicSheet.Filesize <= 0)
            return ErrorUtils.NotPermitted(nameof(MusicSheet), "Filesize must be > 0");

        if (createMusicSheet.PageCount <= 0)
            return ErrorUtils.NotPermitted(nameof(MusicSheet), "PageCount must be > 0");

        var score = _dbContext.Scores.FirstOrDefault(s => s.ScoreId == createMusicSheet.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), createMusicSheet.ScoreId.ToString());

        var voice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == createMusicSheet.VoiceId);
        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), createMusicSheet.VoiceId.ToString());

        var duplicate = _dbContext.MusicSheets.Any(ms =>
            ms.FileHash == createMusicSheet.FileHash &&
            ms.FilePath == createMusicSheet.FilePath);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicSheet), $"{createMusicSheet.FilePath} (Hash={createMusicSheet.FileHash})");

        var sheet = new MusicSheet
        {
            FilePath = createMusicSheet.FilePath,
            FileHash = createMusicSheet.FileHash,
            Filesize = createMusicSheet.Filesize,
            PageCount = createMusicSheet.PageCount,
            FileModifiedDate = createMusicSheet.FileModifiedDate,
            ScoreId = createMusicSheet.ScoreId,
            Score = score,
            VoiceId = createMusicSheet.VoiceId,
            Voice = voice
        };

        _dbContext.MusicSheets.Add(sheet);
        _dbContext.SaveChanges();
        return sheet;
    }

    public ReturnValue<MusicSheet> UpdateMusicSheet(int musicSheetId, UpdateMusicSheet updateMusicSheet)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateMusicSheet))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), musicSheetId.ToString());

        var sheet = _dbContext.MusicSheets.FirstOrDefault(ms => ms.MusicSheetId == musicSheetId);
        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), musicSheetId.ToString());

        var newFilePath = updateMusicSheet.FilePath ?? sheet.FilePath;
        var newFileHash = updateMusicSheet.FileHash ?? sheet.FileHash;
        var newFilesize = updateMusicSheet.Filesize ?? sheet.Filesize;
        var newPageCount = updateMusicSheet.PageCount ?? sheet.PageCount;
        var newFileModifiedDate = updateMusicSheet.FileModifiedDate ?? sheet.FileModifiedDate;
        var newScoreId = updateMusicSheet.ScoreId ?? sheet.ScoreId;
        var newVoiceId = updateMusicSheet.VoiceId ?? sheet.VoiceId;

        if (newFilesize <= 0)
            return ErrorUtils.NotPermitted(nameof(MusicSheet), "Filesize must be > 0");

        if (newPageCount <= 0)
            return ErrorUtils.NotPermitted(nameof(MusicSheet), "PageCount must be > 0");

        var wouldDuplicate = _dbContext.MusicSheets.Any(ms =>
            ms.MusicSheetId != musicSheetId &&
            ms.FileHash == newFileHash &&
            ms.FilePath == newFilePath);

        if (wouldDuplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicSheet), $"{newFilePath} (Hash={newFileHash})");

        // Nur wenn sich die IDs ändern, referenzierte Entities prüfen & setzen.
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

        sheet.FilePath = newFilePath;
        sheet.FileHash = newFileHash;
        sheet.Filesize = newFilesize;
        sheet.PageCount = newPageCount;
        sheet.FileModifiedDate = newFileModifiedDate;

        _dbContext.SaveChanges();
        return sheet;
    }

    public ReturnValue<bool> DeleteMusicSheet(int musicSheetId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteMusicSheet))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), musicSheetId.ToString());

        var sheet = _dbContext.MusicSheets.FirstOrDefault(ms => ms.MusicSheetId == musicSheetId);
        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), musicSheetId.ToString());

        _dbContext.MusicSheets.Remove(sheet);
        _dbContext.SaveChanges();
        return true;
    }
}
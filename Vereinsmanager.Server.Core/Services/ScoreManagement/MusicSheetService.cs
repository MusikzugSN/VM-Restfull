#nullable enable
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


    public MusicSheet? LoadMusicSheetById(int musicSheetId, bool includeScore = false, bool includeVoice = false)
    {
        IQueryable<MusicSheet> q = _dbContext.MusicSheets;

        if (includeScore) q = q.Include(ms => ms.Score);
        if (includeVoice) q = q.Include(ms => ms.Voice);

        return q.FirstOrDefault(ms => ms.MusicSheetId == musicSheetId);
    }

    public ReturnValue<MusicSheet[]> ListMusicSheets(
        int? scoreId = null,
        int? voiceId = null,
        bool includeScore = true,
        bool includeVoice = true)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicSheet))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), "read all");

        IQueryable<MusicSheet> q = _dbContext.MusicSheets;

        if (includeScore) q = q.Include(ms => ms.Score);
        if (includeVoice) q = q.Include(ms => ms.Voice);

        if (scoreId != null) q = q.Where(ms => ms.ScoreId == scoreId.Value);
        if (voiceId != null) q = q.Where(ms => ms.VoiceId == voiceId.Value);

        return q
            .OrderBy(ms => ms.MusicSheetId)
            .ToArray();
    }


    public ReturnValue<MusicSheet> CreateMusicSheet(CreateMusicSheet dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateMusicSheet))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), dto.FilePath);

        if (dto.Filesize <= 0)
            return ErrorUtils.NotPermitted(nameof(MusicSheet), "Filesize must be > 0");

        if (dto.PageCount <= 0)
            return ErrorUtils.NotPermitted(nameof(MusicSheet), "PageCount must be > 0");

        var score = _dbContext.Scores.FirstOrDefault(s => s.ScoreId == dto.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), dto.ScoreId.ToString());

        var voice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == dto.VoiceId);
        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), dto.VoiceId.ToString());

        var duplicate = _dbContext.MusicSheets.Any(ms =>
            ms.FileHash == dto.FileHash &&
            ms.FilePath == dto.FilePath);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicSheet), $"{dto.FilePath} (Hash={dto.FileHash})");

        var sheet = new MusicSheet
        {
            FilePath = dto.FilePath,
            FileHash = dto.FileHash,
            Filesize = dto.Filesize,
            PageCount = dto.PageCount,
            FileModifiedDate = dto.FileModifiedDate,
            ScoreId = dto.ScoreId,
            Score = score,
            VoiceId = dto.VoiceId,
            Voice = voice
        };

        _dbContext.MusicSheets.Add(sheet);
        _dbContext.SaveChanges();
        return sheet;
    }


    public ReturnValue<MusicSheet> UpdateMusicSheet(int musicSheetId, UpdateMusicSheet dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateMusicSheet))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), musicSheetId.ToString());

        var sheet = _dbContext.MusicSheets
            .Include(ms => ms.Score)
            .Include(ms => ms.Voice)
            .FirstOrDefault(ms => ms.MusicSheetId == musicSheetId);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), musicSheetId.ToString());

        var newFilePath = dto.FilePath ?? sheet.FilePath;
        var newFileHash = dto.FileHash ?? sheet.FileHash;
        var newFilesize = dto.Filesize ?? sheet.Filesize;
        var newPageCount = dto.PageCount ?? sheet.PageCount;
        var newFileModifiedDate = dto.FileModifiedDate ?? sheet.FileModifiedDate;
        var newScoreId = dto.ScoreId ?? sheet.ScoreId;
        var newVoiceId = dto.VoiceId ?? sheet.VoiceId;

        if (newFilesize <= 0)
            return ErrorUtils.NotPermitted(nameof(MusicSheet), "Filesize must be > 0");

        if (newPageCount <= 0)
            return ErrorUtils.NotPermitted(nameof(MusicSheet), "PageCount must be > 0");

        Score? score = sheet.Score;
        if (newScoreId != sheet.ScoreId)
        {
            score = _dbContext.Scores.FirstOrDefault(s => s.ScoreId == newScoreId);
            if (score == null)
                return ErrorUtils.ValueNotFound(nameof(Score), newScoreId.ToString());
        }

        Voice? voice = sheet.Voice;
        if (newVoiceId != sheet.VoiceId)
        {
            voice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == newVoiceId);
            if (voice == null)
                return ErrorUtils.ValueNotFound(nameof(Voice), newVoiceId.ToString());
        }

        var wouldDuplicate = _dbContext.MusicSheets.Any(ms =>
            ms.MusicSheetId != musicSheetId &&
            ms.FileHash == newFileHash &&
            ms.FilePath == newFilePath);

        if (wouldDuplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicSheet), $"{newFilePath} (Hash={newFileHash})");

        sheet.FilePath = newFilePath;
        sheet.FileHash = newFileHash;
        sheet.Filesize = newFilesize;
        sheet.PageCount = newPageCount;
        sheet.FileModifiedDate = newFileModifiedDate;

        sheet.ScoreId = newScoreId;
        sheet.Score = score!;

        sheet.VoiceId = newVoiceId;
        sheet.Voice = voice!;

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
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Syncfusion.Pdf.Parsing;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record UpdateMusicSheet(
    int? ScoreId,
    int? VoiceId,
    MusicSheetStatus? Status);

public class MusicSheetService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public MusicSheetService(
        ServerDatabaseContext dbContext,
        Lazy<PermissionService> permissionServiceLazy,
        IWebHostEnvironment hostingEnvironment)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
        _hostingEnvironment = hostingEnvironment;
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

    public ReturnValue<MusicSheet> CreateMusicSheet(CreateMusicSheetRequestDto request)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), $"{request.ScoreId}/{request.VoiceId}");

        var score = _dbContext.Scores.FirstOrDefault(s => s.ScoreId == request.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), request.ScoreId.ToString());

        var voice = _dbContext.Voices.FirstOrDefault(v => v.VoiceId == request.VoiceId);
        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), request.VoiceId.ToString());

        var duplicate = _dbContext.MusicSheets.Any(ms =>
            ms.ScoreId == request.ScoreId &&
            ms.VoiceId == request.VoiceId);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicSheet), $"ScoreId={request.ScoreId}, VoiceId={request.VoiceId}");

        string baseFolderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "Scores");
        Directory.CreateDirectory(baseFolderPath);

        string scoreFolderPath = Path.Combine(baseFolderPath, request.ScoreId.ToString());
        Directory.CreateDirectory(scoreFolderPath);

        string extension = Path.GetExtension(request.File!.FileName);

        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".pdf";
        }

        string fileId = Guid.NewGuid().ToString("N");
        string storedFileName = fileId + extension;
        string filePath = Path.Combine(scoreFolderPath, storedFileName);

        try
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                request.File.CopyTo(fileStream);
            }

            (string fileHash, int pageCount) = ReadPdfMetadata(filePath);

            var sheet = new MusicSheet
            {
                ScoreId = request.ScoreId,
                Score = score,
                VoiceId = request.VoiceId,
                Voice = voice,
                FilePath = filePath,
                FileHash = fileHash,
                Filesize = (int)request.File.Length,
                PageCount = pageCount,
                FileModifiedDate = DateTime.UtcNow,
                Status = MusicSheetStatus.Ungeprueft
            };

            _dbContext.MusicSheets.Add(sheet);
            _dbContext.SaveChanges();

            return sheet;
        }
        catch (DbUpdateException)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return ErrorUtils.AlreadyExists(nameof(MusicSheet), $"ScoreId={request.ScoreId}, VoiceId={request.VoiceId}");
        }
        catch
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            throw;
        }
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

        if (updateMusicSheet.Status != null)
        {
            sheet.Status = updateMusicSheet.Status.Value;
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

        try
        {
            if (!string.IsNullOrWhiteSpace(sheet.FilePath) && File.Exists(sheet.FilePath))
            {
                File.Delete(sheet.FilePath);
            }

            _dbContext.MusicSheets.Remove(sheet);
            _dbContext.SaveChanges();

            return true;
        }
        catch (Exception ex)
        {
            return ErrorUtils.AlreadyExists(nameof(MusicSheet), ex.Message);
        }
    }

    private static (string FileHash, int PageCount) ReadPdfMetadata(string filePath)
    {
        using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        using SHA256 sha256 = SHA256.Create();
        string fileHash = Convert.ToHexString(sha256.ComputeHash(stream));

        stream.Position = 0;

        using PdfLoadedDocument document = new PdfLoadedDocument(stream);
        int pageCount = document.Pages.Count;

        return (fileHash, pageCount);
    }

    public ReturnValue<MusicSheet> UpdateMusicSheetPdf(int musicSheetId, IFormFile file)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), musicSheetId.ToString());

        var sheet = _dbContext.MusicSheets
            .FirstOrDefault(ms => ms.MusicSheetId == musicSheetId);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), musicSheetId.ToString());

        string scoreFolderPath = Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Data",
            "Scores",
            sheet.ScoreId.ToString());

        Directory.CreateDirectory(scoreFolderPath);

        string extension = Path.GetExtension(file.FileName);

        if (string.IsNullOrWhiteSpace(extension))
            extension = ".pdf";

        string fileId = Guid.NewGuid().ToString("N");
        string storedFileName = fileId + extension;
        string filePath = Path.Combine(scoreFolderPath, storedFileName);

        try
        {
            if (!string.IsNullOrWhiteSpace(sheet.FilePath) && File.Exists(sheet.FilePath))
            {
                File.Delete(sheet.FilePath);
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                file.CopyTo(fileStream);
            }

            (string fileHash, int pageCount) = ReadPdfMetadata(filePath);

            sheet.FilePath = filePath;
            sheet.FileHash = fileHash;
            sheet.Filesize = (int)file.Length;
            sheet.PageCount = pageCount;
            sheet.FileModifiedDate = DateTime.UtcNow;

            _dbContext.SaveChanges();

            return sheet;
        }
        catch
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            throw;
        }
    }
}
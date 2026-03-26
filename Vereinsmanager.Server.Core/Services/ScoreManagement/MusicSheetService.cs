using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using TagUser = Vereinsmanager.Database.ScoreManagment.TagUser;

namespace Vereinsmanager.Services.ScoreManagement;

//public record UpdateMusicSheet(
//  int? ScoreId,
// int? VoiceId);

public record MusicSheetTagChange(int TagId, bool? Deleted = null);

public record UpdateMusicSheet(int? ScoreId, int? VoiceId, List<MusicSheetTagChange>? Tags = null);


public class MusicSheetService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly Lazy<VoiceService> _voiceServiceLazy;
    private readonly Lazy<UserContext> _userContextLazy;

    public MusicSheetService(
        ServerDatabaseContext dbContext,
        Lazy<PermissionService> permissionServiceLazy,
        IWebHostEnvironment hostingEnvironment,
        Lazy<VoiceService> voiceServiceLazy,
        Lazy<UserContext> userContextLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
        _hostingEnvironment = hostingEnvironment;
        _voiceServiceLazy = voiceServiceLazy;
        _userContextLazy = userContextLazy;
    }

    private IQueryable<MusicSheet> BaseMusicSheetQuery(int[]? scoreIds = null, int[]? voiceIds = null)
    {
        var dbSet = _dbContext.MusicSheets.AsQueryable();
        
        if (scoreIds != null)
            dbSet = dbSet.Where(x => scoreIds.Contains(x.ScoreId));
        
        if (voiceIds != null)
            dbSet = dbSet.Where(x => voiceIds.Contains(x.VoiceId));
        
        return dbSet;
    }

    public MusicSheet? LoadById(int id)
    {
        return BaseMusicSheetQuery().FirstOrDefault(x => x.MusicSheetId == id);
    }

    public ReturnValue<MusicSheet[]> ListMusicSheets(int? scoreId = null, int? voiceId = null)
    {
        return BaseMusicSheetQuery(
            scoreId != null ? [scoreId.Value] : null,
            voiceId != null ? [voiceId.Value] : null
            ).ToArray();
    }

    public ReturnValue<MusicSheet[]> ListMusicSheets(int folderId, int[] voiceIds)
    {
        var scoreIds = _dbContext.ScoreMusicFolders
            .Where(x => x.MusicFolderId == folderId)
            .Select(x => x.ScoreId)
            .ToArray();

        var searchVoiceIds = voiceIds.Length > 0 ? voiceIds : null;
        
        var musicSheets = BaseMusicSheetQuery(
            scoreIds,
            searchVoiceIds
        ).ToArray();

        if (searchVoiceIds == null)
            return musicSheets;

        var missingEntities = scoreIds
            .SelectMany(scoreId => voiceIds.Select(voiceId => new { ScoreId = scoreId, MissingVoiceId = voiceId }))
            .Where(x => musicSheets.All(sheet => sheet.ScoreId != x.ScoreId && sheet.VoiceId != x.MissingVoiceId))
            .ToArray();
        
        var missingVoiceIds = missingEntities.Select(x => x.MissingVoiceId).Distinct().ToArray();
        var missingScoreIds = missingEntities.Select(x => x.ScoreId).Distinct().ToArray();
        var voices = _voiceServiceLazy.Value.LoadsVoices(missingVoiceIds, true);
        var alternativeVoiceIds = voices.SelectMany(x => x.AlternateVoices?.Select(y => y.AlternativeId) ?? []).Distinct().ToArray();
        
        var allPossibleMusicSheets = _dbContext.MusicSheets.Where(x => missingScoreIds.Contains(x.ScoreId) && alternativeVoiceIds.Contains(x.VoiceId)).ToList();
        
        if (allPossibleMusicSheets.Count == 0)
            return musicSheets; // Keine Alternativen vorhanden, keine Suche 
        
        foreach (var missingEntity in missingEntities)
        {
            var searchingVoice = voices.FirstOrDefault(x => x.VoiceId == missingEntity.MissingVoiceId);
            
            if (searchingVoice == null)
                continue; // Sollte nicht passieren da die VoiceIds vorher lud, just in case
            
            var alternativeMusicSheet = FindBestAlternative(searchingVoice, missingEntity.ScoreId, allPossibleMusicSheets);
            if (alternativeMusicSheet != null)
                musicSheets = musicSheets.Append(alternativeMusicSheet).ToArray();
        }
        
        return musicSheets;

        MusicSheet? FindBestAlternative(Voice voice, int scoreId, List<MusicSheet> allMusicSheets, int priority = 1)
        {
            var altVoiceId = voice.AlternateVoices?
                .Where(x => x.Priority == priority)
                .Select(x => x.AlternativeId)
                .FirstOrDefault(-1);
            
            // Abbruch wenn keine Alternative gefunden wird, weil die höchste Prio läuft
            if (altVoiceId == -1)
                return null;
            
            var musicSheet = allMusicSheets.FirstOrDefault(x => x.ScoreId == scoreId && x.VoiceId == altVoiceId);
            if (musicSheet != null)
                return musicSheet;
            
            return FindBestAlternative(
                voice, 
                scoreId, 
                allMusicSheets, 
                priority + 1);
        }
    }
    
    public ReturnValue<MusicSheet[]> ListMusicSheetsByStatus(int status, int[] voiceIds)
    {
        return BaseMusicSheetQuery(
            null, 
            voiceIds.Length > 0 ? voiceIds : null)
            .Where(x => x.Status == (MusicSheetStatus)status)
            .ToArray();
    }

    public ReturnValue<MusicSheet> GetMusicSheetById(int id, bool includeTags = false)
    {
        IQueryable<MusicSheet> q = _dbContext.MusicSheets;

        if (includeTags)
        {
            q = q.Include(s => s.TagUsers);
        }

        var sheet = q.FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        return sheet;
    }


    public ReturnValue<List<MusicSheet>> CreateMusicSheets(CreateMusicSheetRequestDto request)
    {
        var scoreCount = _dbContext.Scores.Count(x => x.ScoreId == request.ScoreId);
        if (scoreCount != 1)
            return ErrorUtils.ValueNotFound(nameof(Score), request.ScoreId.ToString());

        var voiceIds = request.Files.Select(x => x.VoiceId).ToList();
        var foundVoiceIdCount = _dbContext.Voices.Count(x => voiceIds.Contains(x.VoiceId));
        
        if (foundVoiceIdCount < voiceIds.Count)
            return ErrorUtils.ValueNotFound(nameof(Voice), $"Es wurden {voiceIds.Count - foundVoiceIdCount} ungültige VoiceIds übergeben.");
        
        if (request.Files.Length == 0)
            return ErrorUtils.ValueNotFound("Files", "Keine Dateien übergeben.");

        string basePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "Scores");
        Directory.CreateDirectory(basePath);

        string scoreFolder = Path.Combine(basePath, request.ScoreId.ToString());
        Directory.CreateDirectory(scoreFolder);

        var storesMusicSheets = new List<MusicSheet>();

        foreach (var createMusicSheetFile in request.Files)
        {
            if (createMusicSheetFile.VoiceId == null)
                return ErrorUtils.ValueNotFound("VoiceId", "Keine Dateien übergeben.");
            
            if (createMusicSheetFile.File == null)
                return ErrorUtils.ValueNotFound("File", "Keine Dateien übergeben.");
            
            string fileId = Guid.NewGuid().ToString("N");
            string filePath = Path.Combine(scoreFolder, fileId + ".pdf");
            
            SaveSingleFileAsPdf(createMusicSheetFile.File, filePath);
            var fileMetadata = ReadSingleFileMetadata(filePath);
            storesMusicSheets.Add(new MusicSheet
            {
                ScoreId = request.ScoreId,
                VoiceId = createMusicSheetFile.VoiceId.Value,
                
                FileName = fileId + ".pdf",
                FileHash = fileMetadata.FileHash,
                Filesize = fileMetadata.Filesize,
                PageCount = fileMetadata.PageCount,
            });
            
        }

        _dbContext.MusicSheets.AddRange(storesMusicSheets);
        _dbContext.SaveChanges();

        return storesMusicSheets;
    }

    public ReturnValue<MusicSheet> UpdateMusicSheet(int id, UpdateMusicSheet update)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateValidateNotes))
            return ErrorUtils.NotPermitted(nameof(MusicSheet), id.ToString());

        var sheet = _dbContext.MusicSheets
            .Include(x => x.TagUsers)
            .FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        if (update.ScoreId != null)
            sheet.ScoreId = update.ScoreId.Value;

        if (update.VoiceId != null)
            sheet.VoiceId = update.VoiceId.Value;

        if (update.Tags?.Count > 0)
        {
            var normalized = update.Tags
                .GroupBy(x => x.TagId)
                .Select(g => g.Last())
                .ToList();

            var idsToDelete = normalized
                .Where(x => x.Deleted == true)
                .Select(x => x.TagId)
                .ToHashSet();

            var idsToAdd = normalized
                .Where(x => x.Deleted != true)
                .Select(x => x.TagId)
                .ToHashSet();

            var user = _userContextLazy.Value.GetUserModel();

            if (user == null)
            {
                return ErrorUtils.NotPermitted(nameof(MusicSheet), "Require Login");
            }
            
            if (idsToDelete.Count > 0)
            {
                var toDelete = sheet.TagUsers?
                    .Where(x => idsToDelete.Contains(x.TagId) && user.UserId == x.UserId)
                    .ToList() ?? [];

                foreach (var tag in toDelete)
                    sheet.TagUsers?.Remove(tag);
            }

            if (idsToAdd.Count > 0)
            {
                var existingIds = sheet.TagUsers?.Where(x => user.UserId == x.UserId).Select(x => x.TagId).ToHashSet() ?? [];
                var tagsToAttach = _dbContext.Tags
                    .Select(x => x.TagId)
                    .Where(x => idsToAdd.Contains(x) && !existingIds.Contains(x))
                    .ToList();

                var missingIds = idsToAdd
                    .Where(x => !tagsToAttach.Contains(x) && !existingIds.Contains(x))
                    .ToArray();

                if (missingIds.Length > 0)
                    return ErrorUtils.ValueNotFound(nameof(Tag), string.Join(',', missingIds));

                foreach (var tag in tagsToAttach)
                {
                    sheet.TagUsers?.Add(new TagUser
                    {
                        MusicSheetId = sheet.MusicSheetId,
                        TagId = tag,
                        UserId = user.UserId
                    });
                }
            }
        }

        _dbContext.SaveChanges();

        return sheet;
    }

    public ReturnValue<MusicSheet> ReplaceMusicSheetFile(int id, IFormFile file)
    {
        var sheet = _dbContext.MusicSheets
            .FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        string scoreFolder = Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Data",
            "Scores",
            sheet.ScoreId.ToString());

        Directory.CreateDirectory(scoreFolder);

        var filePath = Path.Combine(scoreFolder, sheet.FileName);
        if (File.Exists(filePath))
            File.Delete(filePath);


        SaveSingleFileAsPdf(file, filePath);
        var fileMetadata = ReadSingleFileMetadata(filePath);
        
        sheet.FileHash = fileMetadata.FileHash;
        sheet.Filesize = fileMetadata.Filesize;
        sheet.PageCount = fileMetadata.PageCount;
        
        _dbContext.Update(sheet);
        _dbContext.SaveChanges();

        return sheet;
    }

    public ReturnValue<List<MusicSheet>> CropPdfByVoices(CropPdfByVoicesRequestDto request)
    {
        var scoreIds = request.Ranges.Select(x => x.ScoreId).Distinct().ToArray();
        var foundScores = _dbContext.Scores.Count(x => scoreIds.Contains(x.ScoreId));
        if (foundScores < scoreIds.Length)
            return ErrorUtils.ValueNotFound(nameof(Score), $"{scoreIds.Length - foundScores} ungültige ScoreIds übergeben.");

        if (request.File == null)
            return ErrorUtils.ValueNotFound(nameof(File), "null");

        var duplicateVoiceIds = request.Ranges
            .GroupBy(x => x.ScoreId)
            .Where(g => g.Select(x => x.VoiceId).Distinct().Count() < g.Count())
            .Select(g => g.Key)
            .ToArray();

        if (duplicateVoiceIds.Length > 0)
        {
            return ErrorUtils.AlreadyExists(
                nameof(MusicSheet),
                $"mehrfache VoiceIds in ranges: {string.Join(", ", duplicateVoiceIds)}");
        }

        string basePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "Scores");
        Directory.CreateDirectory(basePath);

        List<MusicSheet> createdSheets = new List<MusicSheet>();

        using (Stream inputStream = request.File.OpenReadStream())
        using (PdfLoadedDocument sourceDocument = new PdfLoadedDocument(inputStream))
        {
            int totalPages = sourceDocument.Pages.Count;
            
            foreach (var range in request.Ranges)
            {
                var voiceExists = _dbContext.Voices.Any(x => x.VoiceId == range.VoiceId);
                if (!voiceExists)
                    return ErrorUtils.ValueNotFound(nameof(Voice), range.VoiceId.ToString());
                
                var existingMusicSheet = _dbContext.MusicSheets
                    .Any(x => x.ScoreId == range.ScoreId && x.VoiceId == range.VoiceId);

                if (existingMusicSheet)
                {
                    return ErrorUtils.AlreadyExists(
                        nameof(MusicSheet),
                        $"ScoreId {range.ScoreId}, VoiceId {range.VoiceId}");
                }

                if (range.FromPage > totalPages || range.ToPage > totalPages)
                {
                    return ErrorUtils.ValueOutOfRange(
                        nameof(CropPdfByVoicesRequestDto),
                        $"Range {range.FromPage}-{range.ToPage} liegt außerhalb der PDF mit {totalPages} Seiten.");
                }

                string scoreFolder = Path.Combine(basePath, range.ScoreId.ToString());
                Directory.CreateDirectory(scoreFolder);
                
                string fileId = Guid.NewGuid().ToString("N");
                string filePath = Path.Combine(scoreFolder, fileId + ".pdf");

                using (PdfDocument newDocument = new PdfDocument())
                {
                    for (int pageNumber = range.FromPage; pageNumber <= range.ToPage; pageNumber++)
                    {
                        newDocument.ImportPage(sourceDocument, pageNumber - 1);
                    }

                    using (FileStream output = new FileStream(filePath, FileMode.Create))
                    {
                        newDocument.Save(output);
                    }
                }

                var fileMetadata = ReadSingleFileMetadata(filePath);
                
                var musicSheet = new MusicSheet
                {
                    ScoreId = range.ScoreId,
                    VoiceId = range.VoiceId,
                    FileHash = fileMetadata.FileHash,
                    Filesize = fileMetadata.Filesize,
                    PageCount = fileMetadata.PageCount,
                    Status = MusicSheetStatus.Ungeprueft,
                    FileName = fileId + ".pdf"
                };

                createdSheets.Add(musicSheet);
            }
            
        }

        _dbContext.AddRange(createdSheets);
        _dbContext.SaveChanges();

        return createdSheets;
    }

    public ReturnValue<bool> DeleteMusicSheet(int id)
    {
        var sheet = _dbContext.MusicSheets
            .FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        string scoreFolder = Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Data",
            "Scores",
            sheet.ScoreId.ToString());

        Directory.CreateDirectory(scoreFolder);

        var filePath = Path.Combine(scoreFolder, sheet.FileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        _dbContext.MusicSheets.Remove(sheet);
        _dbContext.SaveChanges();

        return true;
    }
    
    private static (string FileHash, int Filesize, int PageCount) ReadSingleFileMetadata(string filePath)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            using (SHA256 sha = SHA256.Create())
            {
                string hash = Convert.ToHexString(sha.ComputeHash(stream));
                int fileSize = (int)stream.Length;

                stream.Position = 0;

                using (PdfLoadedDocument document = new PdfLoadedDocument(stream))
                {
                    return (hash, fileSize, document.Pages.Count);
                }
            }
        }
    }

    private static bool IsPdfFile(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() == ".pdf";
    }

    private static bool IsSupportedImageFile(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();

        return ext == ".jpg"
            || ext == ".jpeg"
            || ext == ".png"
            || ext == ".bmp"
            || ext == ".gif";
    }

    private static void SaveSingleFileAsPdf(IFormFile sourceFile, string targetPdfPath)
    {
        if (IsPdfFile(sourceFile.FileName))
        {
            using (FileStream output = new FileStream(targetPdfPath, FileMode.Create))
            {
                sourceFile.CopyTo(output);
            }
            return;
        }

        if (!IsSupportedImageFile(sourceFile.FileName))
            throw new InvalidOperationException("Dateityp nicht erlaubt.");

        using (Stream input = sourceFile.OpenReadStream())
        using (PdfDocument document = new PdfDocument())
        {
            PdfBitmap image = new PdfBitmap(input);

            document.PageSettings.Size = PdfPageSize.A4;
            document.PageSettings.Orientation =
                image.Width > image.Height
                    ? PdfPageOrientation.Landscape
                    : PdfPageOrientation.Portrait;

            PdfPage page = document.Pages.Add(); 
            
            float pageWidth = page.Size.Width;
            float pageHeight = page.Size.Height;

            float imageWidth = image.Width;
            float imageHeight = image.Height;

            float scaleX = pageWidth / imageWidth;
            float scaleY = pageHeight / imageHeight;
            float scale = Math.Min(scaleX, scaleY);

            float drawWidth = imageWidth * scale;
            float drawHeight = imageHeight * scale;

            page.Graphics.DrawImage(image, 0, 0, drawWidth, drawHeight);

            using (FileStream output = new FileStream(targetPdfPath, FileMode.Create))
            {
                document.Save(output);
            }
        }
    }
}




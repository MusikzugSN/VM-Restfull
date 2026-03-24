using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;

namespace Vereinsmanager.Services.ScoreManagement;

public record UpdateMusicSheet(
    int? ScoreId,
    int? VoiceId,
    bool? IsMarschbuch,
    MusicSheetStatus? Status);

public class MusicSheetService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IMemoryCache _memoryCache;

    public MusicSheetService(
        ServerDatabaseContext dbContext,
        Lazy<PermissionService> permissionServiceLazy,
        IWebHostEnvironment hostingEnvironment,
        IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
        _hostingEnvironment = hostingEnvironment;
        _memoryCache = memoryCache;
    }

    public ReturnValue<MusicSheet[]> ListMusicSheets(int? scoreId = null, int? voiceId = null)
    {
        IQueryable<MusicSheet> query = _dbContext.MusicSheets
            .Include(x => x.Files);

        if (scoreId != null)
            query = query.Where(x => x.ScoreId == scoreId);

        if (voiceId != null)
            query = query.Where(x => x.VoiceId == voiceId);

        return query.ToArray();
    }

    public ReturnValue<MusicSheet[]> ListMusicSheets(int folderId, int[] voiceIds)
    {
        var scoreIds = _dbContext.ScoreMusicFolders
            .Where(x => x.MusicFolderId == folderId)
            .Select(x => x.ScoreId);

        IQueryable<MusicSheet> query = _dbContext.MusicSheets
            .Include(x => x.Files)
            .Where(x => scoreIds.Contains(x.ScoreId));

        if (voiceIds.Length > 0)
            query = query.Where(x => voiceIds.Contains(x.VoiceId));

        return query.ToArray();
    }

    public ReturnValue<MusicSheet> GetMusicSheetById(int id)
    {
        var sheet = _dbContext.MusicSheets
            .Include(x => x.Files)
            .FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        return sheet;
    }

    public ReturnValue<byte[]> GetViewerPdfContent(int musicSheetId, int? fromPage = null, int? toPage = null)
    {
        var sheet = _dbContext.MusicSheets
            .Include(x => x.Files)
            .FirstOrDefault(x => x.MusicSheetId == musicSheetId);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), musicSheetId.ToString());

        string[] filePaths = sheet.Files
            .OrderBy(x => x.SortOrder)
            .Select(x => x.FilePath)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        if (filePaths.Length == 0)
            return ErrorUtils.ValueNotFound(nameof(MusicSheetFile), musicSheetId.ToString());

        foreach (string filePath in filePaths)
        {
            if (!File.Exists(filePath))
                return ErrorUtils.ValueNotFound(nameof(MusicSheetFile), filePath);
        }

        string cacheKey =
            $"musicsheet:viewer:{musicSheetId}:{sheet.FileHash}:{sheet.FileModifiedDate.Ticks}:{fromPage?.ToString() ?? "all"}:{toPage?.ToString() ?? "all"}";

        byte[] bytes = _memoryCache.GetOrCreate(cacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

            return BuildViewerPdfContent(filePaths, fromPage, toPage);
        })!;

        return bytes.ToArray();
    }

    public ReturnValue<MusicSheet> CreateMusicSheet(CreateMusicSheetRequestDto request)
    {
        var score = _dbContext.Scores.FirstOrDefault(x => x.ScoreId == request.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), request.ScoreId.ToString());

        var voice = _dbContext.Voices.FirstOrDefault(x => x.VoiceId == request.VoiceId);
        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), request.VoiceId.ToString());

        if (request.Files == null || request.Files.Length == 0)
            return ErrorUtils.ValueNotFound("Files", "Keine Dateien übergeben.");

        string basePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "Scores");
        Directory.CreateDirectory(basePath);

        string scoreFolder = Path.Combine(basePath, request.ScoreId.ToString());
        Directory.CreateDirectory(scoreFolder);

        var storedFiles = new List<MusicSheetFile>();

        for (int i = 0; i < request.Files.Length; i++)
        {
            var uploadedFile = request.Files[i];

            string fileId = Guid.NewGuid().ToString("N");
            string filePath = Path.Combine(scoreFolder, fileId + ".pdf");

            SaveSingleFileAsPdf(uploadedFile, filePath);

            var fileMetadata = ReadSingleFileMetadata(filePath);

            storedFiles.Add(new MusicSheetFile
            {
                FilePath = filePath,
                SortOrder = i,
                Filesize = fileMetadata.Filesize,
                PageCount = fileMetadata.PageCount,
                FileHash = fileMetadata.FileHash
            });
        }

        var aggregatedMetadata = ReadStoredFilesMetadata(storedFiles);

        MusicSheet sheet = new MusicSheet
        {
            ScoreId = request.ScoreId,
            VoiceId = request.VoiceId,
            Score = score,
            Voice = voice,
            Files = storedFiles,
            FileHash = aggregatedMetadata.FileHash,
            Filesize = aggregatedMetadata.Filesize,
            PageCount = aggregatedMetadata.PageCount,
            FileModifiedDate = DateTime.UtcNow,
            IsMarschbuch = request.IsMarschbuch,
            Status = MusicSheetStatus.Ungeprueft
        };

        _dbContext.MusicSheets.Add(sheet);
        _dbContext.SaveChanges();

        return _dbContext.MusicSheets
            .Include(x => x.Files)
            .First(x => x.MusicSheetId == sheet.MusicSheetId);
    }

    public ReturnValue<MusicSheet> UpdateMusicSheet(int id, UpdateMusicSheet update)
    {
        var sheet = _dbContext.MusicSheets
            .Include(x => x.Files)
            .FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        if (update.ScoreId != null)
            sheet.ScoreId = update.ScoreId.Value;

        if (update.VoiceId != null)
            sheet.VoiceId = update.VoiceId.Value;

        if (update.IsMarschbuch != null)
            sheet.IsMarschbuch = update.IsMarschbuch.Value;

        if (update.Status != null)
            sheet.Status = update.Status.Value;

        _dbContext.SaveChanges();

        return sheet;
    }

    public ReturnValue<MusicSheet> ReplaceMusicSheetFiles(int id, IFormFile[] files)
    {
        var sheet = _dbContext.MusicSheets
            .Include(x => x.Files)
            .FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        if (files == null || files.Length == 0)
            return ErrorUtils.ValueNotFound("Files", "Keine Dateien übergeben.");

        string scoreFolder = Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Data",
            "Scores",
            sheet.ScoreId.ToString());

        Directory.CreateDirectory(scoreFolder);

        foreach (var existingFile in sheet.Files.ToList())
        {
            if (File.Exists(existingFile.FilePath))
                File.Delete(existingFile.FilePath);
        }

        _dbContext.MusicSheetFiles.RemoveRange(sheet.Files);
        sheet.Files.Clear();

        var newFiles = new List<MusicSheetFile>();

        for (int i = 0; i < files.Length; i++)
        {
            var uploadedFile = files[i];

            string fileId = Guid.NewGuid().ToString("N");
            string filePath = Path.Combine(scoreFolder, fileId + ".pdf");

            SaveSingleFileAsPdf(uploadedFile, filePath);

            var fileMetadata = ReadSingleFileMetadata(filePath);

            newFiles.Add(new MusicSheetFile
            {
                MusicSheet = sheet,
                FilePath = filePath,
                SortOrder = i,
                Filesize = fileMetadata.Filesize,
                PageCount = fileMetadata.PageCount,
                FileHash = fileMetadata.FileHash
            });
        }

        foreach (var file in newFiles)
            sheet.Files.Add(file);

        var aggregatedMetadata = ReadStoredFilesMetadata(sheet.Files);

        sheet.FileHash = aggregatedMetadata.FileHash;
        sheet.Filesize = aggregatedMetadata.Filesize;
        sheet.PageCount = aggregatedMetadata.PageCount;
        sheet.FileModifiedDate = DateTime.UtcNow;

        _dbContext.SaveChanges();

        return sheet;
    }

    public ReturnValue<MusicSheet[]> CropPdfByVoices(CropPdfByVoicesRequestDto request)
    {
        if (request.File == null)
            return ErrorUtils.ValueNotFound(nameof(File), "null");

        var duplicatePairs = request.Ranges
            .GroupBy(x => new { x.ScoreId, x.VoiceId })
            .Where(g => g.Count() > 1)
            .Select(g => $"ScoreId {g.Key.ScoreId}, VoiceId {g.Key.VoiceId}")
            .ToArray();

        if (duplicatePairs.Length > 0)
        {
            return ErrorUtils.AlreadyExists(
                nameof(MusicSheet),
                $"mehrfache Kombinationen in ranges: {string.Join("; ", duplicatePairs)}");
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
                var score = _dbContext.Scores.FirstOrDefault(x => x.ScoreId == range.ScoreId);
                if (score == null)
                    return ErrorUtils.ValueNotFound(nameof(Score), range.ScoreId.ToString());

                var voice = _dbContext.Voices.FirstOrDefault(x => x.VoiceId == range.VoiceId);
                if (voice == null)
                    return ErrorUtils.ValueNotFound(nameof(Voice), range.VoiceId.ToString());

                var existingMusicSheet = _dbContext.MusicSheets
                    .Include(x => x.Files)
                    .FirstOrDefault(x => x.ScoreId == range.ScoreId && x.VoiceId == range.VoiceId);

                if (existingMusicSheet != null)
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

                MusicSheetFile musicSheetFile = new MusicSheetFile
                {
                    FilePath = filePath,
                    SortOrder = 0,
                    Filesize = fileMetadata.Filesize,
                    PageCount = fileMetadata.PageCount,
                    FileHash = fileMetadata.FileHash
                };

                MusicSheet musicSheet = new MusicSheet
                {
                    ScoreId = range.ScoreId,
                    Score = score,
                    VoiceId = range.VoiceId,
                    Voice = voice,
                    Files = new List<MusicSheetFile> { musicSheetFile },
                    FileHash = fileMetadata.FileHash,
                    Filesize = fileMetadata.Filesize,
                    PageCount = fileMetadata.PageCount,
                    FileModifiedDate = DateTime.UtcNow,
                    IsMarschbuch = false,
                    Status = MusicSheetStatus.Ungeprueft
                };

                _dbContext.MusicSheets.Add(musicSheet);
                createdSheets.Add(musicSheet);
            }

            _dbContext.SaveChanges();
        }

        var createdIds = createdSheets.Select(x => x.MusicSheetId).ToArray();

        var loadedSheets = _dbContext.MusicSheets
            .Include(x => x.Files)
            .Where(x => createdIds.Contains(x.MusicSheetId))
            .OrderBy(x => x.ScoreId)
            .ThenBy(x => x.VoiceId)
            .ToArray();

        return loadedSheets;
    }

    public ReturnValue<bool> DeleteMusicSheet(int id)
    {
        var sheet = _dbContext.MusicSheets
            .Include(x => x.Files)
            .FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        foreach (var file in sheet.Files)
        {
            if (File.Exists(file.FilePath))
                File.Delete(file.FilePath);
        }

        _dbContext.MusicSheetFiles.RemoveRange(sheet.Files);
        _dbContext.MusicSheets.Remove(sheet);
        _dbContext.SaveChanges();

        return true;
    }

    private static byte[] BuildViewerPdfContent(string[] filePaths, int? fromPage = null, int? toPage = null)
    {
        byte[] fullPdfBytes = BuildMergedPdf(filePaths);

        if (fromPage == null && toPage == null)
            return fullPdfBytes;

        using MemoryStream input = new MemoryStream(fullPdfBytes);
        using PdfLoadedDocument loadedDocument = new PdfLoadedDocument(input);

        int totalPages = loadedDocument.Pages.Count;
        int startPage = fromPage ?? 1;
        int endPage = toPage ?? totalPages;

        if (startPage < 1 || endPage < 1 || startPage > endPage || endPage > totalPages)
            throw new ArgumentOutOfRangeException($"Ungültiger Seitenbereich {startPage}-{endPage} für PDF mit {totalPages} Seiten.");

        using PdfDocument rangeDocument = new PdfDocument();

        for (int pageNumber = startPage; pageNumber <= endPage; pageNumber++)
        {
            rangeDocument.ImportPage(loadedDocument, pageNumber - 1);
        }

        using MemoryStream output = new MemoryStream();
        rangeDocument.Save(output);
        return output.ToArray();
    }

    private static byte[] BuildMergedPdf(string[] filePaths)
    {
        if (filePaths.Length == 1)
            return File.ReadAllBytes(filePaths[0]);

        List<FileStream> streams = new List<FileStream>();
        List<PdfLoadedDocument> loadedDocuments = new List<PdfLoadedDocument>();

        try
        {
            PdfDocument mergedDocument = new PdfDocument();

            foreach (string filePath in filePaths)
            {
                FileStream input = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                streams.Add(input);

                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(input);
                loadedDocuments.Add(loadedDocument);

                for (int pageIndex = 0; pageIndex < loadedDocument.Pages.Count; pageIndex++)
                {
                    mergedDocument.ImportPage(loadedDocument, pageIndex);
                }
            }

            using MemoryStream output = new MemoryStream();
            mergedDocument.Save(output);
            mergedDocument.Close(true);

            return output.ToArray();
        }
        finally
        {
            foreach (PdfLoadedDocument loadedDocument in loadedDocuments)
                loadedDocument.Close(true);

            foreach (FileStream stream in streams)
                stream.Dispose();
        }
    }

    private static (string FileHash, int Filesize, int PageCount) ReadStoredFilesMetadata(IEnumerable<MusicSheetFile> files)
    {
        var orderedFiles = files
            .OrderBy(x => x.SortOrder)
            .ToArray();

        using (SHA256 sha = SHA256.Create())
        {
            int totalSize = 0;
            int totalPages = 0;

            foreach (var file in orderedFiles)
            {
                byte[] bytes = File.ReadAllBytes(file.FilePath);
                sha.TransformBlock(bytes, 0, bytes.Length, null, 0);

                totalSize += file.Filesize;
                totalPages += file.PageCount;
            }

            sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            string hash = Convert.ToHexString(sha.Hash!);

            return (hash, totalSize, totalPages);
        }
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

            float pageWidth = page.GetClientSize().Width;
            float pageHeight = page.GetClientSize().Height;

            float imageWidth = image.Width;
            float imageHeight = image.Height;

            float cropLeftPercent = 0.01f;
            float cropRightPercent = 0.01f;
            float cropTopPercent = 0.04f;
            float cropBottomPercent = 0.06f;

            float scaleX = pageWidth / imageWidth;
            float scaleY = pageHeight / imageHeight;
            float scale = Math.Min(scaleX, scaleY);

            float drawWidth = imageWidth * scale;
            float drawHeight = imageHeight * scale;

            float offsetX = (pageWidth - drawWidth) / 2f - (imageWidth * cropLeftPercent * scale);
            float offsetY = (pageHeight - drawHeight) / 2f - (imageHeight * cropTopPercent * scale);

            page.Graphics.DrawImage(image, offsetX, offsetY, drawWidth, drawHeight);

            using (FileStream output = new FileStream(targetPdfPath, FileMode.Create))
            {
                document.Save(output);
            }
        }
    }
}
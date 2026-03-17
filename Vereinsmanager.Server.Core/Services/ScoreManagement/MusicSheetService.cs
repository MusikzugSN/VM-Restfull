using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
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
        IQueryable<MusicSheet> query = _dbContext.MusicSheets;

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
            .Where(x => scoreIds.Contains(x.ScoreId));

        if (voiceIds.Length > 0)
            query = query.Where(x => voiceIds.Contains(x.VoiceId));

        return query.ToArray();
    }

    public ReturnValue<MusicSheet> CreateMusicSheet(CreateMusicSheetRequestDto request)
    {
        var score = _dbContext.Scores.FirstOrDefault(x => x.ScoreId == request.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), request.ScoreId.ToString());

        var voice = _dbContext.Voices.FirstOrDefault(x => x.VoiceId == request.VoiceId);
        if (voice == null)
            return ErrorUtils.ValueNotFound(nameof(Voice), request.VoiceId.ToString());

        string basePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "Scores");
        Directory.CreateDirectory(basePath);

        string scoreFolder = Path.Combine(basePath, request.ScoreId.ToString());
        Directory.CreateDirectory(scoreFolder);

        string fileId = Guid.NewGuid().ToString("N");
        string filePath = Path.Combine(scoreFolder, fileId + ".pdf");

        SavePdfOrConvertImageToPdf(request.File!, filePath);

        var metadata = ReadPdfMetadata(filePath);

        MusicSheet sheet = new MusicSheet
        {
            ScoreId = request.ScoreId,
            VoiceId = request.VoiceId,
            Score = score,
            Voice = voice,
            FilePath = filePath,
            FileHash = metadata.FileHash,
            Filesize = (int)new FileInfo(filePath).Length,
            PageCount = metadata.PageCount,
            FileModifiedDate = DateTime.UtcNow,
            Status = MusicSheetStatus.Ungeprueft
        };

        _dbContext.MusicSheets.Add(sheet);
        _dbContext.SaveChanges();

        return sheet;
    }

    public ReturnValue<MusicSheet> UpdateMusicSheet(int id, UpdateMusicSheet update)
    {
        var sheet = _dbContext.MusicSheets.FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        if (update.ScoreId != null)
            sheet.ScoreId = update.ScoreId.Value;

        if (update.VoiceId != null)
            sheet.VoiceId = update.VoiceId.Value;

        if (update.Status != null)
            sheet.Status = update.Status.Value;

        _dbContext.SaveChanges();

        return sheet;
    }

    public ReturnValue<MusicSheet> UpdateMusicSheetPdf(int id, IFormFile file)
    {
        var sheet = _dbContext.MusicSheets.FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        string scoreFolder = Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Data",
            "Scores",
            sheet.ScoreId.ToString());

        Directory.CreateDirectory(scoreFolder);

        string fileId = Guid.NewGuid().ToString("N");
        string filePath = Path.Combine(scoreFolder, fileId + ".pdf");

        SavePdfOrConvertImageToPdf(file, filePath);

        var metadata = ReadPdfMetadata(filePath);

        sheet.FilePath = filePath;
        sheet.FileHash = metadata.FileHash;
        sheet.Filesize = (int)new FileInfo(filePath).Length;
        sheet.PageCount = metadata.PageCount;
        sheet.FileModifiedDate = DateTime.UtcNow;

        _dbContext.SaveChanges();

        return sheet;
    }

    public ReturnValue<bool> DeleteMusicSheet(int id)
    {
        var sheet = _dbContext.MusicSheets.FirstOrDefault(x => x.MusicSheetId == id);

        if (sheet == null)
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), id.ToString());

        if (File.Exists(sheet.FilePath))
            File.Delete(sheet.FilePath);

        _dbContext.MusicSheets.Remove(sheet);
        _dbContext.SaveChanges();

        return true;
    }

    private static (string FileHash, int PageCount) ReadPdfMetadata(string filePath)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            using (SHA256 sha = SHA256.Create())
            {
                string hash = Convert.ToHexString(sha.ComputeHash(stream));

                stream.Position = 0;

                using (PdfLoadedDocument document = new PdfLoadedDocument(stream))
                {
                    return (hash, document.Pages.Count);
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

    private static void SavePdfOrConvertImageToPdf(IFormFile sourceFile, string targetPdfPath)
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

            PdfPage page = document.Pages.Add();
            page.Section.PageSettings.Size = PdfPageSize.A4;

            if (image.Width > image.Height)
                page.Section.PageSettings.Orientation = PdfPageOrientation.Landscape;
            else
                page.Section.PageSettings.Orientation = PdfPageOrientation.Portrait;

            float pageWidth = page.GetClientSize().Width;
            float pageHeight = page.GetClientSize().Height;

            float imageWidth = image.Width;
            float imageHeight = image.Height;

            float scaleX = pageWidth / imageWidth;
            float scaleY = pageHeight / imageHeight;
            float scale = Math.Min(scaleX, scaleY);

            float drawWidth = imageWidth * scale;
            float drawHeight = imageHeight * scale;

            float x = (pageWidth - drawWidth) / 2f;
            float y = (pageHeight - drawHeight) / 2f;

            page.Graphics.DrawImage(image, x, y, drawWidth, drawHeight);

            using (FileStream output = new FileStream(targetPdfPath, FileMode.Create))
            {
                document.Save(output);
            }
        }
    }
}
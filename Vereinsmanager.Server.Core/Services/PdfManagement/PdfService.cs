using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Syncfusion.Pdf.Parsing;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.PdfManagement;

public class PdfService : IPdfService
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ServerDatabaseContext _dbContext;

    public PdfService(IWebHostEnvironment hostingEnvironment, ServerDatabaseContext dbContext)
    {
        _hostingEnvironment = hostingEnvironment;
        _dbContext = dbContext;
    }

    public ReturnValue<UploadPdfsResponseDto> UploadPdfs(UploadPdfsRequestDto request)
    {
        var duplicateVoiceIds = request.Files
            .GroupBy(file => file.VoiceId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateVoiceIds.Length > 0)
        {
            return ErrorUtils.AlreadyExists(
                nameof(MusicSheet),
                $"mehrfache VoiceIds im Upload: {string.Join(", ", duplicateVoiceIds)}"
            );
        }

        string baseFolderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "Scores");
        Directory.CreateDirectory(baseFolderPath);

        string scoreFolderPath = Path.Combine(baseFolderPath, request.ScoreId.ToString());
        Directory.CreateDirectory(scoreFolderPath);

        Score? score = _dbContext.Scores
            .FirstOrDefault(s => s.ScoreId == request.ScoreId);

        if (score == null)
        {
            return ErrorUtils.ValueNotFound(nameof(Score), request.ScoreId.ToString());
        }

        UploadPdfsResponseDto response = new UploadPdfsResponseDto
        {
            ScoreId = request.ScoreId
        };

        foreach (UploadPdfFileRequestDto fileDto in request.Files)
        {
            if (fileDto.File == null)
            {
                return ErrorUtils.ValueNotFound(nameof(File), fileDto.FileName);
            }

            Voice? voice = _dbContext.Voices
                .FirstOrDefault(v => v.VoiceId == fileDto.VoiceId);

            if (voice == null)
            {
                return ErrorUtils.ValueNotFound(nameof(Voice), fileDto.VoiceId.ToString());
            }

            MusicSheet? existingMusicSheet = _dbContext.MusicSheets
                .FirstOrDefault(ms => ms.ScoreId == request.ScoreId && ms.VoiceId == fileDto.VoiceId);

            if (existingMusicSheet != null)
            {
                return ErrorUtils.AlreadyExists(
                    nameof(MusicSheet),
                    $"ScoreId {request.ScoreId}, VoiceId {fileDto.VoiceId}"
                );
            }

            string fileId = Guid.NewGuid().ToString("N");
            string extension = Path.GetExtension(fileDto.File.FileName);

            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".pdf";
            }

            string storedFileName = fileId + extension;
            string filePath = Path.Combine(scoreFolderPath, storedFileName);

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fileDto.File.CopyTo(fileStream);
                }

                (string fileHash, int pageCount) = ReadPdfMetadata(filePath);

                MusicSheet musicSheet = new MusicSheet
                {
                    ScoreId = request.ScoreId,
                    Score = score,
                    VoiceId = fileDto.VoiceId,
                    Voice = voice,
                    FilePath = filePath,
                    FileHash = fileHash,
                    Filesize = (int)fileDto.File.Length,
                    PageCount = pageCount,
                    FileModifiedDate = DateTime.UtcNow
                };

                _dbContext.MusicSheets.Add(musicSheet);
                _dbContext.SaveChanges();

                response.Files.Add(new UploadPdfFileResponseDto
                {
                    FileName = fileDto.FileName,
                    FileId = fileId,
                    VoiceId = fileDto.VoiceId,
                    MusicSheetId = musicSheet.MusicSheetId
                });
            }
            catch (DbUpdateException)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return ErrorUtils.AlreadyExists(
                    nameof(MusicSheet),
                    $"ScoreId {request.ScoreId}, VoiceId {fileDto.VoiceId}"
                );
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

        return response;
    }

    public string GetPdfPath(int scoreId, string fileId)
    {
        string scoreFolderPath = Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Data",
            "Scores",
            scoreId.ToString()
        );

        if (!Directory.Exists(scoreFolderPath))
        {
            return string.Empty;
        }

        string[] matchingFiles = Directory.GetFiles(scoreFolderPath, fileId + ".*");

        if (matchingFiles.Length == 0)
        {
            return string.Empty;
        }

        return matchingFiles[0];
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
}
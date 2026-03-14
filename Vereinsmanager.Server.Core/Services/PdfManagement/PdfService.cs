using System.Text.Json;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Vereinsmanager.Controllers.DataTransferObjects;

namespace Vereinsmanager.Services.PdfManagement;

public class PdfService
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public PdfService(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    public PdfUploadResponseDto CreateUpload(PdfUploadRequestDto request)
    {
        if (request == null)
            throw new Exception("Request is null");

        if (request.Files == null || request.Files.Count == 0)
            throw new Exception("No files provided");

        List<PdfUploadResponseFileDto> responseFiles = new();

        foreach (PdfUploadFileDto file in request.Files)
        {
            string guid = Guid.NewGuid().ToString("N");

            StoredPdfMetadata metadata = new StoredPdfMetadata(
                guid,
                request.ScoreId,
                file.VoiceId,
                file.FileName,
                null,
                false
            );

            SaveMetadata(metadata);

            responseFiles.Add(new PdfUploadResponseFileDto(file.FileName, guid));
        }

        return new PdfUploadResponseDto(responseFiles);
    }

    public async Task<PdfUploadResponseDto> UploadRegisteredFiles(IFormCollection form)
    {
        List<PdfUploadResponseFileDto> uploadedFiles = new();

        foreach (IFormFile file in form.Files)
        {
            if (file == null || file.Length == 0)
                continue;

            string guid = file.Name;

            StoredPdfMetadata metadata = LoadMetadata(guid)
                ?? throw new FileNotFoundException($"No metadata found for guid '{guid}'.");

            string originalsFolder = Path.Combine(
                _hostingEnvironment.ContentRootPath,
                "Storage",
                "OriginalFiles"
            );

            Directory.CreateDirectory(originalsFolder);

            string extension = Path.GetExtension(metadata.FileName);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".pdf";

            string path = Path.Combine(originalsFolder, $"{guid}{extension}");

            await using FileStream stream = new(path, FileMode.Create);
            await file.CopyToAsync(stream);

            StoredPdfMetadata updatedMetadata = metadata with
            {
                StoredPath = path,
                IsUploaded = true
            };

            SaveMetadata(updatedMetadata);

            uploadedFiles.Add(new PdfUploadResponseFileDto(metadata.FileName, guid));
        }

        return new PdfUploadResponseDto(uploadedFiles);
    }

    public string CreatePdf(PdfLayoutDto layout)
    {
        string sourcePath = GetPdfPath(layout.SourceGuid);

        if (!System.IO.File.Exists(sourcePath))
            throw new FileNotFoundException("Source PDF not found.");

        using PdfLoadedDocument source = new(sourcePath);
        using PdfDocument target = new();

        SizeF pageSize = GetPageSize(layout.TargetFormat);

        for (int i = 0; i < layout.TargetPageCount; i++)
        {
            PdfPage page = target.Pages.Add();
            page.Section.PageSettings.Size = pageSize;
        }

        foreach (PdfPagePlacementDto placement in layout.Placements)
        {
            PdfLoadedPage sourcePage = (PdfLoadedPage)source.Pages[placement.SourcePageIndex];
            PdfTemplate template = sourcePage.CreateTemplate();

            PdfPage targetPage = target.Pages[placement.TargetPageIndex];
            SizeF targetPageSize = targetPage.GetClientSize();

            RectangleF rect = BuildRectangle(
                placement,
                targetPageSize.Width,
                targetPageSize.Height
            );

            DrawTemplate(targetPage, template, rect, placement.KeepAspectRatio);
        }

        string folder = Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Storage",
            "GeneratedFiles"
        );

        Directory.CreateDirectory(folder);

        string outputPath = Path.Combine(folder, $"{Guid.NewGuid():N}.pdf");

        using FileStream stream = new(outputPath, FileMode.Create);
        target.Save(stream);

        return outputPath;
    }

    public string GetPdfPath(string guid)
    {
        StoredPdfMetadata metadata = LoadMetadata(guid)
            ?? throw new FileNotFoundException($"No metadata found for guid '{guid}'.");

        if (string.IsNullOrWhiteSpace(metadata.StoredPath))
            throw new FileNotFoundException($"No uploaded file found for guid '{guid}'.");

        return metadata.StoredPath;
    }

    private RectangleF BuildRectangle(PdfPagePlacementDto placement, float pageWidth, float pageHeight)
    {
        if (placement.IsNormalized)
        {
            return new RectangleF(
                placement.X * pageWidth,
                placement.Y * pageHeight,
                placement.Width * pageWidth,
                placement.Height * pageHeight
            );
        }

        return new RectangleF(
            placement.X,
            placement.Y,
            placement.Width,
            placement.Height
        );
    }

    private SizeF GetPageSize(string format)
    {
        return (format ?? "A4").ToUpperInvariant() switch
        {
            "A3" => PdfPageSize.A3,
            "A4" => PdfPageSize.A4,
            "A5" => PdfPageSize.A5,
            _ => PdfPageSize.A4
        };
    }

    private void DrawTemplate(
        PdfPage page,
        PdfTemplate template,
        RectangleF rect,
        bool keepAspectRatio
    )
    {
        PdfGraphics graphics = page.Graphics;

        float sourceWidth = template.Width;
        float sourceHeight = template.Height;

        if (!keepAspectRatio)
        {
            graphics.DrawPdfTemplate(
                template,
                new PointF(rect.X, rect.Y),
                new SizeF(rect.Width, rect.Height)
            );
            return;
        }

        float scaleX = rect.Width / sourceWidth;
        float scaleY = rect.Height / sourceHeight;
        float scale = Math.Min(scaleX, scaleY);

        float drawWidth = sourceWidth * scale;
        float drawHeight = sourceHeight * scale;

        float drawX = rect.X + (rect.Width - drawWidth) / 2f;
        float drawY = rect.Y + (rect.Height - drawHeight) / 2f;

        graphics.DrawPdfTemplate(
            template,
            new PointF(drawX, drawY),
            new SizeF(drawWidth, drawHeight)
        );
    }

    private void SaveMetadata(StoredPdfMetadata metadata)
    {
        string metadataFolder = Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Storage",
            "Metadata"
        );

        Directory.CreateDirectory(metadataFolder);

        string path = Path.Combine(metadataFolder, $"{metadata.Guid}.json");

        string json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        System.IO.File.WriteAllText(path, json);
    }

    private StoredPdfMetadata? LoadMetadata(string guid)
    {
        string metadataFolder = Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Storage",
            "Metadata"
        );

        string path = Path.Combine(metadataFolder, $"{guid}.json");

        if (!System.IO.File.Exists(path))
            return null;

        string json = System.IO.File.ReadAllText(path);
        return JsonSerializer.Deserialize<StoredPdfMetadata>(json);
    }

    private record StoredPdfMetadata(
        string Guid,
        int ScoreId,
        int VoiceId,
        string FileName,
        string? StoredPath,
        bool IsUploaded
    );
}
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

    public async Task<UploadPdfDto> UploadPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new Exception("File empty");

        string folder = Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Storage",
            "OriginalFiles"
        );

        Directory.CreateDirectory(folder);

        string fileId = Guid.NewGuid().ToString("N");
        string path = Path.Combine(folder, $"{fileId}.pdf");

        await using FileStream stream = new(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return new UploadPdfDto(fileId, file.FileName, file.Length);
    }

    public string CreatePdf(PdfLayoutDto layout)
    {
        string sourcePath = GetPdfPath(layout.SourceFileId);

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

    public string GetPdfPath(string fileId)
    {
        return Path.Combine(
            _hostingEnvironment.ContentRootPath,
            "Storage",
            "OriginalFiles",
            $"{fileId}.pdf"
        );
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
}
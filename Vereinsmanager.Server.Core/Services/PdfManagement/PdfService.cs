using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Drawing;
using Vereinsmanager.DataTransferObjects;

namespace Vereinsmanager.Services.PdfManagement;

public class PdfService
{
    private readonly IWebHostEnvironment _environment;

    public PdfService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<UploadPdfDto> UploadPdf(IFormFile file)
    {
        if (file.Length == 0)
            throw new Exception("File empty");

        string folder = Path.Combine(_environment.ContentRootPath, "Storage", "OriginalFiles");
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

        using PdfLoadedDocument source = new(sourcePath);
        using PdfDocument target = new();

        SizeF pageSize = GetPageSize(layout.TargetFormat);

        for (int i = 0; i < layout.TargetPageCount; i++)
        {
            PdfPage page = target.Pages.Add();
            page.Section.PageSettings.Size = pageSize;
        }

        foreach (var placement in layout.Placements)
        {
            PdfLoadedPage sourcePage = (PdfLoadedPage)source.Pages[placement.SourcePageIndex];
            PdfTemplate template = sourcePage.CreateTemplate();

            PdfPage targetPage = target.Pages[placement.TargetPageIndex];

            SizeF size = targetPage.GetClientSize();

            RectangleF rect = BuildRectangle(placement, size.Width, size.Height);

            DrawTemplate(targetPage, template, rect, placement.KeepAspectRatio);
        }

        string folder = Path.Combine(_environment.ContentRootPath, "Storage", "GeneratedFiles");
        Directory.CreateDirectory(folder);

        string output = Path.Combine(folder, $"{Guid.NewGuid():N}.pdf");

        using FileStream fs = new(output, FileMode.Create);
        target.Save(fs);

        return output;
    }

    public string GetPdfPath(string fileId)
    {
        return Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "OriginalFiles",
            $"{fileId}.pdf"
        );
    }

    private RectangleF BuildRectangle(PdfPagePlacementDto p, float pageWidth, float pageHeight)
    {
        if (p.IsNormalized)
        {
            return new RectangleF(
                p.X * pageWidth,
                p.Y * pageHeight,
                p.Width * pageWidth,
                p.Height * pageHeight
            );
        }

        return new RectangleF(p.X, p.Y, p.Width, p.Height);
    }

    private SizeF GetPageSize(string format)
    {
        return format.ToUpper() switch
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
        bool keepAspectRatio)
    {
        PdfGraphics g = page.Graphics;

        float sw = template.Width;
        float sh = template.Height;

        if (!keepAspectRatio)
        {
            g.DrawPdfTemplate(template,
                new PointF(rect.X, rect.Y),
                new SizeF(rect.Width, rect.Height));
            return;
        }

        float scaleX = rect.Width / sw;
        float scaleY = rect.Height / sh;
        float scale = Math.Min(scaleX, scaleY);

        float width = sw * scale;
        float height = sh * scale;

        float x = rect.X + (rect.Width - width) / 2;
        float y = rect.Y + (rect.Height - height) / 2;

        g.DrawPdfTemplate(template,
            new PointF(x, y),
            new SizeF(width, height));
    }
}
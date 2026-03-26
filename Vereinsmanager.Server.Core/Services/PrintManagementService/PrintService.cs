using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.PrintManagementService;

public class PrintService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public PrintService(ServerDatabaseContext dbContext, IMemoryCache memoryCache, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
        _hostingEnvironment = environment;
    }

    public ReturnValue<string> CreatePrintUrl(int[] musicSheetIds, bool marschbuch)
    {
        if (musicSheetIds.Length == 0)
            return ErrorUtils.ValueNotFound("MusicSheetIds", "Keine MusicSheetIds übergeben.");

        var sheets = _dbContext.MusicSheets
            .Where(x => musicSheetIds.Contains(x.MusicSheetId))
            .ToList();

        if (sheets.Count != musicSheetIds.Length)
        {
            var foundIds = sheets.Select(x => x.MusicSheetId).ToHashSet();
            var missingIds = musicSheetIds.Where(id => !foundIds.Contains(id));

            return ErrorUtils.ValueNotFound(
                nameof(MusicSheet),
                $"Nicht gefunden: {string.Join(", ", missingIds)}");
        }
        

        byte[] pdfBytes = BuildPrintPdf(sheets, marschbuch);

        string token = Guid.NewGuid().ToString("N");

        _memoryCache.Set(
            GetCacheKey(token),
            pdfBytes,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

        return $"/api/v1/print/download?token={token}";
    }

    public ReturnValue<byte[]> GetPrintPdfByToken(string token)
    {
        if (!_memoryCache.TryGetValue(GetCacheKey(token), out byte[]? pdfBytes) || pdfBytes == null)
            return ErrorUtils.ValueNotFound("PrintToken", "Ungültiger oder abgelaufener Token.");

        _memoryCache.Remove(GetCacheKey(token));

        return pdfBytes;
    }

    private static string GetCacheKey(string token)
    {
        return $"print-pdf:{token}";
    }

    private byte[] BuildPrintPdf(List<MusicSheet> sheets, bool marschbuch)
    {
        using (PdfDocument outputDocument = new PdfDocument())
        {
            if (!marschbuch)
            {
                AppendNormalSheets(outputDocument, sheets);
            }
            else
            {
                AppendMarschbuchSheets(outputDocument, sheets);
            }

            using (MemoryStream output = new MemoryStream())
            {
                outputDocument.Save(output);
                return output.ToArray();
            }
        }
    }

    private void AppendNormalSheets(PdfDocument outputDocument, List<MusicSheet> sheets)
    {
        foreach (var sheet in sheets)
        {
            string scoreFolder = Path.Combine(
                _hostingEnvironment.ContentRootPath,
                "Data",
                "Scores",
                sheet.ScoreId.ToString());
            
            var filePath = Path.Combine(scoreFolder, sheet.FileName);
            
            if (!File.Exists(filePath))
                throw new InvalidOperationException($"Datei nicht gefunden: {filePath}");

            AppendAllPagesFromPdf(outputDocument, filePath);
        }
    }

    private void AppendMarschbuchSheets(PdfDocument outputDocument, List<MusicSheet> sheets)
    {
        var collectedPages = new List<PdfPageSource>();

        foreach (var sheet in sheets)
        {
            string scoreFolder = Path.Combine(
                _hostingEnvironment.ContentRootPath,
                "Data",
                "Scores",
                sheet.ScoreId.ToString());
            
            var filePath = Path.Combine(scoreFolder, sheet.FileName);
            
            if (!File.Exists(filePath))
                throw new InvalidOperationException($"Datei nicht gefunden: {filePath}");

            collectedPages.AddRange(ReadAllPdfPages(filePath));
        }

        AppendCollectedPagesAsMarschbuch(outputDocument, collectedPages);
    }

    private static List<PdfPageSource> ReadAllPdfPages(string filePath)
    {
        var result = new List<PdfPageSource>();

        using (FileStream input = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument(input))
        {
            for (int i = 0; i < loadedDocument.Pages.Count; i++)
            {
                PdfLoadedPage sourcePage = (PdfLoadedPage)loadedDocument.Pages[i];
                PdfTemplate template = sourcePage.CreateTemplate();

                result.Add(new PdfPageSource
                {
                    Template = template
                });
            }
        }

        return result;
    }

    private static void AppendCollectedPagesAsMarschbuch(PdfDocument outputDocument, List<PdfPageSource> pages)
    {
        int pageIndex = 0;

        while (pageIndex < pages.Count)
        {
            outputDocument.PageSettings.Size = PdfPageSize.A4;
            outputDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;

            PdfPage outputPage = outputDocument.Pages.Add();

            float pageWidth = outputPage.GetClientSize().Width;
            float pageHeight = outputPage.GetClientSize().Height;

            float margin = 0f;
            float gap = 5f;

            float availableWidth = pageWidth - (2 * margin) - gap;
            float slotWidth = availableWidth / 2f;
            float slotHeight = pageHeight - (2 * margin);

            DrawTemplateIntoSlot(outputPage, pages[pageIndex].Template, margin, margin, slotWidth, slotHeight);

            if (pageIndex + 1 < pages.Count)
            {
                DrawTemplateIntoSlot(
                    outputPage,
                    pages[pageIndex + 1].Template,
                    margin + slotWidth + gap,
                    margin,
                    slotWidth,
                    slotHeight);
            }

            pageIndex += 2;
        }
    }

    private static void AppendAllPagesFromPdf(PdfDocument outputDocument, string filePath)
    {
        using (FileStream input = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument(input))
        {
            for (int i = 0; i < loadedDocument.Pages.Count; i++)
            {
                PdfLoadedPage sourcePage = (PdfLoadedPage)loadedDocument.Pages[i];
                PdfTemplate template = sourcePage.CreateTemplate();

                // Daten des Original PDF möglichst genau üernehmen
                // Orientierung
                bool isLandscape = sourcePage.Size.Width > sourcePage.Size.Height;
                PdfSection section = outputDocument.Sections.Add();
                section.PageSettings.Orientation =
                    isLandscape ? PdfPageOrientation.Landscape : PdfPageOrientation.Portrait;

                // Rotation
                section.PageSettings.Rotate = sourcePage.Rotation;
                
                section.PageSettings.Margins.All = 0;

                PdfPage outputPage = section.Pages.Add();

                float pageWidth = outputPage.GetClientSize().Width;
                float pageHeight = outputPage.GetClientSize().Height;

                DrawTemplateIntoSlot(outputPage, template, 0f, 0f, pageWidth, pageHeight);
            }
        }
    }

    private static void DrawTemplateIntoSlot(
        PdfPage targetPage,
        PdfTemplate template,
        float x,
        float y,
        float maxWidth,
        float maxHeight)
    {
        float originalWidth = template.Size.Width;
        float originalHeight = template.Size.Height;

        float scale = Math.Min(maxWidth / originalWidth, maxHeight / originalHeight);

        float drawWidth = originalWidth * scale;
        float drawHeight = originalHeight * scale;

        float drawX = x + (maxWidth - drawWidth) / 2f;
        float drawY = y + (maxHeight - drawHeight) / 2f;

        targetPage.Graphics.DrawPdfTemplate(
            template,
            new PointF(drawX, drawY),
            new SizeF(drawWidth, drawHeight));
    }

    private sealed class PdfPageSource
    {
        public required PdfTemplate Template { get; init; }
    }
}
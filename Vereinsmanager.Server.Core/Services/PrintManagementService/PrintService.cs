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

    public PrintService(ServerDatabaseContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public ReturnValue<string> CreatePrintUrl(int[] musicSheetIds, bool marschbuch)
    {
        if (musicSheetIds == null || musicSheetIds.Length == 0)
            return ErrorUtils.ValueNotFound("MusicSheetIds", "Keine MusicSheetIds übergeben.");

        var sheets = _dbContext.MusicSheets
            .Include(x => x.Files)
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

        var orderedSheets = musicSheetIds
            .Select(id => sheets.First(x => x.MusicSheetId == id))
            .ToList();

        byte[] pdfBytes = BuildPrintPdf(orderedSheets, marschbuch);

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

    private static byte[] BuildPrintPdf(List<MusicSheet> sheets, bool marschbuch)
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

    private static void AppendNormalSheets(PdfDocument outputDocument, List<MusicSheet> sheets)
    {
        foreach (var sheet in sheets)
        {
            foreach (var file in sheet.Files.OrderBy(x => x.SortOrder))
            {
                if (!File.Exists(file.FilePath))
                    throw new InvalidOperationException($"Datei nicht gefunden: {file.FilePath}");

                AppendAllPagesFromPdf(outputDocument, file.FilePath);
            }
        }
    }

    private static void AppendMarschbuchSheets(PdfDocument outputDocument, List<MusicSheet> sheets)
    {
        var collectedPages = new List<PdfPageSource>();

        foreach (var sheet in sheets)
        {
            foreach (var file in sheet.Files.OrderBy(x => x.SortOrder))
            {
                if (!File.Exists(file.FilePath))
                    throw new InvalidOperationException($"Datei nicht gefunden: {file.FilePath}");

                collectedPages.AddRange(ReadAllPdfPages(file.FilePath));
            }
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

                PdfPage outputPage = outputDocument.Pages.Add();

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
using System.IO.Compression;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.PrintManagementService;

public class PrintService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger<PrintService> _logger;

    public PrintService(ServerDatabaseContext dbContext, IWebHostEnvironment environment, ILogger<PrintService> logger)
    {
        _dbContext = dbContext;
        _hostingEnvironment = environment;
        _logger = logger;
    }

    public ReturnValue<List<string>> CreatePrintUrl(int[] musicSheetIds, bool marschbuch)
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


        // Für jede MusicSheetId erzeugen wir ein eigenes Token und liefern pro Datei eine URL zurück.
        var urls = new List<string>();

        foreach (var sheet in sheets)
        {
            var token = Guid.NewGuid().ToString("N");
            var pdfPath = GetTempFilePath(token);

            // Erzeuge eine PDF, die genau dieses eine MusicSheet enthält
            var pdfBytes = BuildPrintPdf(new List<MusicSheet> { sheet }, marschbuch);
            File.WriteAllBytes(pdfPath, pdfBytes);

            urls.Add($"/api/v1/print/download?token={token}");
        }

        return urls;
    }

    public ReturnValue<byte[]> GetDownloadBytesByToken(string token, out string contentType)
    {
        var zipPath = GetTempZipPath(token);
        var pdfPath = GetTempFilePath(token);

        if (File.Exists(zipPath)) return TryReadAndDeleteFile(zipPath, "application/zip", token, out contentType);

        if (File.Exists(pdfPath)) return TryReadAndDeleteFile(pdfPath, "application/pdf", token, out contentType);

        contentType = "application/json";
        return ErrorUtils.ValueNotFound("PrintToken", "Ungültiger oder abgelaufener Token.");
    }

    private ReturnValue<byte[]> TryReadAndDeleteFile(string path, string mimeType, string token, out string contentType)
    {
        contentType = mimeType;
        try
        {
            var bytes = File.ReadAllBytes(path);

            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary file '{FilePath}'", path);
            }

            return bytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read/delete file '{Path}' for token '{Token}'", path, token);
            return ErrorUtils.ValueNotFound("PrintToken", $"Fehler beim Lesen der Datei {Path.GetFileName(path)}.");
        }
    }

    // Dateien werden nun als <ContentRoot>/data/temp/{token}.pdf abgelegt.

    /// <summary>
    ///     Liefert den vollständigen Dateipfad für einen Temp-Print-Token und stellt sicher,
    ///     dass das Temp-Verzeichnis existiert.
    /// </summary>
    public ReturnValue<string> CreateDownloadUrl(int[] musicSheetIds, bool asZip, bool marschbuch)
    {
        if (musicSheetIds.Length == 0)
            return ErrorUtils.ValueNotFound("MusicSheetIds", "Keine MusicSheetIds übergeben.");

        var sheetsResult = LoadSheets(musicSheetIds);
        if (!sheetsResult.IsSuccessful())
            return sheetsResult.GetProblemDetails()!;

        var sheets = sheetsResult.GetValue()!;

        var token = Guid.NewGuid().ToString("N");

        if (!asZip)
        {
            var saveResult = SaveCombinedPdf(token, sheets, marschbuch);
            if (!saveResult.IsSuccessful())
                return saveResult.GetProblemDetails()!;

            return $"/api/v1/print/download?token={token}";
        }

        var zipResult = CreateZipForSheets(token, sheets);
        if (!zipResult.IsSuccessful())
            return zipResult.GetProblemDetails()!;

        return $"/api/v1/print/download?token={token}";
    }

    private ReturnValue<List<MusicSheet>> LoadSheets(int[] musicSheetIds)
    {
        var sheets = _dbContext.MusicSheets
            .Where(x => musicSheetIds.Contains(x.MusicSheetId))
            .ToList();

        if (sheets.Count != musicSheetIds.Length)
        {
            var foundIds = sheets.Select(x => x.MusicSheetId).ToHashSet();
            var missingIds = musicSheetIds.Where(id => !foundIds.Contains(id));
            return ErrorUtils.ValueNotFound(nameof(MusicSheet), $"Nicht gefunden: {string.Join(", ", missingIds)}");
        }

        return sheets;
    }

    private ReturnValue<string> SaveCombinedPdf(string token, List<MusicSheet> sheets, bool marschbuch)
    {
        try
        {
            var pdfBytes = BuildPrintPdf(sheets, marschbuch);
            var pdfPath = GetTempFilePath(token);
            File.WriteAllBytes(pdfPath, pdfBytes);
            return pdfPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create combined PDF for token '{Token}'", token);
            return ErrorUtils.ValueNotFound("PdfCreation", "Fehler beim Erstellen der kombinierten PDF.");
        }
    }

    private ReturnValue<string> CreateZipForSheets(string token, List<MusicSheet> sheets)
    {
        try
        {
            var zipPath = GetTempZipPath(token);

            using (var zipToOpen = new FileStream(zipPath, FileMode.Create))
            using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                foreach (var sheet in sheets)
                {
                    var scoreFolder = Path.Combine(
                        _hostingEnvironment.ContentRootPath,
                        "data",
                        "scores",
                        sheet.ScoreId.ToString());

                    var sourcePath = Path.Combine(scoreFolder, sheet.FileName);
                    if (!File.Exists(sourcePath))
                        throw new InvalidOperationException($"Datei nicht gefunden: {sourcePath}");

                    var entryName = $"{sheet.MusicSheetId}_{Path.GetFileName(sourcePath)}";
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    using (var fs = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                    {
                        fs.CopyTo(entryStream);
                    }
                }
            }

            return zipPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ZIP for token '{Token}'", token);
            return ErrorUtils.ValueNotFound("ZipCreation", "Fehler beim Erstellen der ZIP-Datei.");
        }
    }

    private string GetTempFilePath(string token)
    {
        var tempDir = Path.Combine(_hostingEnvironment.ContentRootPath, "data", "temp");
        Directory.CreateDirectory(tempDir);
        return Path.Combine(tempDir, $"{token}.pdf");
    }

    private string GetTempZipPath(string token)
    {
        var tempDir = Path.Combine(_hostingEnvironment.ContentRootPath, "data", "temp");
        Directory.CreateDirectory(tempDir);
        return Path.Combine(tempDir, $"{token}.zip");
    }

    private byte[] BuildPrintPdf(List<MusicSheet> sheets, bool marschbuch)
    {
        using (var outputDocument = new PdfDocument())
        {
            if (!marschbuch)
                AppendNormalSheets(outputDocument, sheets);
            else
                AppendMarschbuchSheets(outputDocument, sheets);

            using (var output = new MemoryStream())
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
            var scoreFolder = Path.Combine(
                _hostingEnvironment.ContentRootPath,
                "data",
                "scores",
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
            var scoreFolder = Path.Combine(
                _hostingEnvironment.ContentRootPath,
                "data",
                "scores",
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

        using (var input = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var loadedDocument = new PdfLoadedDocument(input))
        {
            for (var i = 0; i < loadedDocument.Pages.Count; i++)
            {
                var sourcePage = (PdfLoadedPage)loadedDocument.Pages[i];
                var template = sourcePage.CreateTemplate();

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
        var pageIndex = 0;

        while (pageIndex < pages.Count)
        {
            outputDocument.PageSettings.Size = PdfPageSize.A4;
            outputDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;

            var outputPage = outputDocument.Pages.Add();

            var pageWidth = outputPage.GetClientSize().Width;
            var pageHeight = outputPage.GetClientSize().Height;

            var margin = 0f;
            var gap = 5f;

            var availableWidth = pageWidth - 2 * margin - gap;
            var slotWidth = availableWidth / 2f;
            var slotHeight = pageHeight - 2 * margin;

            DrawTemplateIntoSlot(outputPage, pages[pageIndex].Template, margin, margin, slotWidth, slotHeight);

            if (pageIndex + 1 < pages.Count)
                DrawTemplateIntoSlot(
                    outputPage,
                    pages[pageIndex + 1].Template,
                    margin + slotWidth + gap,
                    margin,
                    slotWidth,
                    slotHeight);

            pageIndex += 2;
        }
    }

    private static void AppendAllPagesFromPdf(PdfDocument outputDocument, string filePath)
    {
        using (var input = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var loadedDocument = new PdfLoadedDocument(input))
        {
            for (var i = 0; i < loadedDocument.Pages.Count; i++)
            {
                var sourcePage = (PdfLoadedPage)loadedDocument.Pages[i];
                var template = sourcePage.CreateTemplate();

                // Daten des Original PDF möglichst genau üernehmen
                // Orientierung
                var isLandscape = sourcePage.Size.Width > sourcePage.Size.Height;
                var section = outputDocument.Sections.Add();
                section.PageSettings.Orientation =
                    isLandscape ? PdfPageOrientation.Landscape : PdfPageOrientation.Portrait;

                // Rotation
                section.PageSettings.Rotate = sourcePage.Rotation;

                section.PageSettings.Margins.All = 0;

                var outputPage = section.Pages.Add();

                var pageWidth = outputPage.GetClientSize().Width;
                var pageHeight = outputPage.GetClientSize().Height;

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
        var originalWidth = template.Size.Width;
        var originalHeight = template.Size.Height;

        var scale = Math.Min(maxWidth / originalWidth, maxHeight / originalHeight);

        var drawWidth = originalWidth * scale;
        var drawHeight = originalHeight * scale;

        var drawX = x + (maxWidth - drawWidth) / 2f;
        var drawY = y + (maxHeight - drawHeight) / 2f;

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
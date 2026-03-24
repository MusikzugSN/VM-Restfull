using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Syncfusion.EJ2.PdfViewer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Redaction;
using System.Drawing;
using System.Net;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers;

[Route("[controller]")]
[ApiController]
public class PdfViewerController : ControllerBase
{
    private static readonly JsonSerializerSettings PascalCaseSettings =
        new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver()
        };

    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IMemoryCache _cache;
    private readonly MusicSheetService _musicSheetService;

    public PdfViewerController(
        IWebHostEnvironment hostingEnvironment,
        IMemoryCache cache,
        MusicSheetService musicSheetService)
    {
        _hostingEnvironment = hostingEnvironment;
        _cache = cache;
        _musicSheetService = musicSheetService;
        Console.WriteLine("PdfViewerController initialized");
    }

    [HttpPost("Load")]
    [Route("[controller]/Load")]
    public IActionResult Load([FromBody] Dictionary<string, string> jsonObject)
    {
        Console.WriteLine("Load called");

        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        MemoryStream stream = new MemoryStream();

        if (jsonObject != null && jsonObject.ContainsKey("document"))
        {
            if (bool.Parse(jsonObject["isFileName"]))
            {
                string document = jsonObject["document"];

                if (document.StartsWith("vm-web://", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseVmWebDocument(document, out int musicSheetId, out int? fromPage, out int? toPage, out string? parseError))
                        return BadRequest(parseError);

                    var pdfResult = _musicSheetService.GetViewerPdfContent(musicSheetId, fromPage, toPage);

                    if (!pdfResult.IsSuccessful())
                        return (ObjectResult)pdfResult;

                    byte[] bytes = pdfResult.GetValue()!;
                    stream = new MemoryStream(bytes);
                }
                else
                {
                    string documentPath = GetDocumentPath(document);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                        stream = new MemoryStream(bytes);
                    }
                    else
                    {
                        string fileName = document.Split(new string[] { "://" }, StringSplitOptions.None)[0];

                        if (fileName == "http" || fileName == "https")
                        {
                            using WebClient webClient = new WebClient();
                            byte[] pdfDoc = webClient.DownloadData(document);
                            stream = new MemoryStream(pdfDoc);
                        }
                        else
                        {
                            return Content(document + " is not found");
                        }
                    }
                }
            }
            else
            {
                byte[] bytes = Convert.FromBase64String(jsonObject["document"]);
                stream = new MemoryStream(bytes);
            }
        }

        object jsonResult = pdfviewer.Load(stream, jsonObject);
        return new JsonResult(jsonResult, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("ValidatePassword")]
    [Route("[controller]/ValidatePassword")]
    public IActionResult ValidatePassword([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        MemoryStream stream = new MemoryStream();

        if (jsonObject != null && jsonObject.ContainsKey("document"))
        {
            if (bool.Parse(jsonObject["isFileName"]))
            {
                string document = jsonObject["document"];

                if (document.StartsWith("vm-web://", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseVmWebDocument(document, out int musicSheetId, out int? fromPage, out int? toPage, out string? parseError))
                        return BadRequest(parseError);

                    var pdfResult = _musicSheetService.GetViewerPdfContent(musicSheetId, fromPage, toPage);

                    if (!pdfResult.IsSuccessful())
                        return (ObjectResult)pdfResult;

                    byte[] bytes = pdfResult.GetValue()!;
                    stream = new MemoryStream(bytes);
                }
                else
                {
                    string documentPath = GetDocumentPath(document);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                        stream = new MemoryStream(bytes);
                    }
                    else
                    {
                        string fileName = document.Split(new string[] { "://" }, StringSplitOptions.None)[0];

                        if (fileName == "http" || fileName == "https")
                        {
                            using WebClient webClient = new WebClient();
                            byte[] pdfDoc = webClient.DownloadData(document);
                            stream = new MemoryStream(pdfDoc);
                        }
                        else
                        {
                            return Content(document + " is not found");
                        }
                    }
                }
            }
            else
            {
                byte[] bytes = Convert.FromBase64String(jsonObject["document"]);
                stream = new MemoryStream(bytes);
            }
        }

        string? password = null;
        if (jsonObject.ContainsKey("password"))
            password = jsonObject["password"];

        var result = pdfviewer.Load(stream, password);
        return new JsonResult(result, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("Bookmarks")]
    [Route("[controller]/Bookmarks")]
    public IActionResult Bookmarks([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        var jsonResult = pdfviewer.GetBookmarks(jsonObject);
        return new JsonResult(jsonResult, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("RenderPdfPages")]
    [Route("[controller]/RenderPdfPages")]
    public IActionResult RenderPdfPages([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        object jsonResult = pdfviewer.GetPage(jsonObject);
        return new JsonResult(jsonResult, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("RenderPdfTexts")]
    [Route("[controller]/RenderPdfTexts")]
    public IActionResult RenderPdfTexts([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        object jsonResult = pdfviewer.GetDocumentText(jsonObject);
        return new JsonResult(jsonResult, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("RenderThumbnailImages")]
    [Route("[controller]/RenderThumbnailImages")]
    public IActionResult RenderThumbnailImages([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        object result = pdfviewer.GetThumbnailImages(jsonObject);
        return new JsonResult(result, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("RenderAnnotationComments")]
    [Route("[controller]/RenderAnnotationComments")]
    public IActionResult RenderAnnotationComments([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);
        return new JsonResult(jsonResult, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("ExportAnnotations")]
    [Route("[controller]/ExportAnnotations")]
    public IActionResult ExportAnnotations([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        string jsonResult = pdfviewer.ExportAnnotation(jsonObject);
        return new JsonResult(jsonResult, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("ImportAnnotations")]
    [Route("[controller]/ImportAnnotations")]
    public IActionResult ImportAnnotations([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        string jsonResult = string.Empty;
        object JsonResult;

        if (jsonObject != null && jsonObject.ContainsKey("fileName"))
        {
            string documentPath = GetDocumentPath(jsonObject["fileName"]);
            if (!string.IsNullOrEmpty(documentPath))
            {
                jsonResult = System.IO.File.ReadAllText(documentPath);
                string[] searchStrings =
                {
                    "textMarkupAnnotation",
                    "measureShapeAnnotation",
                    "freeTextAnnotation",
                    "stampAnnotations",
                    "signatureInkAnnotation",
                    "stickyNotesAnnotation",
                    "signatureAnnotation",
                    "AnnotationType"
                };

                bool isnewJsonFile = !searchStrings.Any(jsonResult.Contains);
                if (isnewJsonFile)
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                    jsonObject["importedData"] = Convert.ToBase64String(bytes);
                    JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                    jsonResult = JsonConvert.SerializeObject(JsonResult);
                }
            }
            else
            {
                return Content(jsonObject["fileName"] + " is not found");
            }
        }
        else
        {
            string extension = Path.GetExtension(jsonObject["importedData"]);
            if (extension != ".xfdf")
            {
                JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                return Content(JsonConvert.SerializeObject(JsonResult));
            }
            else
            {
                string documentPath = GetDocumentPath(jsonObject["importedData"]);
                if (!string.IsNullOrEmpty(documentPath))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                    jsonObject["importedData"] = Convert.ToBase64String(bytes);
                    JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                    return Content(JsonConvert.SerializeObject(JsonResult));
                }
                else
                {
                    return Content(jsonObject["importedData"] + " is not found");
                }
            }
        }

        return new JsonResult(jsonResult, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("ExportFormFields")]
    [Route("[controller]/ExportFormFields")]
    public IActionResult ExportFormFields([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        string jsonResult = pdfviewer.ExportFormFields(jsonObject);
        return new JsonResult(jsonResult, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("ImportFormFields")]
    [Route("[controller]/ImportFormFields")]
    public IActionResult ImportFormFields([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        jsonObject["data"] = GetDocumentPath(jsonObject["data"]);
        object jsonResult = pdfviewer.ImportFormFields(jsonObject);
        return new JsonResult(jsonResult, PascalCaseSettings);
    }

    [AcceptVerbs("Post")]
    [HttpPost("Unload")]
    [Route("[controller]/Unload")]
    public IActionResult Unload([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        pdfviewer.ClearCache(jsonObject);
        return Content("Document cache is cleared");
    }

    [HttpPost("Download")]
    [Route("[controller]/Download")]
    public IActionResult Download([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
        return Content(documentBase);
    }

    [HttpPost("PrintImages")]
    [Route("[controller]/PrintImages")]
    public IActionResult PrintImages([FromBody] Dictionary<string, string> jsonObject)
    {
        PdfRenderer pdfviewer = new PdfRenderer(_cache);
        object pageImage = pdfviewer.GetPrintImage(jsonObject);
        return Content(JsonConvert.SerializeObject(pageImage));
    }

    [HttpPost("Redaction")]
    [Route("[controller]/Redaction")]
    public IActionResult Redaction([FromBody] Dictionary<string, string> jsonObject)
    {
        string RedactionText = "Redacted";
        var finalbase64 = string.Empty;

        if (jsonObject != null && jsonObject.ContainsKey("base64String"))
        {
            string base64 = jsonObject["base64String"];
            string base64String = base64.Split(new string[] { "data:application/pdf;base64," }, StringSplitOptions.None)[1];

            if (!string.IsNullOrEmpty(base64String))
            {
                byte[] byteArray = Convert.FromBase64String(base64String);
                Console.WriteLine("redaction");

                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(byteArray);
                foreach (PdfLoadedPage loadedPage in loadedDocument.Pages)
                {
                    List<PdfLoadedAnnotation> removeItems = new List<PdfLoadedAnnotation>();

                    foreach (PdfLoadedAnnotation annotation in loadedPage.Annotations)
                    {
                        if (annotation is PdfLoadedRectangleAnnotation)
                        {
                            if (annotation.Author == "Redaction")
                            {
                                removeItems.Add(annotation);
                                PdfRedaction redaction = new PdfRedaction(annotation.Bounds, annotation.Color);
                                loadedPage.AddRedaction(redaction);
                                annotation.Flatten = true;
                            }

                            if (annotation.Author == "Text")
                            {
                                removeItems.Add(annotation);
                                PdfRedaction redaction = new PdfRedaction(annotation.Bounds);
                                PdfStandardFont font = new PdfStandardFont(PdfFontFamily.Courier, 8);

                                CreateRedactionAppearance(
                                    redaction.Appearance.Graphics,
                                    PdfTextAlignment.Left,
                                    true,
                                    new SizeF(annotation.Bounds.Width, annotation.Bounds.Height),
                                    RedactionText,
                                    font,
                                    PdfBrushes.Red);

                                loadedPage.AddRedaction(redaction);
                                annotation.Flatten = true;
                            }

                            if (annotation.Author == "Pattern")
                            {
                                removeItems.Add(annotation);
                                PdfRedaction redaction = new PdfRedaction(annotation.Bounds);

                                Syncfusion.Drawing.RectangleF rect = new Syncfusion.Drawing.RectangleF(0, 0, 8, 8);
                                PdfTilingBrush tillingBrush = new PdfTilingBrush(rect);
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.Gray, new Syncfusion.Drawing.RectangleF(0, 0, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.White, new Syncfusion.Drawing.RectangleF(2, 0, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(4, 0, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.DarkGray, new Syncfusion.Drawing.RectangleF(6, 0, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.White, new Syncfusion.Drawing.RectangleF(0, 2, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(2, 2, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(4, 2, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(6, 2, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(0, 4, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.DarkGray, new Syncfusion.Drawing.RectangleF(2, 4, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(4, 4, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.White, new Syncfusion.Drawing.RectangleF(6, 4, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(0, 6, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(2, 6, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(4, 6, 2, 2));
                                tillingBrush.Graphics.DrawRectangle(PdfBrushes.DarkGray, new Syncfusion.Drawing.RectangleF(6, 6, 2, 2));

                                rect = new Syncfusion.Drawing.RectangleF(0, 0, 16, 14);
                                PdfTilingBrush tillingBrushNew = new PdfTilingBrush(rect);
                                tillingBrushNew.Graphics.DrawRectangle(tillingBrush, rect);

                                redaction.Appearance.Graphics.DrawRectangle(
                                    tillingBrushNew,
                                    new Syncfusion.Drawing.RectangleF(0, 0, annotation.Bounds.Width, annotation.Bounds.Height));

                                loadedPage.AddRedaction(redaction);
                                annotation.Flatten = true;
                            }
                        }
                        else if (annotation is PdfLoadedRubberStampAnnotation)
                        {
                            if (annotation.Author == "Image")
                            {
                                Stream[] images = PdfLoadedRubberStampAnnotationExtension.GetImages(annotation as PdfLoadedRubberStampAnnotation);
                                PdfRedaction redaction = new PdfRedaction(annotation.Bounds);
                                images[0].Position = 0;
                                PdfImage image = new PdfBitmap(images[0]);

                                redaction.Appearance.Graphics.DrawImage(
                                    image,
                                    new Syncfusion.Drawing.RectangleF(0, 0, annotation.Bounds.Width, annotation.Bounds.Height));

                                loadedPage.AddRedaction(redaction);
                                annotation.Flatten = true;
                            }
                        }
                    }

                    foreach (PdfLoadedAnnotation annotation1 in removeItems)
                    {
                        loadedPage.Annotations.Remove(annotation1);
                    }
                }

                loadedDocument.Redact();
                MemoryStream stream = new MemoryStream();
                loadedDocument.Save(stream);
                stream.Position = 0;
                loadedDocument.Close(true);

                byteArray = stream.ToArray();
                finalbase64 = "data:application/pdf;base64," + Convert.ToBase64String(byteArray);
                stream.Dispose();

                return Content(finalbase64);
            }
        }

        return Content("data:application/pdf;base64,");
    }

    private static bool TryParseVmWebDocument(
        string document,
        out int musicSheetId,
        out int? fromPage,
        out int? toPage,
        out string? error)
    {
        musicSheetId = 0;
        fromPage = null;
        toPage = null;
        error = null;

        string raw = document.Replace("vm-web://", "");
        string[] parts = raw.Split('?', 2);

        if (!int.TryParse(parts[0], out musicSheetId))
        {
            error = "Ungültige musicSheetId.";
            return false;
        }

        if (parts.Length == 1)
            return true;

        string[] queryParts = parts[1].Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (string queryPart in queryParts)
        {
            string[] kv = queryPart.Split('=', 2);
            if (kv.Length != 2)
                continue;

            if (kv[0] == "from")
            {
                if (!int.TryParse(kv[1], out int parsedFrom))
                {
                    error = "Ungültiger from-Wert.";
                    return false;
                }

                fromPage = parsedFrom;
            }
            else if (kv[0] == "to")
            {
                if (!int.TryParse(kv[1], out int parsedTo))
                {
                    error = "Ungültiger to-Wert.";
                    return false;
                }

                toPage = parsedTo;
            }
        }

        if (fromPage != null && fromPage <= 0)
        {
            error = "from muss größer als 0 sein.";
            return false;
        }

        if (toPage != null && toPage <= 0)
        {
            error = "to muss größer als 0 sein.";
            return false;
        }

        if (fromPage != null && toPage != null && fromPage > toPage)
        {
            error = "from darf nicht größer als to sein.";
            return false;
        }

        return true;
    }

    private static void CreateRedactionAppearance(
        PdfGraphics graphics,
        PdfTextAlignment alignment,
        bool repeat,
        SizeF size,
        string overlayText,
        PdfFont font,
        PdfBrush textcolor)
    {
        float col = 0, row;
        if (font == null)
            font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);

        int textAlignment = Convert.ToInt32(alignment);
        float y = 0, x = 0, diff = 0;
        Syncfusion.Drawing.RectangleF rect;
        Syncfusion.Drawing.SizeF textsize = font.MeasureString(overlayText);

        if (repeat)
        {
            col = size.Width / textsize.Width;
            row = (float)Math.Floor(size.Height / font.Size);
            diff = Math.Abs(size.Width - (float)(Math.Floor(col) * textsize.Width));

            if (textAlignment == 1)
                x = diff / 2;

            if (textAlignment == 2)
                x = diff;

            for (int i = 1; i < col; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    rect = new Syncfusion.Drawing.RectangleF(x, y, 0, 0);
                    graphics.DrawString(overlayText, font, textcolor, rect);
                    y = y + font.Size;
                }

                x = x + textsize.Width;
                y = 0;
            }
        }
        else
        {
            diff = Math.Abs(size.Width - textsize.Width);

            if (textAlignment == 1)
                x = diff / 2;

            if (textAlignment == 2)
                x = diff;

            rect = new Syncfusion.Drawing.RectangleF(x, 0, 0, 0);
            graphics.DrawString(overlayText, font, textcolor, rect);
        }
    }

    private string GetDocumentPath(string document)
    {
        string documentPath = string.Empty;

        if (!System.IO.File.Exists(document))
        {
            var path = _hostingEnvironment.ContentRootPath;
            if (System.IO.File.Exists(path + "/Data/" + document))
                documentPath = path + "/Data/" + document;
        }
        else
        {
            documentPath = document;
        }

        Console.WriteLine(documentPath);
        return documentPath;
    }

    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }
}
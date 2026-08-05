using Microsoft.AspNetCore.Mvc;
using PaymentApp.Application.DTOs;
using PaymentApp.Application.Interfaces;
using PaymentApp.Domain.Exceptions;

namespace PaymentApp.Api.Controllers;

[ApiController]
[Route("v1/document")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ScanResult>> Upload(int userId, IFormFile file)
    {
        // Validate file type
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".txt")
            return BadRequest(new { error = "Only .txt files are accepted." });

        // I/O: read the uploaded bytes
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        // CPU: scan the document on a pool thread
        // Task.Run moves CPU-bound work off the request thread
        var result = await Task.Run(() => _documentService.Scan(file.FileName, bytes));

        // I/O: store the file AND its metadata; return the metadata to the caller
        try
        {
            var meta = await _documentService.StoreAsync(userId, file.FileName, bytes, result);
            return Ok(meta);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new { code = ex.Code, message = ex.Message });
        }
    }

    [HttpGet("download")]
    public async Task<IActionResult> Download(int userId)
    {
        try
        {
            var (content, meta) = await _documentService.OpenAsync(userId);
            return File(content, "application/octet-stream", meta.OriginalName);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new { code = ex.Code, message = ex.Message });
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("statement")]
    public async Task<IActionResult> Statement(int userId, string? currency = null)
    {
        try
        {
            var text = await _documentService.BuildStatementAsync(userId, currency);
            return Content(text, "text/plain");
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new { code = ex.Code, message = ex.Message });
        }
    }
}
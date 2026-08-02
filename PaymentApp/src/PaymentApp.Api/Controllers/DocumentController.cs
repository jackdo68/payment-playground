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

        // I/O: store on disk and update user
        try
        {
            await _documentService.StoreAsync(userId, file.FileName, bytes);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new { code = ex.Code, message = ex.Message });
        }

        return Ok(result);
    }
}
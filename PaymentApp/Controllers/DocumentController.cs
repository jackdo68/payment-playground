using Microsoft.AspNetCore.Mvc;
using PaymentApp.Services.Document;

namespace PaymentApp.Controllers;

[ApiController]
[Route("v1/document")]
public class DocumentController(DocumentService documents) : ControllerBase
{
    private readonly DocumentService _documents = documents;

    [HttpPost("upload")]
    public async Task<ActionResult<ScanResult>> Upload(int userId, IFormFile file)
    {
        if (Path.GetExtension(file.FileName).ToLowerInvariant() != ".txt")
            return BadRequest(new { error = "Only .txt files are accepted." });

        var ms = new MemoryStream();
        try
        {
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var result = await Task.Run(() => _documents.Scan(file.FileName, bytes));
            try { await _documents.StoreAsync(userId, bytes); }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            return Ok(result);
        }
        finally
        {
            ms.Dispose();   // always runs, even on exception
        }
    }
}
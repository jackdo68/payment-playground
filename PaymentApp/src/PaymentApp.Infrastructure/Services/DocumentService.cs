using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaymentApp.Application.DTOs;
using PaymentApp.Application.Interfaces;
using PaymentApp.Domain.Exceptions;
using PaymentApp.Infrastructure.Data;

namespace PaymentApp.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly PaymentDbContext _db;
    private readonly string _uploadDir;

    // Write pretty, camelCase JSON (readable when you `cat` the sidecar).
    private static readonly JsonSerializerOptions _jsonWrite = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    // Web defaults are camelCase + case-insensitive — perfect for reading it back.
    private static readonly JsonSerializerOptions _jsonRead = new(JsonSerializerDefaults.Web);

    public DocumentService(PaymentDbContext db)
    {
        _db = db;
        _uploadDir = Path.Combine(AppContext.BaseDirectory, "uploads");
        Directory.CreateDirectory(_uploadDir);
    }

    public ScanResult Scan(string fileName, byte[] content)
    {
        var hash = Convert.ToHexString(SHA256.HashData(content));
        var text = Encoding.UTF8.GetString(content);
        double signal = 0;
        for (int i = 0; i < 5_000_000; i++) signal += Math.Sqrt(i);
        var words = text.Split(default(char[]?), StringSplitOptions.RemoveEmptyEntries).Length;
        var flagged = text.Contains("fraud", StringComparison.OrdinalIgnoreCase);
        return new ScanResult(fileName, words, hash, flagged);
    }

    public async Task<DocumentMetadata> StoreAsync(int userId, string fileName, byte[] content, ScanResult scan)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UserNotFoundException(userId);

        var storedName = $"{userId}_{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(_uploadDir, storedName);
        await File.WriteAllBytesAsync(filePath, content);   // I/O: the file itself

        // Build metadata and serialize it to a sidecar: "<storedName>.meta.json"
        var meta = new DocumentMetadata(
            OriginalName: fileName,
            StoredName: storedName,
            SizeBytes: content.LongLength,
            Sha256: scan.Sha256,
            Words: scan.Words,
            Flagged: scan.Flagged,
            UploadedAtUtc: DateTime.UtcNow);       // always store timestamps in UTC

        var metaPath = filePath + ".meta.json";
        await File.WriteAllTextAsync(metaPath, JsonSerializer.Serialize(meta, _jsonWrite));

        user.DocumentPath = storedName;
        await _db.SaveChangesAsync();
        return meta;
    }

    public async Task<(Stream Content, DocumentMetadata Meta)> OpenAsync(int userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UserNotFoundException(userId);

        if (string.IsNullOrEmpty(user.DocumentPath))
            throw new InvalidOperationException($"User {userId} has no document on file.");

        var filePath = Path.Combine(_uploadDir, user.DocumentPath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Stored document is missing.", user.DocumentPath);

        // Read the sidecar back into the record (JSON -> object)
        var metaPath = filePath + ".meta.json";
        var meta = JsonSerializer.Deserialize<DocumentMetadata>(
            await File.ReadAllTextAsync(metaPath), _jsonRead)!;

        // Open a READ STREAM — the framework streams these bytes to the client and
        // disposes the stream for us. A 2 GB file uses a small buffer, not 2 GB of RAM.
        Stream content = File.OpenRead(filePath);
        return (content, meta);
    }

    public async Task<string> BuildStatementAsync(int userId, string? currency = null)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UserNotFoundException(userId);

        var usd = CultureInfo.GetCultureInfo("en-US");

        // A small collection of label/value rows — iterated to render the body.
        var lines = new List<(string Label, string Value)>
    {
        ("Account holder", user.Name),
        ("Email", user.Email),
        ("Current balance", user.Balance.ToString("C", usd)),   // $1,000.00
        ("Document on file", string.IsNullOrEmpty(user.DocumentPath) ? "(none)" : user.DocumentPath),
    };

        // StringBuilder: build the report with one buffer, not string + string + ...
        var sb = new StringBuilder();
        sb.AppendLine("=== PaymentApp Account Statement ===");
        sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        foreach (var (label, value) in lines)
            sb.AppendLine($"{label,-18}: {value,38} |");   // {,-18} left-pads the label to 18 cols
        sb.AppendLine();
        sb.AppendLine("Thank you for banking with PaymentApp.");
        return sb.ToString();
    }
}
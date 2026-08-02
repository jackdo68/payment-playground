using System.Security.Cryptography;
using System.Text;
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

    public DocumentService(PaymentDbContext db)
    {
        _db = db;
        _uploadDir = Path.Combine(AppContext.BaseDirectory, "uploads");
        Directory.CreateDirectory(_uploadDir);
    }

    // CPU-BOUND: No awaits — this method burns CPU cycles
    // The caller should use Task.Run to move this off the request thread
    public ScanResult Scan(string fileName, byte[] content)
    {
        // Hash the content (CPU-intensive)
        var hash = Convert.ToHexString(SHA256.HashData(content));

        // Parse as text
        var text = Encoding.UTF8.GetString(content);

        // Simulate CPU-heavy work (malware scan, OCR, etc.)
        // In production, this might be ML inference, image processing, etc.
        double signal = 0;
        for (int i = 0; i < 5_000_000; i++)
            signal += Math.Sqrt(i);

        // Analyze the text
        var words = text.Split(default(char[]?), StringSplitOptions.RemoveEmptyEntries).Length;
        var flagged = text.Contains("fraud", StringComparison.OrdinalIgnoreCase);

        return new ScanResult(fileName, words, hash, flagged);
    }

    // I/O-BOUND: Uses await — call normally with await
    public async Task StoreAsync(int userId, string fileName, byte[] content)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UserNotFoundException(userId);

        // Generate a unique filename
        var storedName = $"{userId}_{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(_uploadDir, storedName);

        // Write to disk (I/O)
        await File.WriteAllBytesAsync(filePath, content);

        // Update user record
        user.DocumentPath = storedName;
        await _db.SaveChangesAsync();
    }
}
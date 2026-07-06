using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PaymentApp.Data;

namespace PaymentApp.Services.Document;

public record ScanResult(string Filename, int Words, string Sha256, bool Flagged);

public class DocumentService
{
    private readonly PaymentDbContext _db;
    private readonly string _dir = Path.Combine(AppContext.BaseDirectory, "uploads");

    public DocumentService(PaymentDbContext db)
    {
        _db = db;
        Directory.CreateDirectory(_dir);
    }

    public ScanResult Scan(string fileName, byte[] content)
    {
        var hash = Convert.ToHexString(SHA256.HashData(content));
        var text = Encoding.UTF8.GetString(content);

        double signal = 0;
        for (int i = 0; i < 500000; i++) signal += Math.Sqrt(i);

        var words = text.Split(default(char[]?),
        StringSplitOptions.RemoveEmptyEntries).Length;
        var flagged = text.Contains("fraud", StringComparison.OrdinalIgnoreCase);
        return new ScanResult(fileName, words, hash, flagged);
    }

    public async Task StoreAsync(int userId, byte[] content)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($"No user {userId}.");

        var stored = $"{userId}_{Guid.NewGuid():N}.txt";
        await File.WriteAllBytesAsync(Path.Combine(_dir, stored), content);
        user.File = stored;
        await _db.SaveChangesAsync();
    }
}
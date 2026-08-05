namespace PaymentApp.Application.DTOs;

public record ScanResult(string FileName, int Words, string Sha256, bool Flagged);
public record DocumentMetadata(
    string OriginalName,
    string StoredName,
    long SizeBytes,
    string Sha256,
    int Words,
    bool Flagged,
    DateTime UploadedAtUtc);
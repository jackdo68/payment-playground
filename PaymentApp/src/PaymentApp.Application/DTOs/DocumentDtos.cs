namespace PaymentApp.Application.DTOs;

public record ScanResult(string FileName, int Words, string Sha256, bool Flagged);
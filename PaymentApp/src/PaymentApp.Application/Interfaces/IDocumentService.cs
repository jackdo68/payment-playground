using PaymentApp.Application.DTOs;

namespace PaymentApp.Application.Interfaces;

public interface IDocumentService
{
    /// <summary>
    /// CPU-bound: hash and scan the document content.
    /// Call this with Task.Run to avoid blocking the request thread.
    /// </summary>
    ScanResult Scan(string fileName, byte[] content);

    /// <summary>
    /// I/O-bound: store the document on disk and update the user.
    /// </summary>
    Task<DocumentMetadata> StoreAsync(int userId, string fileName, byte[] content, ScanResult scan);

    // Returns an open read-stream for the user's document + its metadata.
    Task<(Stream Content, DocumentMetadata Meta)> OpenAsync(int userId);
}
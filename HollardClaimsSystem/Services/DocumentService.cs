using HollardClaimsSystem.Models;
using System.Text.Json;

namespace HollardClaimsSystem.Services;

public interface IDocumentService
{
    Task<UploadResult> UploadDocumentAsync(IFormFile file, string claimNumber, string uploadedBy);
    Task<Document?> GetDocumentAsync(string documentId);
    Task<IEnumerable<Document>> GetAllDocumentsAsync();
    Task<Stream?> GetDocumentStreamAsync(string documentId);
    Task<bool> DeleteDocumentAsync(string documentId);
    Task<DocumentReview> AddReviewAsync(string documentId, DocumentReview review);
    string GetContentType(string fileName);
    bool IsViewableInline(string contentType);
}

public class UploadResult
{
    public bool Success { get; set; }
    public string? DocumentId { get; set; }
    public string? FileName { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PreviewUrl { get; set; }
}

public class DocumentService : IDocumentService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DocumentService> _logger;
    private readonly string _documentsPath;
    private readonly string _metadataPath;

    private readonly HashSet<string> _viewableContentTypes = new()
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "text/plain"
    };

    public DocumentService(IWebHostEnvironment environment, ILogger<DocumentService> logger)
    {
        _environment = environment;
        _logger = logger;
        _documentsPath = Path.Combine(_environment.ContentRootPath, "Data", "Documents");
        _metadataPath = Path.Combine(_environment.ContentRootPath, "Data", "Metadata");

        Directory.CreateDirectory(_documentsPath);
        Directory.CreateDirectory(_metadataPath);
    }

    public async Task<UploadResult> UploadDocumentAsync(IFormFile file, string claimNumber, string uploadedBy)
    {
        try
        {
            if (file == null || file.Length == 0)
                return new UploadResult { Success = false, ErrorMessage = "No file provided" };

            if (file.Length > 50 * 1024 * 1024)
                return new UploadResult { Success = false, ErrorMessage = "File size exceeds 50MB limit" };

            var documentId = Guid.NewGuid().ToString();
            var safeFileName = $"{documentId}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(_documentsPath, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new Document
            {
                Id = documentId,
                FileName = safeFileName,
                OriginalFileName = file.FileName,
                FileSize = file.Length,
                ContentType = GetContentType(file.FileName),
                UploadedAt = DateTime.UtcNow,
                UploadedBy = uploadedBy,
                ClaimNumber = claimNumber,
                Status = DocumentStatus.PendingReview
            };

            var metadataFile = Path.Combine(_metadataPath, $"{documentId}.json");
            var json = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metadataFile, json);

            return new UploadResult
            {
                Success = true,
                DocumentId = documentId,
                FileName = document.OriginalFileName,
                PreviewUrl = $"/api/documents/view/{documentId}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return new UploadResult { Success = false, ErrorMessage = "Upload failed" };
        }
    }

    public async Task<Document?> GetDocumentAsync(string documentId)
    {
        var metadataFile = Path.Combine(_metadataPath, $"{documentId}.json");
        if (!File.Exists(metadataFile)) return null;
        var json = await File.ReadAllTextAsync(metadataFile);
        return JsonSerializer.Deserialize<Document>(json);
    }

    public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
    {
        var documents = new List<Document>();
        var metadataFiles = Directory.GetFiles(_metadataPath, "*.json");
        foreach (var file in metadataFiles)
        {
            var json = await File.ReadAllTextAsync(file);
            var doc = JsonSerializer.Deserialize<Document>(json);
            if (doc != null) documents.Add(doc);
        }
        return documents.OrderByDescending(d => d.UploadedAt);
    }

    public async Task<Stream?> GetDocumentStreamAsync(string documentId)
    {
        var document = await GetDocumentAsync(documentId);
        if (document == null) return null;
        var filePath = Path.Combine(_documentsPath, document.FileName);
        return !File.Exists(filePath) ? null : new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        try
        {
            var document = await GetDocumentAsync(documentId);
            if (document == null) return false;

            var filePath = Path.Combine(_documentsPath, document.FileName);
            if (File.Exists(filePath)) File.Delete(filePath);

            var metadataFile = Path.Combine(_metadataPath, $"{documentId}.json");
            if (File.Exists(metadataFile)) File.Delete(metadataFile);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document");
            return false;
        }
    }

    public async Task<DocumentReview> AddReviewAsync(string documentId, DocumentReview review)
    {
        var document = await GetDocumentAsync(documentId);
        if (document == null)
            throw new ArgumentException("Document not found");

        review.ReviewedAt = DateTime.UtcNow;
        review.Id = Guid.NewGuid().ToString();
        document.Reviews.Add(review);
        document.Status = review.Status;

        var metadataFile = Path.Combine(_metadataPath, $"{documentId}.json");
        var json = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(metadataFile, json);

        return review;
    }

    public string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    public bool IsViewableInline(string contentType) => _viewableContentTypes.Contains(contentType);
}
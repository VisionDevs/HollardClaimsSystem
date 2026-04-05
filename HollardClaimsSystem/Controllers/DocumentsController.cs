using HollardClaimsSystem.Models;
using HollardClaimsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace HollardClaimsSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file, [FromForm] string claimNumber = "CLAIM001", [FromForm] string uploadedBy = "System")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        var result = await _documentService.UploadDocumentAsync(file, claimNumber, uploadedBy);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result);
    }

    [HttpGet("view/{documentId}")]
    public async Task<IActionResult> ViewDocument(string documentId)
    {
        var document = await _documentService.GetDocumentAsync(documentId);
        if (document == null)
            return NotFound(new { error = "Document not found" });

        var stream = await _documentService.GetDocumentStreamAsync(documentId);
        if (stream == null)
            return NotFound(new { error = "Document file not found" });

        var contentType = _documentService.GetContentType(document.OriginalFileName);
        var isViewable = _documentService.IsViewableInline(contentType);

        if (!isViewable)
            return File(stream, contentType, document.OriginalFileName);

        Response.Headers.Append("Content-Disposition", $"inline; filename=\"{document.OriginalFileName}\"");
        return File(stream, contentType);
    }

    [HttpGet("metadata/{documentId}")]
    public async Task<IActionResult> GetDocumentMetadata(string documentId)
    {
        var document = await _documentService.GetDocumentAsync(documentId);
        if (document == null)
            return NotFound(new { error = "Document not found" });
        return Ok(document);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllDocuments()
    {
        var documents = await _documentService.GetAllDocumentsAsync();
        return Ok(documents);
    }

    [HttpDelete("{documentId}")]
    public async Task<IActionResult> DeleteDocument(string documentId)
    {
        var result = await _documentService.DeleteDocumentAsync(documentId);
        if (!result)
            return NotFound(new { error = "Document not found" });
        return Ok(new { message = "Document deleted successfully" });
    }

    [HttpPost("{documentId}/review")]
    public async Task<IActionResult> AddReview(string documentId, [FromBody] ReviewRequest reviewRequest)
    {
        try
        {
            // Convert string status to enum
            var status = reviewRequest.Status.ToLower() switch
            {
                "approved" => DocumentStatus.Approved,
                "rejected" => DocumentStatus.Rejected,
                "needsrevision" => DocumentStatus.NeedsRevision,
                _ => DocumentStatus.PendingReview
            };

            var review = new DocumentReview
            {
                ReviewerName = reviewRequest.ReviewerName ?? "Hollard Verifier",
                Comments = reviewRequest.Comments,
                Status = status
            };

            var result = await _documentService.AddReviewAsync(documentId, review);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // Add this new class inside the same file, after the controller class
    public class ReviewRequest
    {
        public string ReviewerName { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
using System.Text.Json.Serialization;

namespace HollardClaimsSystem.Models;

public class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuredName { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DocumentStatus Status { get; set; }

    public List<DocumentReview> Reviews { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DocumentStatus
{
    PendingReview,
    Approved,
    Rejected,
    NeedsRevision
}

public class DocumentReview
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ReviewerName { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DocumentStatus Status { get; set; }

    public DateTime ReviewedAt { get; set; }
}
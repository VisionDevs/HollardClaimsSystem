namespace HollardClaimsSystem.Models;

public class Claim
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuredName { get; set; } = string.Empty;
    public string InsuredEmail { get; set; } = string.Empty;
    public string InsuredPhone { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public DateTime ClaimDate { get; set; } = DateTime.Now;
    public string ClaimType { get; set; } = string.Empty;
    public decimal ClaimAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public List<string> DocumentIds { get; set; } = new();
}

public class ClaimSubmission
{
    public Claim Claim { get; set; } = new();
    public List<IFormFile> Documents { get; set; } = new();
}
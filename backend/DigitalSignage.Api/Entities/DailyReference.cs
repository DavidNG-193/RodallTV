namespace DigitalSignage.Api.Entities;

public sealed class DailyReference
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateOnly ReferenceDate { get; set; }
    public string Client { get; set; } = string.Empty;
    public string OperationCode { get; set; } = string.Empty;
    public string OperationDisplayName { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public int CustomsOfficeNumber { get; set; }
    public string CustomsOffice { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public DateTime LastExternalUpdateAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
}

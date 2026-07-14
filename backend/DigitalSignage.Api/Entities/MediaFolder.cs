namespace DigitalSignage.Api.Entities;

public class MediaFolder
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
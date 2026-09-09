namespace DigitalSignage.Api.Configuration;

public sealed class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";

    public string RootPath { get; set; } = "/RodallTVData/media";
}

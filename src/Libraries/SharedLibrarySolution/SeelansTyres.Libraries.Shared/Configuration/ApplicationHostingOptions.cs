namespace SeelansTyres.Libraries.Shared.Configuration;

/// <summary>
/// Used to bind to InAzure and InContainer settings at the root of configuration
/// </summary>
public class ApplicationHostingOptions
{
    public bool InAzure { get; set; } = default;
    public bool InContainer { get; set; } = default;
}

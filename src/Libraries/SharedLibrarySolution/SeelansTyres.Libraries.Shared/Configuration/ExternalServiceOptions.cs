namespace SeelansTyres.Libraries.Shared.Configuration;

/// <summary>
/// Used to bind external services like AzureAppConfig, AppInsights or Redis from configuation
/// </summary>
public class ExternalServiceOptions
{
    public bool Enabled { get; set; } = default;
    public string ConnectionString { get; set; } = string.Empty;
}

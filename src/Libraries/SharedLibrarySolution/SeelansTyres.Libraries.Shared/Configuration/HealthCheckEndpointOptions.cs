namespace SeelansTyres.Libraries.Shared.Configuration;

/// <summary>
/// Used to bind the HealthCheckEndpoint and LivenessCheckEndpoint settings from configuration
/// </summary>
public class HealthCheckEndpointOptions
{
    public string HealthCheckEndpoint { get; set; } = string.Empty;
    public string LivenessCheckEndpoint { get; set; } = string.Empty;
}

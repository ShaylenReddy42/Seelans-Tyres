namespace SeelansTyres.Libraries.Shared.Models;

public class HealthChecksModel
{
    public bool EnableElasticsearchHealthCheck { get; set; } = default;
    public string ElasticsearchUrl { get; set; } = string.Empty;
}

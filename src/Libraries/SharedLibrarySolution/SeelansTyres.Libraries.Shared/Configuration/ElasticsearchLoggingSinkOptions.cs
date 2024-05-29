namespace SeelansTyres.Libraries.Shared.Configuration;

/// <summary>
/// Used to bind the LoggingSinks:Elasticsearch configuration section
/// </summary>
public class ElasticsearchLoggingSinkOptions
{
    public string Enabled { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

namespace SeelansTyres.Libraries.Shared.Models;

public class RabbitMQSettingsModel
{
    // Credentials
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    // Connection Properties
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; }
    // Bindings
    public string Exchange { get; set; } = string.Empty;
    public string? Queue { get; set; }
}

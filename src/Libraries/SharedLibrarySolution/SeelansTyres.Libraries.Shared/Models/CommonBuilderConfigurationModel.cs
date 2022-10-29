using System.Reflection;

namespace SeelansTyres.Libraries.Shared.Models;

public class CommonBuilderConfigurationModel
{
    public int KestrelLocalhostPortNumber { get; set; }
    public Assembly OriginAssembly { get; set; } = Assembly.GetExecutingAssembly();
    public string DefaultDescriptiveApplicationName { get; set; } = string.Empty;
}

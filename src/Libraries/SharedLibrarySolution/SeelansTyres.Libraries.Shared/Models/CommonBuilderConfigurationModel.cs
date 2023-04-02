using System.Reflection; // Assembly

namespace SeelansTyres.Libraries.Shared.Models;

public class CommonBuilderConfigurationModel
{
    public Assembly OriginAssembly { get; set; } = Assembly.GetExecutingAssembly();
    public string DefaultDescriptiveApplicationName { get; set; } = string.Empty;
}

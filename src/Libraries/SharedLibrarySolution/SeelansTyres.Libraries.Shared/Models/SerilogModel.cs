using System.Reflection;

namespace SeelansTyres.Libraries.Shared.Models;

public class SerilogModel
{
    public Assembly Assembly { get; set; } = Assembly.GetExecutingAssembly();
    public string DefaultDescriptiveApplicationName { get; set; } = string.Empty;
}

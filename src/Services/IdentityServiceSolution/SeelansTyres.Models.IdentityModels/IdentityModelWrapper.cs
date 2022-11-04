namespace SeelansTyres.Models.IdentityModels;

public class IdentityModelWrapper
{
    public byte[] SerializedIdentityModel { get; set; } = Array.Empty<byte>();
    public static string ModelVersion { get; set; } = Constants.CurrentVersion;
}

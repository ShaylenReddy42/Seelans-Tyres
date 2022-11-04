namespace SeelansTyres.Models.IdentityModels;

public class IdentityModelWrapper
{
    public byte[] SerializedIdentityModel { get; set; } = Array.Empty<byte>();
    public Type ModelType { get; set; } = Type.EmptyTypes[0];
    public string ModelVersion { get; set; } = Constants.CurrentVersion;
}

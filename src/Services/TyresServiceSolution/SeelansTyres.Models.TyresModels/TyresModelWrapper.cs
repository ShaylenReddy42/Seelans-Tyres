namespace SeelansTyres.Models.TyresModels;

public class TyresModelWrapper
{
    public byte[] SerializedTyresModel { get; set; } = Array.Empty<byte>();
    public static string ModelVersion { get; set; } = Constants.CurrentVersion;
    public Type ModelType { get; set; } = Type.EmptyTypes[0];
}

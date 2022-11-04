namespace SeelansTyres.Models.TyresModels;

public class TyresModelWrapper
{
    public byte[] SerializedTyresModel { get; set; } = Array.Empty<byte>();
    public Type ModelType { get; set; } = Type.EmptyTypes[0];
    public string ModelVersion { get; set; } = Constants.CurrentVersion;
}

namespace SeelansTyres.Models.AddressModels;

public class AddressModelWrapper
{
    public byte[] SerializedAddressModel { get; set; } = Array.Empty<byte>();
    public Type ModelType { get; set; } = Type.EmptyTypes[0];
    public string ModelVersion { get; set; } = Constants.CurrentVersion;
}

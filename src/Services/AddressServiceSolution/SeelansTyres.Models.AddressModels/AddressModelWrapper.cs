namespace SeelansTyres.Models.AddressModels;

public class AddressModelWrapper
{
    public byte[] SerializedAddressModel { get; set; } = Array.Empty<byte>();
    public static string ModelVersion { get; set; } = Constants.CurrentVersion;
}

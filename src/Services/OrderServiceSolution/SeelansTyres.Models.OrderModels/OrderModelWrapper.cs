namespace SeelansTyres.Models.OrderModels;

public class OrderModelWrapper
{
    public byte[] SerializedOrderModel { get; set; } = Array.Empty<byte>();
    public Type ModelType { get; set; } = Type.EmptyTypes[0];
    public string ModelVersion { get; set; } = Constants.CurrentVersion;
}

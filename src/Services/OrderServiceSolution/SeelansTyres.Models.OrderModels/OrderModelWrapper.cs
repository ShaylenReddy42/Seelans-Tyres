namespace SeelansTyres.Models.OrderModels;

public class OrderModelWrapper
{
    public byte[] SerializedOrderModel { get; set; } = Array.Empty<byte>();
    public static string ModelVersion { get; set; } = Constants.CurrentVersion;
    public Type ModelType { get; set; } = Type.EmptyTypes[0];
}

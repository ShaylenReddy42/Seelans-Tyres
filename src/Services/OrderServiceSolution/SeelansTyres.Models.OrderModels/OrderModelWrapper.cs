﻿namespace SeelansTyres.Models.OrderModels;

public class OrderModelWrapper
{
    public byte[] SerializedOrderModel { get; set; } = Array.Empty<byte>();
    public static string ModelVersion => Constants.CurrentVersion;
}

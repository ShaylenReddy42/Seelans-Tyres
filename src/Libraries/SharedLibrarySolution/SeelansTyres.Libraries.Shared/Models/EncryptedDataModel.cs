using System.Security.Cryptography; // RandomNumberGenerator

namespace SeelansTyres.Libraries.Shared.Models;

public class EncryptedDataModel
{
    public byte[] AesGcmNonce { get; set; } = RandomNumberGenerator.GetBytes(12);
    public byte[] EncryptedAesKey { get; set; } = Array.Empty<byte>();
    public byte[] AesGcmCipherText { get; set; } = Array.Empty<byte>();
    public byte[] AesGcmTag { get; set; } = new byte[16];
}

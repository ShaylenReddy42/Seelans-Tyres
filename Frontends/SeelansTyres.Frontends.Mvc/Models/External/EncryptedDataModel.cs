using System.Security.Cryptography;

namespace SeelansTyres.Frontends.Mvc.Models.External;

public class EncryptedDataModel
{
    public byte[] AesGcmNonce { get; set; } = RandomNumberGenerator.GetBytes(12);
    public byte[] EncryptedAesKey { get; set; } = Array.Empty<byte>();
    public byte[] AesGcmCipherText { get; set; } = Array.Empty<byte>();
    public byte[] AesGcmTag { get; set; } = new byte[16];
    public byte[] HmacOfCipherText { get; set; } = Array.Empty<byte>();
}

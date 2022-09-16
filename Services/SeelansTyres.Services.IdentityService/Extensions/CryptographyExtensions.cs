using IdentityServer4.Stores;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Services.IdentityService.Models;
using System.Security.Cryptography;
using System.Text.Json;

namespace SeelansTyres.Services.IdentityService.Extensions;

public static class CryptographyExtensions
{
    public static async Task<T?> DecryptAsync<T>(
        this EncryptedDataModel encryptedDataModel, 
        ISigningCredentialStore signingCredentialStore)
    {
        var jsonWebKey = (JsonWebKey)(await signingCredentialStore.GetSigningCredentialsAsync()).Key;

        var rsaParameters = new RSAParameters
        {
            D = Base64UrlEncoder.DecodeBytes(jsonWebKey.D),
            DP = Base64UrlEncoder.DecodeBytes(jsonWebKey.DP),
            DQ = Base64UrlEncoder.DecodeBytes(jsonWebKey.DQ),
            Exponent = Base64UrlEncoder.DecodeBytes(jsonWebKey.E),
            InverseQ = Base64UrlEncoder.DecodeBytes(jsonWebKey.QI),
            Modulus = Base64UrlEncoder.DecodeBytes(jsonWebKey.N),
            P = Base64UrlEncoder.DecodeBytes(jsonWebKey.P),
            Q = Base64UrlEncoder.DecodeBytes(jsonWebKey.Q)
        };

        var rsa = RSA.Create(rsaParameters);

        var aesKey = rsa.Decrypt(encryptedDataModel.EncryptedAesKey, RSAEncryptionPadding.OaepSHA256);

        using var hmac = new HMACSHA256(aesKey);

        var recomputedHmac = hmac.ComputeHash(encryptedDataModel.AesGcmCipherText);

        if (Convert.ToBase64String(recomputedHmac) != Convert.ToBase64String(encryptedDataModel.HmacOfCipherText))
        {
            return default;
        }

        var modelAsBytes = new byte[encryptedDataModel.AesGcmCipherText.Length];

        using var aesGcm = new AesGcm(aesKey);

        try
        {
            aesGcm.Decrypt(
                nonce: encryptedDataModel.AesGcmNonce,
                ciphertext: encryptedDataModel.AesGcmCipherText,
                tag: encryptedDataModel.AesGcmTag,
                plaintext: modelAsBytes);
        }
        catch (CryptographicException)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(modelAsBytes);
    }
}

using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Mvc.Models.External;
using System.Security.Cryptography;
using System.Text.Json;

namespace SeelansTyres.Mvc.Extensions;

public static class CryptographyExtensions
{
    public static async Task<EncryptedDataModel> EncryptAsync<T>(this T model, HttpClient client) where T : class
    {
        var modelAsJson = JsonSerializer.SerializeToUtf8Bytes(model);

        var encryptedDataModel = new EncryptedDataModel();

        var aesKey = RandomNumberGenerator.GetBytes(32);

        encryptedDataModel.AesGcmCipherText = new byte[modelAsJson.Length];

        using var aesGcm = new AesGcm(aesKey);

        aesGcm.Encrypt(
            nonce: encryptedDataModel.AesGcmNonce, 
            plaintext: modelAsJson, 
            ciphertext: encryptedDataModel.AesGcmCipherText, 
            tag: encryptedDataModel.AesGcmTag);

        using var hmac = new HMACSHA256(aesKey);

        encryptedDataModel.HmacOfCipherText = hmac.ComputeHash(encryptedDataModel.AesGcmCipherText);

        var discoveryDocument = await client.GetDiscoveryDocumentAsync();

        var jsonWebKey = discoveryDocument.KeySet.Keys[0];

        var rsaParameters = new RSAParameters
        {
            Exponent = Base64UrlEncoder.DecodeBytes(jsonWebKey.E),
            Modulus = Base64UrlEncoder.DecodeBytes(jsonWebKey.N)
        };

        var rsa = RSA.Create(rsaParameters);

        encryptedDataModel.EncryptedAesKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);

        return encryptedDataModel;
    }
}

using IdentityServer4.Stores;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Services.IdentityService.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace SeelansTyres.Services.IdentityService.Extensions;

public static class CryptographyExtensions
{
    public static async Task<T?> DecryptAsync<T>(
        this EncryptedDataModel encryptedDataModel, 
        ISigningCredentialStore signingCredentialStore,
        ILogger logger) where T : class
    {
        Stopwatch stopwatch = new();

        stopwatch.Start();
        
        logger.LogInformation(
            "Beginning decryption process for model of type {modelType}", 
            typeof(T).Name);

        logger.LogDebug("Retrieving the Json Web Key from the signing credential store");
        
        var jsonWebKey = (JsonWebKey)(await signingCredentialStore.GetSigningCredentialsAsync()).Key;

        logger.LogDebug("Converting the Json Web Key to an RSA private key. Values were encoded in base64url according to the documentation");

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

        logger.LogDebug("Decrypting the encrypted Aes key");

        var aesKey = rsa.Decrypt(encryptedDataModel.EncryptedAesKey, RSAEncryptionPadding.OaepSHA256);

        logger.LogDebug("Recomputing the hash of the encrypted data using the Aes key");

        using var hmac = new HMACSHA256(aesKey);

        var recomputedHmac = hmac.ComputeHash(encryptedDataModel.AesGcmCipherText);

        if (Convert.ToBase64String(recomputedHmac) != Convert.ToBase64String(encryptedDataModel.HmacOfCipherText))
        {
            stopwatch.Stop();
            
            logger.LogWarning(
                "{announcement} ({stopwatchElapsedTime}ms): Decryption failed, integrity of the Aes key or data compromised",
                "FAILED", stopwatch.ElapsedMilliseconds);
            
            return default;
        }

        logger.LogDebug("Creating a byte array to hold the decrypted data");
        
        var modelAsBytes = new byte[encryptedDataModel.AesGcmCipherText.Length];

        logger.LogDebug("Recreating the AesGcm instance with the Aes key to decrypt the data");

        using var aesGcm = new AesGcm(aesKey);

        try
        {
            logger.LogDebug("Attempting to decrypt the data");
            
            aesGcm.Decrypt(
                nonce: encryptedDataModel.AesGcmNonce,
                ciphertext: encryptedDataModel.AesGcmCipherText,
                tag: encryptedDataModel.AesGcmTag,
                plaintext: modelAsBytes);
        }
        catch (CryptographicException ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Decryption failed, integrity checks by the AesGcm algorithm indicates that the data has been compromised",
                "FAILED", stopwatch.ElapsedMilliseconds);
            
            return default;
        }

        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Decryption process for model of type {modelType} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, typeof(T).Name);

        return JsonSerializer.Deserialize<T>(modelAsBytes);
    }
}

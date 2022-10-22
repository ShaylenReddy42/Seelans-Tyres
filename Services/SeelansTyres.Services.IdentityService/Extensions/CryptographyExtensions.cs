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

        logger.LogDebug("Retrieving the RSA Security Key from the signing credential store");
        
        var rsaSecurityKey = (RsaSecurityKey)(await signingCredentialStore.GetSigningCredentialsAsync()).Key;

        var rsa = RSA.Create(rsaSecurityKey.Parameters);

        logger.LogDebug("Decrypting the encrypted Aes key");

        var aesKey = rsa.Decrypt(encryptedDataModel.EncryptedAesKey, RSAEncryptionPadding.OaepSHA256);

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

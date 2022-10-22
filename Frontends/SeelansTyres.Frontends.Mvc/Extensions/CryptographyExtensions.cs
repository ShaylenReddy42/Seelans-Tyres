using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Frontends.Mvc.Models.External;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace SeelansTyres.Frontends.Mvc.Extensions;

public static class CryptographyExtensions
{
    public static async Task<EncryptedDataModel> EncryptAsync<T>(
        this T model, 
        HttpClient client, 
        IConfiguration configuration,
        ILogger logger) where T : class
    {
        Stopwatch stopwatch = new();

        stopwatch.Start();

        logger.LogInformation(
            "Beginning encryption process for model of type {modelType}",
            typeof(T).Name);

        var modelAsJson = JsonSerializer.SerializeToUtf8Bytes(model);

        var encryptedDataModel = new EncryptedDataModel();

        logger.LogDebug("Randomly generating a symmetric Aes key");

        var aesKey = RandomNumberGenerator.GetBytes(32);

        logger.LogDebug("Creating a byte array to hold the encrypted data");

        encryptedDataModel.AesGcmCipherText = new byte[modelAsJson.Length];

        using var aesGcm = new AesGcm(aesKey);

        logger.LogDebug("Performing symmetric encryption of the data using AesGcm");

        aesGcm.Encrypt(
            nonce: encryptedDataModel.AesGcmNonce, 
            plaintext: modelAsJson, 
            ciphertext: encryptedDataModel.AesGcmCipherText, 
            tag: encryptedDataModel.AesGcmTag);

        logger.LogDebug("Attempting to retrieve the discovery document from IdentityServer4");

        var discoveryDocument = await client.GetDiscoveryDocumentAsync(configuration["IdentityServerUrl"]);

        if (discoveryDocument.IsError is true)
        {
            logger.LogError(
                "{announcement}: Attempt to retrieve the discovery document from IdentityServer4 was unsuccessful",
                "FAILED");
        }

        logger.LogDebug("Retrieving the Json Web Key from the discovery document");

        var jsonWebKey = discoveryDocument.KeySet.Keys[0];

        logger.LogDebug("Converting the Json Web Key to an RSA public key. Values were encoded in base64url according to the documentation");

        var rsaParameters = new RSAParameters
        {
            Exponent = Base64UrlEncoder.DecodeBytes(jsonWebKey.E),
            Modulus = Base64UrlEncoder.DecodeBytes(jsonWebKey.N)
        };

        var rsa = RSA.Create(rsaParameters);

        logger.LogDebug("Encrypting the Aes key");

        encryptedDataModel.EncryptedAesKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);

        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Encryption process for model of type {modelType} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, typeof(T).Name);

        return encryptedDataModel;
    }
}

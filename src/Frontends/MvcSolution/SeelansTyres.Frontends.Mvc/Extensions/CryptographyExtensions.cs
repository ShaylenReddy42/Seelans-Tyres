using IdentityModel.Client;                 // GetDiscoveryDocumentAsync()
using Microsoft.IdentityModel.Tokens;       // Base64UrlEncoder
using SeelansTyres.Libraries.Shared.Models; // EncryptedDataModel
using System.Diagnostics;                   // Stopwatch
using System.Security.Cryptography;         // RandomNumberGenerator, AesGcm, RSAParameters, RSA, RSAEncryptionPadding
using System.Text.Json;                     // JsonSerializer

namespace SeelansTyres.Frontends.Mvc.Extensions;

public static class CryptographyExtensions
{
    /// <summary>
    /// Encrypts a model of type T using hybrid encryption
    /// </summary>
    /// <typeparam name="T">The modeltype to encrypt</typeparam>
    /// <param name="model">The model</param>
    /// <param name="client">An http client used to retrieve the discovery document from IdentityServer4</param>
    /// <param name="configuration">An instance of IConfiguration to extract the identity server url</param>
    /// <param name="logger">An instance of ILogger injected from the client code's constructor</param>
    /// <returns>An encrypted data model</returns>
    public static async Task<EncryptedDataModel> EncryptAsync<T>(
        this T model, 
        HttpClient client, 
        IConfiguration configuration,
        ILogger logger) where T : class
    {
        Stopwatch stopwatch = new();

        stopwatch.Start();

        logger.LogInformation(
            "Beginning encryption process for model of type {ModelType}",
            typeof(T).Name);

        var modelAsJson = JsonSerializer.SerializeToUtf8Bytes(model);

        var encryptedDataModel = new EncryptedDataModel();

        logger.LogDebug("Randomly generating a symmetric Aes key");

        var aesKey = RandomNumberGenerator.GetBytes(32);

        encryptedDataModel.AesGcmCipherText = new byte[modelAsJson.Length];

        using var aesGcm = new AesGcm(aesKey, 16);

        logger.LogDebug("Performing symmetric encryption of the data using AesGcm");

        aesGcm.Encrypt(
            nonce: encryptedDataModel.AesGcmNonce, 
            plaintext: modelAsJson, 
            ciphertext: encryptedDataModel.AesGcmCipherText, 
            tag: encryptedDataModel.AesGcmTag);

        logger.LogDebug("Attempting to retrieve the discovery document from IdentityServer4 to extract the Json Web Keys [JWKs]");

        var discoveryDocument = await client.GetDiscoveryDocumentAsync(configuration["IdentityServer"]);

        if (discoveryDocument.IsError)
        {
            logger.LogError(
                "{Announcement}: Attempt to retrieve the discovery document from IdentityServer4 was unsuccessful. Reason: {IdentityServer4Error}",
                "FAILED", discoveryDocument.Error);
        }

        var jsonWebKey = discoveryDocument.KeySet!.Keys[0];

        logger.LogDebug("Converting the JWK to an RSA public key by first decoding the exponent and modulus from base64url strings, and finally encrypting the Aes key");

        var rsaParameters = new RSAParameters
        {
            Exponent = Base64UrlEncoder.DecodeBytes(jsonWebKey.E),
            Modulus = Base64UrlEncoder.DecodeBytes(jsonWebKey.N)
        };

        var rsa = RSA.Create(rsaParameters);

        encryptedDataModel.EncryptedAesKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);

        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Encryption process for model of type {ModelType} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, typeof(T).Name);

        return encryptedDataModel;
    }
}

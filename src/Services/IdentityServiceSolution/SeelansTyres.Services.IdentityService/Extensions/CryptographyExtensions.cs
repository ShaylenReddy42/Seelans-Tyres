﻿using IdentityServer4.Stores;                  // ISigningCredentialStore
using Microsoft.IdentityModel.Tokens;          // Base64UrlEncoder, SigningCredentials, SecurityAlgorithms
using SeelansTyres.Libraries.Shared.Constants; // LoggerConstants
using SeelansTyres.Libraries.Shared.Models;    // EncryptedDataModel
using System.Diagnostics;                      // Stopwatch
using System.Security.Cryptography;            // RSAParameters, RSA, RSAEncryptionPadding, AesGcm, CryptographicException
using System.Text.Json;                        // JsonSerializer

namespace SeelansTyres.Services.IdentityService.Extensions;

public static class CryptographyExtensions
{
    /// <summary>
    /// <para>Extracts the RSA parameters from configuration to create signing credentials for IdentityServer4</para>
    /// 
    /// <para>
    ///     RSA parameters were extracted from the temp key and added to appsettings [NOT A GOOD PRACTICE OF COURSE]<br/>
    ///     and were originally Base64Url encoded
    /// </para>
    /// </summary>
    /// <param name="builder">A builder for web applications</param>
    /// <returns>Signing credentials created from an RSA security key</returns>
    public static SigningCredentials GenerateSigningCredentialsFromConfiguration(
        this WebApplicationBuilder builder)
    {
        var rsaSecurityKey = new RsaSecurityKey(
            new RSAParameters
            {
                D = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:D"]),
                DP = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:DP"]),
                DQ = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:DQ"]),
                Exponent = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:Exponent"]),
                InverseQ = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:InverseQ"]),
                Modulus = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:Modulus"]),
                P = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:P"]),
                Q = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:Q"])
            });

        return new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);
    }
    
    /// <summary>
    /// Decrypts the cipher text to the original model of type T
    /// </summary>
    /// <typeparam name="T">Model to deserialize the decrypted cipher text to from the json as a utf8 byte array</typeparam>
    /// <param name="encryptedDataModel">The incoming encrypted model from the request</param>
    /// <param name="signingCredentialStore">IdentityServer4's credential store to extract the signing credentials to decrypt the encrypted Aes key</param>
    /// <param name="logger">An instance of ILogger injected from the client code's constructor</param>
    /// <returns>A decrypted model of type T if decryption succeeds, else default [null]</returns>
    public static async Task<T?> DecryptAsync<T>(
        this EncryptedDataModel encryptedDataModel, 
        ISigningCredentialStore signingCredentialStore,
        ILogger logger) where T : class
    {
        Stopwatch stopwatch = new();

        stopwatch.Start();
        
        logger.LogInformation(
            "Beginning decryption process for model of type {ModelType}", 
            typeof(T).Name);

        logger.LogDebug("Retrieving the RSA Security Key from the signing credential store, and decrypting the encrypted Aes key");
        
        var rsaSecurityKey = (RsaSecurityKey)(await signingCredentialStore.GetSigningCredentialsAsync()).Key;

        var rsa = RSA.Create(rsaSecurityKey.Parameters);

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
                "{Announcement} ({StopwatchElapsedTime}ms): Decryption failed, integrity checks by the AesGcm algorithm indicates that the data has been compromised",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds);
            
            return default;
        }

        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Decryption process for model of type {ModelType} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, typeof(T).Name);

        return JsonSerializer.Deserialize<T>(modelAsBytes);
    }
}

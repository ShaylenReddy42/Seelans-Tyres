using IdentityServer4.Models; // IdentityResource, IdentityResources, ApiScope, ApiResource, Client, Secret, GrantTypes, Sha256()
using System.Security.Claims; // ClaimTypes

namespace SeelansTyres.Services.IdentityService;

public static class Config
{
    // Set by the ConfigurationDataSeeder's SeedConfigurationDataAsync() method
    public static IConfiguration? Configuration { get; set; }
    
    public static List<IdentityResource> IdentityResources => new()
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResource(
            name: "role",
            displayName: "Your roles",
            userClaims: new List<string> { ClaimTypes.Role })
    };

    public static List<ApiScope> ApiScopes => new()
    {
        new ApiScope("SeelansTyresMvcBff.fullaccess"),

        new ApiScope("AddressService.fullaccess"),

        new ApiScope("CustomerService.fullaccess"),
        new ApiScope("CustomerService.createaccount"),
        new ApiScope("CustomerService.retrievesinglebyemail"),
        new ApiScope("CustomerService.resetpassword"),

        new ApiScope("OrderService.fullaccess"),

        new ApiScope("TyresService.fullaccess"),
    };

    public static List<ApiResource> ApiResources => new()
    {
        new ApiResource(
            name: "SeelansTyresMvcBff", 
            displayName: "Seelan's Tyres Mvc Bff", 
            userClaims: new[] { ClaimTypes.Role })
        {
            Scopes = { "SeelansTyresMvcBff.fullaccess" }
        },
        new ApiResource(
            name: "AddressService", 
            displayName: "Address Microservice", 
            userClaims: new[] { ClaimTypes.Role })
        {
            Scopes = { "AddressService.fullaccess" }
        },
        new ApiResource(
            name: "CustomerService",
            displayName: "Customer Microservice", 
            userClaims: new[] { ClaimTypes.Role })
        {
            Scopes = 
            { 
                "CustomerService.fullaccess",
                "CustomerService.createaccount",
                "CustomerService.retrievesinglebyemail",
                "CustomerService.resetpassword",
            }
        },
        new ApiResource(
            name: "OrderService", 
            displayName: "Order Microservice", 
            userClaims: new[] { ClaimTypes.Role })
        {
            Scopes = { "OrderService.fullaccess" }
        },
        new ApiResource(
            name: "TyresService", 
            displayName: "Tyres Microservice", 
            userClaims: new[] { ClaimTypes.Role })
        {
            Scopes = { "TyresService.fullaccess" }
        },
    };

    public static List<Client> Clients => new()
    {
        new Client
        {
            ClientId = Configuration!["Clients:SeelansTyresMvcClient:ClientId"],
            ClientName = "Seelan's Tyres Mvc Frontend",
            ClientSecrets = { new Secret(Configuration!["Clients:SeelansTyresMvcClient:ClientSecret"].Sha256()) },
            AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
            RedirectUris = { $"{Configuration!["Clients:SeelansTyresMvcClient:Url"]}/signin-oidc" },
            PostLogoutRedirectUris = { $"{Configuration!["Clients:SeelansTyresMvcClient:Url"]}/signout-callback-oidc" },
            AlwaysSendClientClaims = true,
            AllowOfflineAccess = true,
            AllowedScopes = 
            { 
                "openid", "profile", "role",
                "SeelansTyresMvcBff.fullaccess",
                "CustomerService.createaccount",
                "CustomerService.retrievesinglebyemail",
                "CustomerService.resetpassword"
            }
        },
        new Client
        {
            ClientId = Configuration!["Clients:SeelansTyresMvcBffClient:ClientId"],
            ClientName = "Seelan's Tyres Mvc Bff to Downstream",
            ClientSecrets = { new Secret(Configuration!["Clients:SeelansTyresMvcBffClient:ClientSecret"].Sha256()) },
            AllowedGrantTypes = { "urn:ietf:params:oauth:grant-type:token-exchange" },
            AllowedScopes =
            {
                "openid", "profile", "role",
                "AddressService.fullaccess",
                "CustomerService.fullaccess",
                "OrderService.fullaccess",
                "TyresService.fullaccess"
            }
        }
    };
}

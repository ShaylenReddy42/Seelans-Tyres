using IdentityServer4.Models;
using System.Security.Claims;

namespace SeelansTyres.Services.IdentityService;

public static class Config
{
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
        new ApiResource("SeelansTyresMvcBff", "Seelan's Tyres Mvc Bff", new[] { ClaimTypes.Role })
        {
            Scopes = { "SeelansTyresMvcBff.fullaccess" }
        },
        new ApiResource("AddressService", "Address Microservice", new[] { ClaimTypes.Role })
        {
            Scopes = { "AddressService.fullaccess" }
        },
        new ApiResource("CustomerService", "Customer Microservice", new[] { ClaimTypes.Role })
        {
            Scopes = 
            { 
                "CustomerService.fullaccess",
                "CustomerService.createaccount",
                "CustomerService.retrievesinglebyemail",
                "CustomerService.resetpassword",
            }
        },
        new ApiResource("OrderService", "Order Microservice", new[] { ClaimTypes.Role })
        {
            Scopes = { "OrderService.fullaccess" }
        },
        new ApiResource("TyresService", "Tyres Microservice", new[] { ClaimTypes.Role })
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

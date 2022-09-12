using IdentityServer4.Models;
using System.Security.Claims;

namespace SeelansTyres.Services.IdentityService;

public static class Config
{
    public static IConfiguration? Configuration { get; set; }
    
    public static IEnumerable<IdentityResource> IdentityResources => new IdentityResource[]
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResource(
            name: "role",
            displayName: "Your roles",
            userClaims: new List<string> { ClaimTypes.Role })
    };

    public static IEnumerable<ApiScope> ApiScopes => new ApiScope[]
    {
        new ApiScope("AddressService.fullaccess"),
        new ApiScope("CustomerService.fullaccess"),
        new ApiScope("OrderService.fullaccess"),
        new ApiScope("TyresService.fullaccess"),
    };

    public static IEnumerable<ApiResource> ApiResources => new ApiResource[]
    {
        new ApiResource("AddressService", "Address Microservice")
        {
            Scopes = { "AddressService.fullaccess" }
        },
        new ApiResource("CustomerService", "Customer Microservice")
        {
            Scopes = { "CustomerService.fullaccess" }
        },
        new ApiResource("OrderService", "Order Microservice")
        {
            Scopes = { "OrderService.fullaccess" }
        },
        new ApiResource("TyresService", "Tyres Microservice")
        {
            Scopes = { "TyresService.fullaccess" }
        },
    };

    public static IEnumerable<Client> Clients => new Client[]
    {
        new Client
        {
            ClientId = Configuration!["SeelansTyresMvcClient:ClientId"],
            ClientName = "Seelan's Tyres Mvc Frontend",
            ClientSecrets = { new Secret(Configuration!["SeelansTyresMvcClient:ClientSecret"].Sha256()) },
            AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
            RedirectUris = { $"{Configuration!["SeelansTyresMvcClient:Url"]}/signin-oidc" },
            PostLogoutRedirectUris = { $"{Configuration!["SeelansTyresMvcClient:Url"]}/signout-callback-oidc" },
            AlwaysSendClientClaims = true,
            AllowOfflineAccess = true,
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

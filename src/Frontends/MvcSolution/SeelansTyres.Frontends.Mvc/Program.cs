using SeelansTyres.Frontends.Mvc.Services;               // All services
using System.Net;                                        // NetworkCredential
using System.Net.Mail;                                   // SmtpClient
using static System.Net.Mime.MediaTypeNames;             // Application
using Microsoft.AspNetCore.Authentication.Cookies;       // CookieAuthenticationDefaults
using Microsoft.AspNetCore.Authentication.OpenIdConnect; // OpenIdConnectDefaults
using Microsoft.AspNetCore.Authentication;               // MapUniqueJsonKey()
using System.Security.Claims;                            // ClaimTypes
using Microsoft.Extensions.Diagnostics.HealthChecks;     // HealthStatus
using SeelansTyres.Frontends.Mvc.BackgroundServices;     // SendReceiptChannelReaderBackgroundService
using SeelansTyres.Frontends.Mvc.Channels;               // SendReceiptChannel
using SeelansTyres.Libraries.Shared.Extensions;          // ConditionallyUseAzureAppConfiguration(), AddCommonStartupDelay()
using SeelansTyres.Frontends.Mvc.HttpClients;            // All strongly-typed http clients
using SeelansTyres.Libraries.Shared.Abstractions;        // All common methods
using SeelansTyres.Libraries.Shared.Configuration;       // ExternalServiceOptions

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Mvc Frontend"
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddOpenIdConnectAccessTokenManagement();

builder.Services.AddHttpClient<IAddressServiceClient, AddressServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AddressService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler()
    .AddCommonResiliencyPolicies<AddressServiceClient>(builder.Services);

builder.Services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler()
    .AddCommonResiliencyPolicies<CustomerServiceClient>(builder.Services);

builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler()
    .AddCommonResiliencyPolicies<OrderServiceClient>(builder.Services);

builder.Services.AddHttpClient<ITyresServiceClient, TyresServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TyresService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler()
    .AddCommonResiliencyPolicies<TyresServiceClient>(builder.Services);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

var redisOptions =
    builder.Configuration.GetSection("Redis")
        .Get<ExternalServiceOptions>()
            ?? throw new InvalidOperationException("Redis configuration section is missing");

if (!redisOptions.Enabled)
{
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<ICacheService, InMemoryCacheService>(); 
}
else
{
    builder.Services.AddStackExchangeRedisCache(setup =>
    {
        setup.Configuration = redisOptions.ConnectionString;
        setup.InstanceName = "seelanstyres_";
    });
    builder.Services.AddScoped<ICacheService, DistributedCacheService>();
}

builder.Services.AddScoped<ICartService, CartService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddTransient<IImageService, LocalImageService>();
}
else
{
    builder.Services.AddTransient<IImageService, CloudImageService>();
}

builder.Services.AddFluentEmail(
        builder.Configuration["EmailCredentials:Email"], "Seelan's Tyres")
    .AddRazorRenderer()
    .AddSmtpSender(
        new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            Credentials = 
                new NetworkCredential(
                    builder.Configuration["EmailCredentials:Email"],
                    builder.Configuration["EmailCredentials:Password"])
        });

builder.Services.AddScoped<IMailService, MailService>();

builder.Services.AddSingleton<SendReceiptChannel>();
builder.Services.AddHostedService<SendReceiptChannelReaderBackgroundService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration["ClientCredentials:ClientId"];
        options.ClientSecret = builder.Configuration["ClientCredentials:ClientSecret"];

        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Authority = builder.Configuration["IdentityServer"];
        options.RequireHttpsMetadata = false;
        
        options.ResponseType = "code";
        options.SaveTokens = true;
        
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Add("SeelansTyresWebBff.fullaccess");
        options.Scope.Add("offline_access");
        options.Scope.Add("role");

        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Role, ClaimTypes.Role);
    });

builder.Services.AddHealthChecks()
    .AddCommonIdentityServerCheck(builder.Configuration["IdentityServer"]!)
    .AddUrlGroup(
        uri: new($"{builder.Configuration["WebBffUrl"]}{builder.Configuration["LivenessCheckEndpoint"]}"),
        name: "Web Backend-For-Frontend",
        failureStatus: HealthStatus.Unhealthy);

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHealthChecks()
        .AddAzureBlobStorage(
            connectionString: builder.Configuration.GetConnectionString("AzureStorageAccount")!,
            name: "Azure Storage",
            failureStatus: HealthStatus.Unhealthy);
}

if (builder.Configuration.GetValue<bool>("Redis:Enabled"))
{
    builder.Services.AddHealthChecks()
        .AddRedis(
            redisConnectionString: builder.Configuration["Redis:ConnectionString"]!,
            name: "Redis",
            failureStatus: HealthStatus.Unhealthy);
}

var app = builder.Build();

app.HonorForwardedHeaders();

app.UseCommonCookiePolicy();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.ConditionallyUseAzureAppConfiguration();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapDefaultControllerRoute();

app.MapCommonHealthChecks();

app.AddCommonStartupDelay();

await app.RunAsync();

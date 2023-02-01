using SeelansTyres.Frontends.Mvc.Services;
using System.Net;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using SeelansTyres.Libraries.Shared;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SeelansTyres.Frontends.Mvc.BackgroundServices;
using SeelansTyres.Frontends.Mvc.Channels;
using SeelansTyres.Libraries.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    KestrelLocalhostPortNumber = 5001,
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Mvc Frontend"
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAccessTokenManagement();

builder.Services.AddHttpClient<IAddressService, AddressService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AddressService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler()
    .AddCommonResiliencyPolicies<AddressService>(builder.Services);

builder.Services.AddHttpClient<ICustomerService, CustomerService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler()
    .AddCommonResiliencyPolicies<CustomerService>(builder.Services);

builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler()
    .AddCommonResiliencyPolicies<OrderService>(builder.Services);

builder.Services.AddHttpClient<ITyresService, TyresService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TyresService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler()
    .AddCommonResiliencyPolicies<TyresService>(builder.Services);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

if (builder.Configuration.GetValue<bool>("InContainer") is false)
{
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<ICacheService, InMemoryCacheService>(); 
}
else
{
    builder.Services.AddStackExchangeRedisCache(setup =>
    {
        setup.Configuration = builder.Configuration.GetConnectionString("Redis");
        setup.InstanceName = "seelanstyres_";
    });
    builder.Services.AddScoped<ICacheService, DistributedCacheService>();
}

builder.Services.AddScoped<ICartService, CartService>();

if (builder.Environment.IsDevelopment() is true)
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

        options.Scope.Add("SeelansTyresMvcBff.fullaccess");
        options.Scope.Add("offline_access");
        options.Scope.Add("role");

        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Role, ClaimTypes.Role);
    });

builder.Services.AddHealthChecks()
    .AddCommonIdentityServerCheck(builder.Configuration["IdentityServer"]!)
    .AddUrlGroup(
        uri: new($"{builder.Configuration["MvcBffUrl"]}{builder.Configuration["LivenessCheckEndpoint"]}"),
        name: "gateway",
        failureStatus: HealthStatus.Unhealthy);

if (builder.Environment.IsDevelopment() is false)
{
    builder.Services.AddHealthChecks()
        .AddAzureBlobStorage(
            connectionString: builder.Configuration.GetConnectionString("AzureStorageAccount")!,
            name: "azureStorageAccount",
            failureStatus: HealthStatus.Unhealthy);
}

if (builder.Configuration.GetValue<bool>("InContainer") is true)
{
    builder.Services.AddHealthChecks()
        .AddRedis(
            redisConnectionString: builder.Configuration.GetConnectionString("Redis")!,
            name: "redis",
            failureStatus: HealthStatus.Unhealthy);
}

var app = builder.Build();

app.HonorForwardedHeaders();

app.UseCommonCookiePolicy();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() is false)
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

app.Run();

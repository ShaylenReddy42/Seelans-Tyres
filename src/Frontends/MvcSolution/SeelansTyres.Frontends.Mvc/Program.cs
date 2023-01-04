using SeelansTyres.Frontends.Mvc.Services;
using System.Net;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using SeelansTyres.Libraries.Shared.Models;
using SeelansTyres.Libraries.Shared;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SeelansTyres.Frontends.Mvc.BackgroundServices;
using SeelansTyres.Frontends.Mvc.Channels;
using Microsoft.AspNetCore.HttpOverrides;

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
    .AddUserAccessTokenHandler();

builder.Services.AddHttpClient<ICustomerService, CustomerService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler();

builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler();

builder.Services.AddHttpClient<ITyresService, TyresService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TyresService"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddTransient<IImageService, LocalImageService>();

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

var healthChecksModel = new HealthChecksModel
{
    EnableElasticsearchHealthCheck = builder.Configuration.GetValue<bool>("LoggingSinks:Elasticsearch:Enabled"),
    ElasticsearchUrl = builder.Configuration["LoggingSinks:Elasticsearch:Url"]!,

    PublishHealthStatusToAppInsights = builder.Configuration.GetValue<bool>("AppInsights:Enabled")
};

builder.Services.AddHealthChecks()
    .AddCommonChecks(healthChecksModel)
    .AddIdentityServer(
        idSvrUri: new(builder.Configuration["IdentityServer"]!),
        name: "identityServer",
        failureStatus: HealthStatus.Unhealthy)
    .AddUrlGroup(
        uri: new($"{builder.Configuration["MvcBffUrl"]}{builder.Configuration["LivenessCheckEndpoint"]}"),
        name: "gateway",
        failureStatus: HealthStatus.Unhealthy);

var app = builder.Build();

app.HonorForwardedHeaders();

app.UseCommonCookiePolicy();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() is false)
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapDefaultControllerRoute();

app.MapCommonHealthChecks();

app.Run();

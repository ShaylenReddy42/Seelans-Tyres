using ConfigurationSubstitution;
using SeelansTyres.Frontends.Mvc.Services;
using System.Net;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Serilog.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", " ?? ");

builder.Logging.ClearProviders();

var assembly = typeof(Program).Assembly;

builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(hostBuilderContext.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .Enrich.WithProperty("Application Name", hostBuilderContext.HostingEnvironment.ApplicationName)
        .Enrich.WithProperty("Descriptive Application Name", assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product)
        .Enrich.WithProperty("Codebase Version", $"v{assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}")
        .WriteTo.Console();

    var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToList();

    metadata.ForEach(attribute => loggerConfiguration.Enrich.WithProperty(attribute.Key, attribute.Value));

    if (hostBuilderContext.Configuration.GetValue<bool>("LoggingSinks:Elasticsearch:Enabled") is true)
    {
        loggerConfiguration
            .WriteTo.Elasticsearch(
                new ElasticsearchSinkOptions(new Uri(hostBuilderContext.Configuration["LoggingSinks:Elasticsearch:Url"]))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = "seelanstyres-logs-{0:yyyy.MM.dd}",
                    MinimumLogEventLevel = LogEventLevel.Debug
                });
    }
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAccessTokenManagement();

builder.Services.AddHttpClient<IAddressService, AddressService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AddressService"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler();

builder.Services.AddHttpClient<ICustomerService, CustomerService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerService"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler();

builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderService"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler();

builder.Services.AddHttpClient<ITyresService, TyresService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TyresService"]);
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
builder.Services.AddScoped<IEmailService, EmailService>();

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
        options.Authority = builder.Configuration["IdentityServerUrl"];
        options.RequireHttpsMetadata = false;
        
        options.ResponseType = "code";
        options.SaveTokens = true;
        
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Add("SeelansTyresMvcBff.fullaccess");
        options.Scope.Add("offline_access");
        options.Scope.Add("role");

        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Role, ClaimTypes.Role);
    });

var app = builder.Build();

app.Urls.Clear();
app.Urls.Add("https://localhost:5001");
app.Urls.Add("http://localhost:4001");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() is false)
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapDefaultControllerRoute();

app.Run();

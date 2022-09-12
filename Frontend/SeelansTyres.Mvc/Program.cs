using SeelansTyres.Mvc.Services;
using System.Net;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAccessTokenManagement();

builder.Services.AddHttpClient<IAddressService, AddressService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AddressServiceApi"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler();

builder.Services.AddHttpClient<ICustomerService, CustomerService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CustomerServiceApi"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler();

builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["OrderServiceApi"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddUserAccessTokenHandler();

builder.Services.AddHttpClient<ITyresService, TyresService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TyresServiceApi"]);
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

        options.Scope.Add("AddressService.fullaccess");
        options.Scope.Add("CustomerService.fullaccess");
        options.Scope.Add("OrderService.fullaccess");
        options.Scope.Add("TyresService.fullaccess");

        options.Scope.Add("offline_access");
        options.Scope.Add("role");

        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Role, ClaimTypes.Role);
    });

var app = builder.Build();

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

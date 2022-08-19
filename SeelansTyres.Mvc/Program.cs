using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.Entities;
using SeelansTyres.Mvc.Data;
using SeelansTyres.Mvc.Services;
using System.Net;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SeelansTyresContext>(
    options => options.UseSqlServer(
        builder.Configuration["ConnectionStrings:SeelansTyresContext"]));

builder.Services.AddIdentity<Customer, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SeelansTyresContext>()
    .AddDefaultTokenProviders();

// i'm not all that strict with this
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 0;

    options.User.RequireUniqueEmail = true;
});

builder.Services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebApiUrl"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
});

builder.Services.AddHttpClient<IAddressService, AddressService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebApiUrl"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
});

builder.Services.AddHttpClient<ICartService, CartService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebApiUrl"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
});

builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebApiUrl"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
});

builder.Services.AddHttpClient<ITyresService, TyresService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebApiUrl"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

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

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.Urls.Clear();
    app.Urls.Add("http://localhost:4200");
    app.Urls.Add("https://localhost:4201");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

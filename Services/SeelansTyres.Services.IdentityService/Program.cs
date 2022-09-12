using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rsk.TokenExchange.IdentityServer4;
using SeelansTyres.Services.IdentityService.Authorization;
using SeelansTyres.Services.IdentityService.Data;
using SeelansTyres.Services.IdentityService.Data.Entities;
using SeelansTyres.Services.IdentityService.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

var connectionString = builder.Configuration["SeelansTyresIdentityContext"];
var assemblyName = typeof(Program).Assembly.GetName().Name;

try
{
    Log.Information("Starting host...");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly.");
}

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CustomerContext>(options =>
{
    options.UseSqlServer(connectionString, options => options.MigrationsAssembly(assemblyName));
});

builder.Services.AddIdentity<Customer, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<CustomerContext>()
    .AddRoles<IdentityRole<Guid>>()
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

builder.Services.AddScoped<AdminAccountSeeder>();
builder.Services.AddScoped<ConfigurationDataSeeder>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    options.EmitStaticAudienceClaim = true;
})
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = builder => 
            builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(assemblyName));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = builder => 
            builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(assemblyName));

        options.EnableTokenCleanup = true;
    })
    .AddAspNetIdentity<Customer>()
    .AddTokenExchange()
    .AddDeveloperSigningCredential();

builder.Services.AddAuthentication()
    .AddJwtBearer(configure =>
    {
        configure.Authority = "https://localhost:5005";
        configure.Audience = "CustomerService";
    });

builder.Services.AddTransient<IAuthorizationHandler, CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerIdFromClaimsMustMatchCustomerIdFromRoute", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement());
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.Urls.Clear();

app.Urls.Add("https://localhost:5005");
app.Urls.Add("https://localhost:4005");

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

app.UseIdentityServer();

app.UseAuthorization();

app.MapDefaultControllerRoute();

await RunSeeders();

app.Run();

async Task RunSeeders()
{
    using var scope = app.Services.CreateScope();

    var configurationDbContext = scope.ServiceProvider.GetService<ConfigurationDbContext>();
    var persistedGrantDbContext = scope.ServiceProvider.GetService<PersistedGrantDbContext>();
    var customerContext = scope.ServiceProvider.GetService<CustomerContext>();

    configurationDbContext!.Database.Migrate();
    persistedGrantDbContext!.Database.Migrate();
    customerContext!.Database.Migrate();

    var adminAccountSeeder = scope.ServiceProvider.GetService<AdminAccountSeeder>();
    await adminAccountSeeder!.CreateAdminAsync();

    var configurationDataSeeder = scope.ServiceProvider.GetService<ConfigurationDataSeeder>();
    await configurationDataSeeder!.SeedConfigurationDataAsync();
}

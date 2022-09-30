using ConfigurationSubstitution;
using Hellang.Middleware.ProblemDetails;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SeelansTyres.Services.IdentityService.Authorization;
using SeelansTyres.Services.IdentityService.Data;
using SeelansTyres.Services.IdentityService.Data.Entities;
using SeelansTyres.Services.IdentityService.Services;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;
using System.Security.Cryptography;

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
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", 
            theme: AnsiConsoleTheme.Code);

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

builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    setup.AddSecurityDefinition("CustomerServiceAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Input a valid token to access this API"
    });

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "CustomerServiceAuth"
            }
        },
        new List<string>()
    }});
});

var connectionString = builder.Configuration["SeelansTyresIdentityContext"];
var assemblyName = assembly.GetName().Name;

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

var rsaSecurityKey = new RsaSecurityKey(
    new RSAParameters
    {
        D = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:D"]),
        DP = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:DP"]),
        DQ = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:DQ"]),
        Exponent = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:Exponent"]),
        InverseQ = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:InverseQ"]),
        Modulus = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:Modulus"]),
        P = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:P"]),
        Q = Base64UrlEncoder.DecodeBytes(builder.Configuration["RSAParameters:Q"])
    });

var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

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
    .AddExtensionGrantValidator<TokenExchangeExtensionGrantValidator>()
    .AddSigningCredential(signingCredentials);

builder.Services.AddAuthentication()
    .AddJwtBearer(configure =>
    {
        configure.Authority = builder.Configuration["BaseUrl"];
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

    options.AddPolicy("CreateAccountPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "CustomerService.createaccount");
    });

    options.AddPolicy("RetrieveSingleByEmailPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "CustomerService.retrievesinglebyemail");
    });

    options.AddPolicy("ResetPasswordPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "CustomerService.resetpassword");
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddProblemDetails(configure =>
{
    configure.IncludeExceptionDetails = (httpContext, exception) => false;
});

var app = builder.Build();

app.UseProblemDetails();

app.Urls.Clear();
app.Urls.Add("https://localhost:5005");
app.Urls.Add("http://localhost:4005");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() is false)
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();

app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Logger.LogInformation("Program => Migrating and seeding databases");

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

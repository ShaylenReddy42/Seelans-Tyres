using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Services.IdentityService.Authorization;
using SeelansTyres.Services.IdentityService.Data;
using SeelansTyres.Services.IdentityService.Data.Entities;
using SeelansTyres.Services.IdentityService.Services;
using System.Reflection;
using System.Security.Cryptography;
using SeelansTyres.Services.IdentityService.Extensions;
using SeelansTyres.Libraries.Shared.Services;
using SeelansTyres.Libraries.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    KestrelLocalhostPortNumber = 5005,
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Identity / Customer Microservice"
});

builder.Services.AddControllersWithViews();

builder.Services.AddCommonSwaggerGen();

var connectionString = builder.Configuration["Database:ConnectionString"];
var assemblyName = typeof(Program).Assembly.GetName().Name;

builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    options.UseSqlServer(connectionString, options => options.MigrationsAssembly(assemblyName));
});

builder.Services.AddIdentity<Customer, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<CustomerDbContext>()
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

    options.Authentication.CookieSameSiteMode = SameSiteMode.None;
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
        configure.RequireHttpsMetadata = false;
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

builder.Services.AddHealthChecks()
    .AddCommonDbContextCheck<CustomerDbContext>();

if (builder.Environment.IsDevelopment() is true)
{
    builder.Services.AddHealthChecks()
        .AddCommonRabbitMQCheck(builder.Configuration["RabbitMQ:ConnectionProperties:ConnectionString"]!);
    
    builder.Services.AddUnpublishedUpdatesManagement<RabbitMQPublisher>(
        databaseConnectionString: connectionString);
}
else
{
    builder.Services.AddHealthChecks()
        .AddCommonAzureServiceBusTopicCheck(
            connectionString: builder.Configuration["AzureServiceBus:ConnectionString"]!,
            topicName: builder.Configuration["AzureServiceBus:Topics:DeleteAccount"]!)
        .AddCommonAzureServiceBusTopicCheck(
            connectionString: builder.Configuration["AzureServiceBus:ConnectionString"]!,
            topicName: builder.Configuration["AzureServiceBus:Topics:UpdateAccount"]!);

    builder.Services.AddUnpublishedUpdatesManagement<AzureServiceBusPublisher>(
        databaseConnectionString: connectionString);
}

var app = builder.Build();

app.HonorForwardedHeaders();

app.UseCommonCookiePolicy();

app.UseProblemDetails();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() is false)
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseCommonSwagger();
}

app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();

app.UseAuthorization();

app.MapDefaultControllerRoute();

app.MapCommonHealthChecks();

app.AddCommonStartupDelay();

app.Logger.LogInformation("Program => Migrating and seeding databases");

await app.RunSeedersAsync();

app.Run();

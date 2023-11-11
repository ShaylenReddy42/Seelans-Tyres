using Hellang.Middleware.ProblemDetails;                   // UseProblemDetails()
using Microsoft.AspNetCore.Authorization;                  // IAuthorizationHandler
using Microsoft.AspNetCore.Identity;                       // IdentityRole, AddDefaultTokenProviders()
using Microsoft.EntityFrameworkCore;                       // UseSqlServer()
using SeelansTyres.Libraries.Shared;                       // All common methods
using SeelansTyres.Services.IdentityService.Data;          // CustomerDbContext
using SeelansTyres.Services.IdentityService.Data.Entities; // Customer
using SeelansTyres.Services.IdentityService.Services;      // AdminAccountSeeder, ConfigurationDataSeeder, ICustomerService, CustomerService, TokenExchangeExtensionGrantValidator
using System.Reflection;                                   // Assembly
using SeelansTyres.Services.IdentityService.Extensions;    // GenerateSigningCredentialsFromConfiguration(), RunSeedersAsync()
using SeelansTyres.Libraries.Shared.Services;              // RabbitMQPublisher, AzureServiceBusPublisher
using SeelansTyres.Libraries.Shared.Extensions;            // AddCommonStartupDelay()
using SeelansTyres.Libraries.Shared.Authorization;         // CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler, CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement()
using SeelansTyres.Libraries.Shared.Abstractions;          // All health check abstractions

var descriptiveApplicationName = "Seelan's Tyres: Identity / Customer Microservice";

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = descriptiveApplicationName
});

builder.Services.AddControllersWithViews();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    setup.AddCommonSwaggerSecurityDefinitions();
    setup.AddCommonSwaggerDoc(descriptiveApplicationName, "This API allows you to manage customers");

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    setup.IncludeXmlComments(xmlFilePath);
});

var connectionString = builder.Configuration["Database:ConnectionString"];
var assemblyName = typeof(Program).Assembly.GetName().Name;

builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    options.UseSqlServer(
        connectionString, 
        options =>
        {
            options.MigrationsAssembly(assemblyName);
            options.EnableRetryOnFailure(maxRetryCount: 5);
        });
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
            builder.UseSqlServer(
                connectionString, 
                options =>
                {
                    options.MigrationsAssembly(assemblyName);
                    options.EnableRetryOnFailure(maxRetryCount: 5);
                });
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = builder => 
            builder.UseSqlServer(
                connectionString, 
                options => 
                { 
                    options.MigrationsAssembly(assemblyName);
                    options.EnableRetryOnFailure(maxRetryCount: 5);
                });

        options.EnableTokenCleanup = true;
    })
    .AddAspNetIdentity<Customer>()
    .AddExtensionGrantValidator<TokenExchangeExtensionGrantValidator>()
    .AddSigningCredential(builder.GenerateSigningCredentialsFromConfiguration());

builder.Services.AddAuthentication()
    .AddJwtBearer(configure =>
    {
        configure.Authority = builder.Configuration["BaseUrl"];
        configure.Audience = "CustomerService";
        configure.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        configure.RequireHttpsMetadata = false;
    });

builder.Services.AddTransient<IAuthorizationHandler, CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerIdFromClaimsMustMatchCustomerIdFromRoute", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement("id"));
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

if (builder.Environment.IsDevelopment())
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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseCommonSwagger(descriptiveApplicationName);
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

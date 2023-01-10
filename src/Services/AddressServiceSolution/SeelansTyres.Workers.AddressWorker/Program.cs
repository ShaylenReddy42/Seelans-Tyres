using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SeelansTyres.Data.AddressData;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Libraries.Shared.Services;
using SeelansTyres.Workers.AddressWorker.BackgroundServices;
using SeelansTyres.Workers.AddressWorker.Services;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    KestrelLocalhostPortNumber = 5021,
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Address Worker Service"
});

builder.Services.AddDbContext<AddressDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["Database:ConnectionString"]!));

builder.Services.AddScoped<IAddressUpdateService, AddressUpdateService>();

if (builder.Environment.IsDevelopment() is true)
{
    builder.Services.AddHostedService<DeleteAccountWorkerWithRabbitMQ>();
}
else
{
    builder.Services.AddHostedService<DeleteAccountWorkerWithAzureServiceBus>();
}

builder.Services.AddHttpClient<ITokenValidationService, TokenValidationService>(client =>
{
    client.BaseAddress = new(builder.Configuration["IdentityServer"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
});

builder.Services.AddHealthChecks()
    .AddCommonDbContextCheck<AddressDbContext>()
    .AddCommonIdentityServerCheck(builder.Configuration["IdentityServer"]!)
    .AddRabbitMQ(
        name: "rabbitmq",
        rabbitConnectionString: builder.Configuration["RabbitMQ:ConnectionProperties:ConnectionString"]!,
        failureStatus: HealthStatus.Degraded);

var app = builder.Build();

app.MapCommonHealthChecks();

app.Run();

using SeelansTyres.Libraries.Shared;            // AddCommonBuilderConfiguration(), HonorForwardedHeaders()
using SeelansTyres.Libraries.Shared.Extensions; // AddCommonStartupDelay()

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Health Checks UI"
});

builder.Services.AddHealthChecksUI().AddInMemoryStorage();

var app = builder.Build();

app.HonorForwardedHeaders();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/";
    options.AsideMenuOpened = false;
});

app.AddCommonStartupDelay();

app.Run();

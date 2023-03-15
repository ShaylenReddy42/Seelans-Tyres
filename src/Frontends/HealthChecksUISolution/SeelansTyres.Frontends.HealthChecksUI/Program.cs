using SeelansTyres.Libraries.Shared;
using SeelansTyres.Libraries.Shared.Extensions;

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

using SeelansTyres.Libraries.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    KestrelLocalhostPortNumber = 5000,
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Health Checks UI"
});

builder.Services.AddHealthChecksUI().AddInMemoryStorage();

var app = builder.Build();

app.UseRouting();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/";
    options.AsideMenuOpened = false;
});

app.Run();

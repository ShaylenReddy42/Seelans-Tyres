using SeelansTyres.Libraries.Shared;
using SeelansTyres.Libraries.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

var commonBuilderConfigurationBuilderModel = new CommonBuilderConfigurationModel
{
    KestrelLocalhostPortNumber = 5000,
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Health Checks UI"
};

builder.AddCommonBuilderConfiguration(commonBuilderConfigurationBuilderModel);

builder.Services.AddHealthChecksUI().AddInMemoryStorage();

var app = builder.Build();

app.UseRouting();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/";
    options.AsideMenuOpened = false;
});

app.Run();

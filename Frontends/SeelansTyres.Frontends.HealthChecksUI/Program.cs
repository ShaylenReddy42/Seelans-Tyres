using ConfigurationSubstitution;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Libraries.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration.GetValue<bool>("UseDocker") is false)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5000);
    });
}

builder.Configuration.EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", " ?? ");

builder.Logging.ClearProviders();

var assembly = typeof(Program).Assembly;

var serilogModel = new SerilogModel
{
    Assembly = assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Health Checks UI"
};

builder.Host.UseCommonSerilog(serilogModel);

builder.Services.AddHealthChecksUI().AddInMemoryStorage();

var app = builder.Build();

app.UseRouting();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/";
    options.AsideMenuOpened = false;
});

app.Run();

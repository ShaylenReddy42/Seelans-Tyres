using ConfigurationSubstitution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SeelansTyres.Libraries.Shared.Models;

namespace SeelansTyres.Libraries.Shared;

public static class WebApplicationBuilderConfiguration
{
    public static WebApplicationBuilder AddCommonBuilderConfiguration(
        this WebApplicationBuilder builder,
        CommonBuilderConfigurationModel commonBuilderConfigurationModel)
    {
        if (builder.Configuration.GetValue<bool>("InContainer") is false)
        {
            builder.WebHost.ConfigureKestrel(options => 
                options.ListenLocalhost(
                    commonBuilderConfigurationModel.KestrelLocalhostPortNumber));
        }

        builder.Configuration.EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", " ?? ");

        builder.Logging.ClearProviders();

        builder.Host.UseCommonSerilog(commonBuilderConfigurationModel);

        return builder;
    }
}

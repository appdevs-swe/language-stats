using CollectStats_Functions;
using CollectStats_Functions.Models;
using CollectStats_Functions.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace CollectStats_Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<AppOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("AppOptions").Bind(settings);
            });

            builder.Services.AddTransient<IGetStats<GithubLanguageStat>, GithubService>();
            builder.Services.AddTransient<IGetDependencies<GithubDependency>, GithubService>();
            builder.Services.AddTransient<IGetStats<AzdoLanguageStat>, AzDOService>();
        }
    }
}

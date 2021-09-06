using CollectStats_Functions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectStats_Functions.Services
{
    public class GithubService : IGetStats<GithubLanguageStat>
    {
        private readonly AppOptions _appOptions;
        private readonly ILogger<GithubService> _logger;

        public GithubService(IOptions<AppOptions> options, ILogger<GithubService> logger)
        {
            _appOptions = options.Value;
            _logger = logger;
        }

        private string GetJwtFromApp()
        {
            // Use GitHubJwt library to create the GitHubApp Jwt Token using our private certificate PEM file
            var generator = new GitHubJwt.GitHubJwtFactory(
                new GitHubJwt.StringPrivateKeySource(_appOptions.GithubAppPrivateKey
                ),
                new GitHubJwt.GitHubJwtFactoryOptions
                {
                    AppIntegrationId = _appOptions.GithubAppId, // The GitHub App Id
                    ExpirationSeconds = 600 // 10 minutes is the maximum time allowed
                });
            ;
            return generator.CreateEncodedJwtToken();
        }

        private async Task<GitHubClient> GetInstallationClient()
        {
            var jwtToken = GetJwtFromApp();
            // Use the JWT as a Bearer token
            var appClient = new GitHubClient(new ProductHeaderValue("local-app"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var response = await appClient.GitHubApps.CreateInstallationToken(_appOptions.GithubAppInstallationId);

            // Create a new GitHubClient using the installation token as authentication
            var installationClient = new GitHubClient(new ProductHeaderValue($"local-Installation-{_appOptions.GithubAppInstallationId}"))
            {
                Credentials = new Credentials(response.Token)
            };

            return installationClient;
        }

        public async Task<IEnumerable<GithubLanguageStat>> GetStats(string org)
        {
            var appClient = await GetInstallationClient();
            var results = new List<GithubLanguageStat>();
            foreach (var repo in await appClient.Repository.GetAllForOrg(org))
            {
                var languages = await appClient.Repository.GetAllLanguages(org, repo.Name);
                var stats = languages.Select(l => new GithubLanguageStat
                {
                    Repository = repo.Name,
                    LanguageBytes = l.NumberOfBytes,
                    Organization = org,
                    LanguageName = l.Name,
                    Date = DateTimeOffset.UtcNow
                });
                results.AddRange(stats);
            }

            return results;
        }
    }
}

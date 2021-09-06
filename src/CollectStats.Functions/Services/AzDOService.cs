using CollectStats_Functions.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CollectStats_Functions.Services
{
    public class AzDOService : IGetFromAzdo
    {
        private readonly AppOptions _appOptions;
        private readonly ILogger<AzDOService> _logger;

        public AzDOService(IOptions<AppOptions> options, ILogger<AzDOService> logger)
        {
            _appOptions = options.Value;
            _logger = logger;
        }

        //https://github.com/microsoft/azure-devops-auth-samples/blob/master/ManagedClientConsoleAppSample/Program.cs#L19
        // constant ResourceId for Azure DevOps, go figure
        private static string Resource = "499b84ac-1321-427f-aa17-267ca6975798";

        private async Task<string> GetAccessToken(string providerString, string tenant)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider(providerString);
            return await azureServiceTokenProvider.GetAccessTokenAsync(Resource, tenant);
        }

        private async Task<IEnumerable<string>> GetProjects(string accessToken, string orgUrl)
        {
            var connection = new VssConnection(new Uri(orgUrl), new VssOAuthAccessTokenCredential(accessToken));
            using var client = connection.GetClient<ProjectHttpClient>();
            var projects = await client.GetProjects();
            return projects.Select(a => a.Name);
        }

        private async Task<LanguageDistributionResponse> FetchProjectStats(HttpClient client, string orgUrl, LanguageDistributionRequest body)
        {
            var project = body.dataProviderContext.properties.sourcePage.routeValues.project;
            var url = $"{orgUrl}/_apis/Contribution/HierarchyQuery/project/{project}?api-version=6.1-preview";
            var response = await client.PostAsJsonAsync(url, body);
            return await response.Content.ReadAsAsync<LanguageDistributionResponse>();
        }

        public async Task<List<AzdoLanguageStat>> GetLanguageStats(string organization)
        {
            var accessToken = await GetAccessToken(_appOptions.AzureTokenProviderString, _appOptions.TenantId);
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var stats = new List<AzdoLanguageStat>();
            var now = DateTimeOffset.UtcNow;
            // Get All Projects for Organization
            var orgUrl = $"https://dev.azure.com/{organization}";
            var projectNames = await GetProjects(accessToken, orgUrl);
            // Get Stats for Project
            foreach (var project in projectNames)
            {
                var body = LanguageDistributionRequest.Create(project);
                var projectStats = await FetchProjectStats(client, orgUrl, body);
                var languages = projectStats.dataProviders?
                                            .ProjectLanguagesDataProvider?
                                            .projectLanguages?
                                            .languages?.Select(l => new AzdoLanguageStat(organization, project, l.name, l.languagePercentage, now));
                if (languages != null) stats.AddRange(languages);
            }
            return stats;
        }
    }

    public interface IGetFromAzdo
    {
        Task<List<AzdoLanguageStat>> GetLanguageStats(string organization);
    }
}

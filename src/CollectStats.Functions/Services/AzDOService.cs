using CollectStats_Functions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;


namespace CollectStats_Functions.Services
{
    public class AzDOService : IGetStats<AzdoLanguageStat>
    {
        private readonly AppOptions _appOptions;
        private readonly ILogger<AzDOService> _logger;
        private readonly string ApiVersion = "6.1-preview";

        public AzDOService(IOptions<AppOptions> options, ILogger<AzDOService> logger)
        {
            _appOptions = options.Value;
            _logger = logger;
        }

        private async Task<AzdoProjects> GetProjects(HttpClient client, string orgUrl)
        {
            var url = $"{orgUrl}/_apis/projects?api-version={ApiVersion}";
            var response = await client.GetAsync(url);
            return await response.Content.ReadAsAsync<AzdoProjects>();
        }

        private async Task<LanguageDistributionResponse> FetchProjectStats(HttpClient client, string orgUrl, LanguageDistributionRequest body)
        {
            var project = body.dataProviderContext.properties.sourcePage.routeValues.project;
            var url = $"{orgUrl}/_apis/Contribution/HierarchyQuery/project/{project}?api-version={ApiVersion}";
            var response = await client.PostAsJsonAsync(url, body);
            return await response.Content.ReadAsAsync<LanguageDistributionResponse>();
        }

        public async Task<IEnumerable<AzdoLanguageStat>> GetStats(string organization)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    ASCIIEncoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", "", _appOptions.AzdoPAT))));

            var stats = new List<AzdoLanguageStat>();
            var now = DateTimeOffset.UtcNow;
            // Get All Projects for Organization
            var orgUrl = $"https://dev.azure.com/{organization}";
            var projects = await GetProjects(client, orgUrl);
            // Get Stats for Project
            foreach (var project in projects.value.Select(a=>a.name))
            {
                var body = LanguageDistributionRequest.Create(project);
                var projectStats = await FetchProjectStats(client, orgUrl, body);
                var languages = projectStats.dataProviders?
                                            .ProjectLanguagesDataProvider?
                                            .projectLanguages?
                                            .languages?.Select(l => new AzdoLanguageStat
                                            {
                                                Organization = organization,
                                                ProjectName = project,
                                                LanguageName = l.name,
                                                LanguagePercentage = l.languagePercentage,
                                                Date = now
                                            });
                if (languages != null) stats.AddRange(languages);
            }
            return stats;
        }
    }
    public interface IGetStats<T>
    {
        Task<IEnumerable<T>> GetStats(string organization);
    }

    public interface IGetDependencies<T>
    {
        Task<IEnumerable<T>> GetDependencies(string organization);
    }
}

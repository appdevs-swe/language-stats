using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CollectStats_Functions;
using Microsoft.Extensions.Options;
using CollectStats_Functions.Services;
using CollectStats_Functions.Models;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;

namespace CollectStats.Functions
{
    public class CollectAzdoStats
    {
        private readonly AppOptions _appOptions;
        private readonly ILogger<CollectAzdoStats> _logger;

        public CollectAzdoStats(IOptions<AppOptions> options, ILogger<CollectAzdoStats> logger)
        {
            _appOptions = options.Value;
            _logger = logger;
        }

        [FunctionName("CollectAzdoStats")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var accessToken = await AzDO.GetAccessToken(_appOptions.AzureTokenProviderString, _appOptions.TenantId);
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var stats = new List<AzdoLanguageStat>();
            var now = DateTimeOffset.UtcNow;
            var org = _appOptions.Organization;
            // Get All Projects for Organization
            var orgUrl = $"https://dev.azure.com/{org}";
            var projectNames = await AzDO.GetProjects(accessToken, orgUrl);
            // Get Stats for Project
            foreach (var project in projectNames)
            {
                var body = LanguageDistributionRequest.Create(project);
                var projectStats = await AzDO.GetLanguageStats(client, orgUrl, body);
                var languages = projectStats.dataProviders?
                                            .ProjectLanguagesDataProvider?
                                            .projectLanguages?
                                            .languages?.Select(l => new AzdoLanguageStat(org, project, l.name, l.languagePercentage, now));
                if (languages != null) stats.AddRange(languages);
            }

            return new OkObjectResult("");
        }
    }
}

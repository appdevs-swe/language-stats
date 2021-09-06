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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Collections.Generic;
using CollectStats_Functions.Models;

namespace CollectStats.Functions
{
    public class CollectGithubStats
    {
        private readonly ILogger<CollectGithubStats> _logger;
        private readonly IGetFromGithub _github;

        public CollectGithubStats(ILogger<CollectGithubStats> logger, IGetFromGithub github)
        {
            _logger = logger;
            _github = github;
        }

        [OpenApiOperation(operationId: "getGithubStats", tags: new[] { "github", "stats" }, Summary = "Get Language stats from Github", Description = "Use octokit to get Github stats", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(GithubRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<GithubLanguageStat>), Summary = "The response", Description = "This returns the response")]
        [FunctionName("CollectGithubStats")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] GithubRequest request,
            ILogger log)
        {
            var stats = await _github.GetStats(request.Organization);
            return new OkObjectResult(stats);
        }
    }
}

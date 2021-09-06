using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using CollectStats_Functions.Services;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Collections.Generic;
using CollectStats_Functions.Models;
using Azure.Storage.Blobs;
using System.Text.Json;

namespace CollectStats.Functions
{
    public class CollectGithubStats
    {
        private readonly ILogger<CollectGithubStats> _logger;
        private readonly IGetStats<GithubLanguageStat> _github;

        public CollectGithubStats(ILogger<CollectGithubStats> logger, IGetStats<GithubLanguageStat> github)
        {
            _logger = logger;
            _github = github;
        }

        [OpenApiOperation(operationId: "getGithubStats", tags: new[] { "github" }, Summary = "Get Language stats from Github", Description = "Use octokit to get Github stats", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(GithubRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<GithubLanguageStat>), Summary = "The response", Description = "This returns the response")]
        [FunctionName("CollectGithubStats")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] GithubRequest request,
             [Blob("language-stats", Connection = "OutputStorage")] BlobContainerClient bcClient,
            ILogger log)
        {
            var results = await _github.GetStats(request.Organization);
            await BlobService.StoreBlob(
                JsonSerializer.SerializeToUtf8Bytes(results),
                bcClient,
                $"gh-{request.Organization}");
            return new OkObjectResult(results);
        }
    }
}

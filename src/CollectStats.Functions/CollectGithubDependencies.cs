using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CollectStats_Functions.Models;
using CollectStats_Functions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Collections.Generic;

namespace CollectStats_Functions
{
    public class CollectGithubDependencies
    {

        private readonly ILogger<CollectGithubDependencies> _logger;
        private readonly IGetDependencies<GithubDependency> _github;

        public CollectGithubDependencies(ILogger<CollectGithubDependencies> logger, IGetDependencies<GithubDependency> github)
        {
            _logger = logger;
            _github = github;
        }

        [FunctionName("CollectGithubDependencies")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(GithubRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<GithubLanguageStat>), Summary = "The response", Description = "This returns the response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] GithubRequest request,
             [Blob("language-stats", Connection = "OutputStorage")] BlobContainerClient bcClient)
        {
            var results = await _github.GetDependencies(request.Organization);
            await BlobService.StoreBlob(
                JsonSerializer.SerializeToUtf8Bytes(results),
                bcClient,
                $"gh-dep-{request.Organization}");
            return new OkObjectResult(results);
        }
    }
}


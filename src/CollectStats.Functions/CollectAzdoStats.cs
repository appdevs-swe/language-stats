using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using CollectStats_Functions.Services;
using CollectStats_Functions.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Collections.Generic;
using Azure.Storage.Blobs;
using System;
using System.Text.Json;
using System.IO;

namespace CollectStats.Functions
{
    public class CollectAzdoStats
    {
        private readonly ILogger<CollectAzdoStats> _logger;
        private readonly IGetStats<AzdoLanguageStat> _azdo;

        public CollectAzdoStats(ILogger<CollectAzdoStats> logger, IGetStats<AzdoLanguageStat> azdo)
        {
            _logger = logger;
            _azdo = azdo;
        }

        [OpenApiOperation(operationId: "getAzdoStats", tags: new[] { "Azure DevOps" }, Summary = "Get Language stats from Azure DevOps", Description = "Use undocumented endpoint to get Azure DevOps stats", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(AzdoRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<AzdoLanguageStat>), Summary = "The response", Description = "This returns the response")]
        [FunctionName("CollectAzdoStats")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] AzdoRequest request,
            [Blob("language-stats", Connection = "OutputStorage")] BlobContainerClient bcClient,
            ILogger log)
        {
            var results = await _azdo.GetStats(request.Organization);
            await BlobService.StoreBlob(
                JsonSerializer.SerializeToUtf8Bytes(results),
                bcClient,
                $"azdo-{request.Organization}");
            return new OkObjectResult(results);
        }


    }

}

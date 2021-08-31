using CollectStats_Functions.Models;
using Microsoft.Azure.Services.AppAuthentication;
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
    public class AzDO
    {
        //https://github.com/microsoft/azure-devops-auth-samples/blob/master/ManagedClientConsoleAppSample/Program.cs#L19
        // constant ResourceId for Azure DevOps, go figure
        private static string Resource = "499b84ac-1321-427f-aa17-267ca6975798";

        public static async Task<string> GetAccessToken(string providerString, string tenant)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider(providerString);
            return await azureServiceTokenProvider.GetAccessTokenAsync(Resource, tenant);
        }

        public static async Task<IEnumerable<string>> GetProjects(string accessToken, string orgUrl)
        {
            var connection = new VssConnection(new Uri(orgUrl), new VssOAuthAccessTokenCredential(accessToken));
            using var client = connection.GetClient<ProjectHttpClient>();
            var projects = await client.GetProjects();
            return projects.Select(a => a.Name);
        }

        public static async Task<LanguageDistributionResponse> GetLanguageStats(HttpClient client, string orgUrl, LanguageDistributionRequest body)
        {
            var project = body.dataProviderContext.properties.sourcePage.routeValues.project;
            var url = $"{orgUrl}/_apis/Contribution/HierarchyQuery/project/{project}?api-version=6.1-preview";
            var response = await client.PostAsJsonAsync(url, body);
            return await response.Content.ReadAsAsync<LanguageDistributionResponse>();
        }
    }
}

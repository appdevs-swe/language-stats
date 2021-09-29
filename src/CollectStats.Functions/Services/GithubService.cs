using CollectStats_Functions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Net.Http;
using System.Net.Http.Headers;
using GraphQL;

namespace CollectStats_Functions.Services
{
    public class GithubService : IGetStats<GithubLanguageStat>, IGetDependencies<GithubDependency>
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
            var appClient = new GitHubClient(new Octokit.ProductHeaderValue("local-app"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var response = await appClient.GitHubApps.CreateInstallationToken(_appOptions.GithubAppInstallationId);

            // Create a new GitHubClient using the installation token as authentication
            var installationClient = new GitHubClient(new Octokit.ProductHeaderValue($"local-Installation-{_appOptions.GithubAppInstallationId}"))
            {
                Credentials = new Credentials(response.Token)
            };

            return installationClient;
        }

        private async Task<GraphQLHttpClient> GetGraphQlClient()
        {
            var jwtToken = GetJwtFromApp();

            // Use the JWT as a Bearer token
            var appClient = new GitHubClient(new Octokit.ProductHeaderValue("local-app"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var response = await appClient.GitHubApps.CreateInstallationToken(_appOptions.GithubAppInstallationId);
            var options = new GraphQLHttpClientOptions
            {
                MediaType = @"application/vnd.github.hawkgirl-preview+json",
                EndPoint = new Uri("https://api.github.com/graphql"),
            };

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response.Token);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"application/vnd.github.hawkgirl-preview+json"));

            var graphQLClient = new GraphQLHttpClient(options, new SystemTextJsonSerializer(), httpClient);
            return graphQLClient;
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


        private async Task<RepoDependenciesResponse> GetDependencies(GraphQLHttpClient client, string org, string repo)
        {
            var query = new GraphQLRequest { Query = $@" {{
                              repository(owner: ""{org}"", name:""{repo}"") {{
                                dependencyGraphManifests {{
                                            nodes {{
                                                blobPath
                                              dependencies(first: 100) {{
                                                    nodes {{
                                                        packageName
                                                        requirements
                                                        hasDependencies
                                                        repository {{
                                                            nameWithOwner
                                                        }}
                                                        packageManager
                                                    }}
                                                }}
                                            }}
                                        }}
                                    }}
                                }}" };

            
            var result = await client.SendQueryAsync<RepoDependenciesResponse>(query);
            return result.Data;
        }

        public async Task<IEnumerable<GithubDependency>> GetDependencies(string org)
        {
            var appClient = await GetInstallationClient();
            var results = new List<GithubDependency>();
            var graphClient = await GetGraphQlClient();    
            foreach (var repo in await appClient.Repository.GetAllForOrg(org))
            {
                var response = await GetDependencies(graphClient, org, repo.Name);
                var deps = response.repository.dependencyGraphManifests.nodes.SelectMany(d => d.dependencies.nodes.Select( n => new GithubDependency
                {
                    Organization = org,
                    Repository = repo.Name,
                    PackageManager = n.packageManager,
                    PackageName = n.packageName,
                    PackageVersion = n.requirements,
                    PackagesSource = d.blobPath
                }));
                results.AddRange(deps);
            }
            return results;
        }
    }
}

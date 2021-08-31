namespace CollectStats_Functions.Models
{
    public class LanguageDistributionRequest
    {
        public string[] contributionIds { get; set; }
        public DataProviderContext dataProviderContext { get; set; }

        public static LanguageDistributionRequest Create(string project)
        {
            return new LanguageDistributionRequest
            {
                contributionIds = new[] { "ms.vss-code-web.vc-project-languages-data-provider" },
                dataProviderContext = new DataProviderContext
                {
                    properties = new Properties
                    {
                        sourcePage = new SourcePage
                        {
                            routeId = "ms.vss-tfs-web.project-overview-route",
                            routeValues = new RouteValues
                            {
                                project = project
                            }
                        }
                    }
                }
            };
        }
    }

    public class DataProviderContext
    {
        public Properties properties { get; set; }
    }

    public class Properties
    {
        public SourcePage sourcePage { get; set; }
    }

    public class SourcePage
    {
        public string routeId { get; set; }
        public RouteValues routeValues { get; set; }
    }

    public class RouteValues
    {
        public string project { get; set; }
    }




}

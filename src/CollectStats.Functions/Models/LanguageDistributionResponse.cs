using Newtonsoft.Json;

namespace CollectStats_Functions.Models
{
    public class LanguageDistributionResponse
    {
        public ProviderSharedData dataProviderSharedData { get; set; }
        public DataProviders dataProviders { get; set; }
    }

    public class ProviderSharedData
    {
    }

    public class DataProviders
    {
        [JsonProperty(PropertyName = "ms.vss-web.component-data")]
        public MsVssWebComponentData WebComponentData { get; set; }
        [JsonProperty(PropertyName = "ms.vss-web.shared-data")]
        public object WebSharedData { get; set; }
        [JsonProperty(PropertyName = "ms.vss-code-web.vc-project-languages-data-provider")]
        public MsVssCodeWebVcProjectLanguagesDataProvider ProjectLanguagesDataProvider { get; set; }
    }

    public class MsVssWebComponentData
    {
    }

    public class MsVssCodeWebVcProjectLanguagesDataProvider
    {
        public Projectlanguages projectLanguages { get; set; }
    }

    public class Projectlanguages
    {
        public Language[] languages { get; set; }
    }

    public class Language
    {
        public string name { get; set; }
        public double languagePercentage { get; set; }
    }
}

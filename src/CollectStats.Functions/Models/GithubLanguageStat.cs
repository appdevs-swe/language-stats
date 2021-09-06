using System;
using System.Collections.Generic;
using System.Text;

namespace CollectStats_Functions.Models
{
    public class GithubLanguageStat
    {
        public string Organization { get; set; }
        public string Repository { get; set; }
        public string LanguageName { get; set; }
        public long LanguageBytes { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}

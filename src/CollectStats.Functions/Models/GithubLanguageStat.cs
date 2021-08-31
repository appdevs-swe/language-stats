using System;
using System.Collections.Generic;
using System.Text;

namespace CollectStats_Functions.Models
{
    public class GithubLanguageStat
    {
        public GithubLanguageStat(string organization, string repo, string languageName, long languageBytes, DateTimeOffset date)
        {
            Organization = organization;
            Repository = repo;
            Date = date;
            LanguageBytes = languageBytes;
            LanguageName = languageName;

        }
        public readonly string Organization;
        public readonly string Repository;
        public readonly string LanguageName;
        public readonly long LanguageBytes;
        public readonly DateTimeOffset Date;
    }
}

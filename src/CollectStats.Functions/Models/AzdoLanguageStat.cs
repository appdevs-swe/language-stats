using System;
using System.Collections.Generic;
using System.Text;

namespace CollectStats_Functions.Models
{
    public class AzdoLanguageStat
    {
        public AzdoLanguageStat(string organization, string project, string languageName, double languagePercentage, DateTimeOffset date)
        {
            Organization = organization;
            ProjectName = project;
            Date = date;
            LanguageName = languageName;
            LanguagePercentage = languagePercentage;

        }
        public readonly string Organization;
        public readonly string ProjectName;
        public readonly string LanguageName;
        public readonly double LanguagePercentage;
        public readonly DateTimeOffset Date;
    }
}

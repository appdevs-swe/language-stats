using System;
using System.Collections.Generic;
using System.Text;

namespace CollectStats_Functions.Models
{
    public class AzdoLanguageStat
    {
        public string Organization { get; set; }
        public string ProjectName { get; set; }
        public string LanguageName { get; set; }
        public double LanguagePercentage { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}

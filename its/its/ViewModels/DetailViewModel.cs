using System;
using System.Collections.Generic;
using its.Models;

namespace its.ViewModels
{
    public class DetailViewModel
    {
        public DateTime Date { get; set; }
        public string Branch { get; set; }
        public string BranchName { get; set; }

        public DateTime MinimumBranchHour { get; set; }
        public DateTime MaximumBranchHour { get; set; }
        public DateTime MinimumGlobalHour { get; set; }
        public DateTime MaximumGlobalHour { get; set; }

        public DateTime HistoricalDate { get; set; }
        public DateTime Historical2Date { get; set; }

        public IEnumerable<DisplayHourlySummary> Stats { get; set; }
        public IEnumerable<DisplayHourlySummary> Historical { get; set; }
        public IEnumerable<DisplayHourlySummary> Historical2 { get; set; }
        public IEnumerable<DisplayHourlySummary> GlobalStats { get; set; }
        public IEnumerable<DisplayHourlySummary> GlobalHistorical { get; set; }
        public IEnumerable<DisplayHourlySummary> GlobalHistorical2 { get; set; }
    }
}

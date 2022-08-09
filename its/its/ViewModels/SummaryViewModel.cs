using System;
using System.Collections.Generic;
using its.Models;

namespace its.ViewModels
{
    public class SummaryViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Branch { get; set; }
        public IEnumerable<DisplayDailySummary> Stats { get; set; }
    }
}
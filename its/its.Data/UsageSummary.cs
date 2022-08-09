using System;

namespace its.Data
{
    public class UsageSummary
    {
        public DateTime Date { get; set; }
        public string Branch { get; set; }
        public int Minutes { get; set; }
        public int MaxMinutes { get; set; }
    }
}

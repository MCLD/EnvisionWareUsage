using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using its.Data;

namespace its.Models
{
    public class DisplayDailySummary : UsageSummary
    {
        public string Color { get; set; }
        public int Percentage { get; set; }

        public string TextClass
        {
            get
            {
                return "cu-day"
                    + (IsMax ? " cu-day-max" : "")
                    + (IsMin ? " cu-day-min" : "");
            }
        }

        public bool IsMax { get; set; }
        public bool IsMin { get; set; }

        public string Title
        {
            get
            {
                return $"{Percentage}%, {Minutes} minutes";
            }
        }

        public DisplayDailySummary(UsageSummary summary, int minMinutes, int maxMinutes)
        {
            Date = summary.Date;
            Branch = summary.Branch;
            Minutes = summary.Minutes;
            MaxMinutes = summary.MaxMinutes;

            var hue = 120 - (Minutes * 120 / maxMinutes);

            Color = its.Color.HsvToRgb(hue, 1, 1);
            Percentage = Minutes * 100 / maxMinutes;
            IsMax = Minutes == maxMinutes;
            IsMin = Minutes == minMinutes;
        }
    }
}

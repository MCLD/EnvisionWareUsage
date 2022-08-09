using its.Data;

namespace its.Models
{
    public class DisplayHourlySummary : UsageSummary
    {
        public int Percent
        {
            get { return MaxMinutes > 0 ? Minutes * 100 / MaxMinutes : 0; }
        }

        public string Label
        {
            get
            {
                return Percent >= 10 ? $"{Percent}%" : "";
            }
        }

        public string Title
        {
            get { return $"{Percent}%, {Minutes} minutes"; }
        }

        public string TextClass
        {
            get { return Minutes == 0 ? "cu-timespan-nodata" : "cu-timespan-data"; }
        }

        public DisplayHourlySummary(UsageSummary summary)
        {
            Date = summary.Date;
            Branch = summary.Branch;
            Minutes = summary.Minutes;
            MaxMinutes = summary.MaxMinutes;
        }
    }
}

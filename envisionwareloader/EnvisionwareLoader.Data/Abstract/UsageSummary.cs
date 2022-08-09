using System;
using System.ComponentModel.DataAnnotations;

namespace EnvisionwareLoader.Data.Abstract
{
    public abstract class UsageSummary
    {
        public DateTime Date { get; set; }

        [MaxLength(50)]
        public string Branch { get; set; }

        public int Minutes { get; set; }
        public int MaxMinutes { get; set; }
    }
}

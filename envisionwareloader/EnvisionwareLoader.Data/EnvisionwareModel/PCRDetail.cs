using System;

namespace EnvisionwareLoader.Data.EnvisionwareModel
{
    public class PCRDetail
    {
        public int PcrKey { get; set; }
        public int PcrMinutesUsed { get; set; }
        public DateTime PcrDateTime { get; set; }
        public string PcrBranch { get; set; }
        public string PcrArea { get; set; }
    }
}

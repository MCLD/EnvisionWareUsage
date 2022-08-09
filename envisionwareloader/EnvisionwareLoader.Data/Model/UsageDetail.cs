using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnvisionwareLoader.Data.Model
{
    public class UsageDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Key { get; set; }
        public int Minutes { get; set; }
        public DateTime Timestamp { get; set; }
        [MaxLength(50)]
        public string Branch { get; set; }
        [MaxLength(50)]
        public string Area { get; set; }
    }
}

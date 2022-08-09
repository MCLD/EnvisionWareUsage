using System.ComponentModel.DataAnnotations;

namespace EnvisionwareLoader.Data.Model
{
    public class Branch
    {
        [Key]
        [MaxLength(50)]
        public string Code { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }
    }
}

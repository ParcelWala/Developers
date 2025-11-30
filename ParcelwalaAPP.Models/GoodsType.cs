using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.Models
{
    public class GoodsType
    {
        [Key]
        public int GoodsTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Icon { get; set; } = string.Empty;

        [Required]
        public double DefaultWeight { get; set; }

        [Required]
        public int DefaultPackages { get; set; } = 1;

        [Required]
        public int DefaultValue { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

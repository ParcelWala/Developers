using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class ErrorResponse
    {
        public bool success { get; set; } = false;
        public string message { get; set; }
        public string? errorcode { get; set; }
        public DateTime? timestamp { get; set; } = DateTime.UtcNow;
    }
}

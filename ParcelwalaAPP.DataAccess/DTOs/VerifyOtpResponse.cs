using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class VerifyOtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AuthData? Data { get; set; }
    }
}

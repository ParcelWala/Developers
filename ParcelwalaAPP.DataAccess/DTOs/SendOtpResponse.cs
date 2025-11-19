using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class SendOtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public SendOtpResponseData Data { get; set; }



    }
    public class SendOtpResponseData
    {
        public string OTP { get; set; } = string.Empty; // Only in development
        public DateTime ExpiresAt { get; set; }
        public int UserID { get; set; }
        public int ExpiresInSeconds { get; set; }

    }
}

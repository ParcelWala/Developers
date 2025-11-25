using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class SendOtpResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public SendOtpResponseData Data { get; set; }



    }
    public class SendOtpResponseData
    {
        public string otp_id { get; set; } = string.Empty; //Unique OTP session identifier
        public string otp { get; set; } = string.Empty; // Only in development
        //public DateTime ExpiresAt { get; set; }
        //public int userID { get; set; }
        public int expires_in { get; set; }
        public int can_resend_after { get; set; }

    }
}

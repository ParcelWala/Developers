using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class VerifyOtpRequest
    {
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be a 6-digit number")]
        public string Otp { get; set; } = string.Empty;
        public string device_token { get; set; } = string.Empty;
        public string device_type { get; set; } = string.Empty;
    }
}

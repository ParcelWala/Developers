using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP
{
    [Table("OTPVerifications")]
    public class OTPVerifications
    {
        [Key]
        public int OTPID { get; set; }
        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "OTP Code is required")]
        public string OTPCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Purpose is required")]
        public string Purpose { get; set; } = string.Empty;     
        public bool? IsUsed { get; set; }       
        public DateTime CreatedAt     { get; set; }
        public DateTime ExpiresAt { get; set; }
     
    }                            
}

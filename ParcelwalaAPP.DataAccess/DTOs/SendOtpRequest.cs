using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class SendOtpRequest
    {
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Phone number must be in E.164 format")]
        public string phone_number { get; set; } = string.Empty;
        [Required(ErrorMessage = "Country Code is required")]
        public string country_code { get; set; } = string.Empty;
        [Required(ErrorMessage = "Purpose is required")]
        public string purpose { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Full name is required")]
        //[MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        //[MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        //public string FullName { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Email is required")]
        //[EmailAddress(ErrorMessage = "Invalid email format")]
        //public string Email { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class CompleteProfileRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [MinLength(3, ErrorMessage = "Full name must be at least 3 characters")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string full_name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100)]
        public string? email { get; set; }

        [MaxLength(20, ErrorMessage = "Referral code cannot exceed 20 characters")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Referral code must contain only uppercase letters and numbers")]
        public string? referral_code { get; set; }
    }

    public class CompleteProfileResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public CompleteProfileData? Data { get; set; }
    }

    public class CompleteProfileData
    {
        public int user_id { get; set; }
        public int customer_id { get; set; }
        public string phone_number { get; set; } = string.Empty;
        public string full_name { get; set; } = string.Empty;
        public string? email { get; set; }
        public string? profile_image { get; set; }
        public bool is_new_user { get; set; }
        public decimal wallet_balance { get; set; }
        public string referral_code { get; set; } = string.Empty;
    }
}

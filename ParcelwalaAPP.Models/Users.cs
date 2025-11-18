using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.Models
{
    [Table("Users")]
    public class Users
    {
        [Key]
        public int UserID       { get; set; }
        public string? UserType      { get; set; }
        public string? Email         { get; set; }
        public string? PhoneNumber   { get; set; }
        public string? PasswordHash  { get; set; }
        public string? FullName      { get; set; }
        public string? ProfileImage  { get; set; }
        public bool IsActive      { get; set; }
        public bool IsVerified    { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public DateTime CreatedAt     { get; set; }
        public DateTime? UpdatedAt     { get; set; }
        public DateTime? LastLoginAt   { get; set; }
        public string? DeviceToken { get; set; }

        // Navigation property
        [NotMapped]
        public ICollection<OTPVerifications> oTPVerifications { get; set; } = new List<OTPVerifications>();
    }                            
}

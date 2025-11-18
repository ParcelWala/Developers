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
    //[Table("CustomerProfiles")]
    public class CustomerProfiles
    {
        [Key]
        public int CustomerID { get; set; }
        public int UserID { get; set; }
        public string? CustomerOtp { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public string? CompanyName { get; set; }
        public string? GSTNumber { get; set; }
        public string? CustomerType { get; set; }
        public string? PreferredLanguage { get; set; }
        public decimal? Rating { get; set; }
        public int? TotalBookings { get; set; }
        public decimal? WalletBalance { get; set; }
        public string? ReferralCode { get; set; }
        public int? ReferredBy { get; set; }
        public DateTime CreatedAt     { get; set; }
        public DateTime UpdatedAt     { get; set; }
     
    }                            
}

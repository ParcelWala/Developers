using ParcelwalaAPP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class AuthData
    {
        public userDTO user { get; set; }
        public tokensDTO tokens { get; set; } 
    }
    public class userDTO
    {
        public int user_id { get; set; } 
        public int customer_id { get; set; } 
        public string phone_number { get; set; } = string.Empty;
        public string full_name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string profile_image { get; set; } = string.Empty;
        public bool? is_new_user { get; set; } 
        public decimal wallet_balance { get; set; }
        public string referral_code { get; set; } = string.Empty;
    }
    public class tokensDTO
    {
        public string access_token { get; set; } = string.Empty;
        public string refresh_token { get; set; } = string.Empty;
        public int expires_in { get; set; } 
      
    }
}

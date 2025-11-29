using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class CustomerProfile
    {
        public bool success { get; set; }
        public CustomerProfileResponse Data { get; set; }
    }
    public class CustomerProfileResponse
    {
           public int user_id { get; set; }
           public string full_name        {get;set;}
           public string email            {get;set;}
           public string phone_number     {get;set;}
           public string profile_image    {get;set;}
           public string customer_type   {get;set;}
           public string company_name     {get;set;}
           public string gst_number       {get;set;}
           public decimal? rating           {get;set;}
           public int? total_bookings   {get;set;}
           public decimal? wallet_balance   {get;set;}
           public string referral_code    {get;set;}
           public bool is_verified      {get;set;}
           public DateTime created_at { get; set; }
    }
}

//using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Parcelwala.DataAccess.Data;
using Parcelwala.DataAccess.Services;
using ParcelwalaAPP.DataAccess.DTOs;
using ParcelwalaAPP.Models;


//using Parcelwala.DataAccess.Data;
//using Parcelwala.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.Services
{
    public interface ICustomerService
    {
        //Task<(bool Success, string Message, OTPVerifications? OTPVerifications)> SendOtpAsync(
        //    string phoneNumber, string countrycode, string purpose);
        Task<CustomerProfile> GetProfileAsync(int userId);
       
    }

    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<OtpAuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;
       // private const int OTP_EXPIRY_MINUTES=5;
        public CustomerService(
            AppDbContext context,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<OtpAuthService> logger,
            IConfiguration configuration,
            IJwtService jwtService)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
            _configuration = configuration;
            _jwtService = jwtService;
        }

    

        public async Task<CustomerProfile> GetProfileAsync(int UserId)
        {
            try
            {               

                // Find or create user
                var user = await _context.Users
                    .Include(c=>c.CustomerProfile)
                     .FirstOrDefaultAsync(u => u.UserID == UserId); 

                if (user != null)
                {
                    var response = new CustomerProfileResponse
                    {

                        user_id=user.UserID,
                        full_name=user.FullName,
                        email=user.Email,
                        phone_number=user.PhoneNumber,
                        profile_image=user.ProfileImage,
                        customer_type=user.CustomerProfile.CustomerType,
                        company_name=user.CustomerProfile.CompanyName,
                        gst_number=user.CustomerProfile.GSTNumber,
                        rating=user.CustomerProfile.Rating,
                        total_bookings=user.CustomerProfile.TotalBookings,
                        wallet_balance=user.CustomerProfile.WalletBalance,
                        referral_code=user.CustomerProfile.ReferralCode,
                        is_verified=user.IsVerified,
                        created_at=user.CreatedAt,


                    };
                    _logger.LogInformation("Get Customer Profiles");
                    return new CustomerProfile
                    {
                        success = true,
                        Data = response
                    };
                   
                }
                else
                {
                  
                    _logger.LogInformation("Customer Profile not found with UserID: {UserID}", UserId);
                    return new CustomerProfile
                    {
                        success = false,

                        Data = null
                    };
                }

                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer profile", UserId);
                return new CustomerProfile
                {
                    success = false,
                   
                    Data = null
                };
            }
        }
       
    }
}

//using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Parcelwala.DataAccess.Data;

//using Parcelwala.DataAccess.Data;
//using Parcelwala.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.Services
{
    public interface IOtpService
    {
        Task<(bool Success, string Message, OTPVerifications? OTPVerifications)> SendOtpAsync(
            string phoneNumber, string countrycode, string purpose);
        string GenerateOtp();
    }

    public class OtpService : IOtpService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<OtpService> _logger;
        private readonly IConfiguration _configuration;

        public OtpService(
            AppDbContext context,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<OtpService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, OTPVerifications? OTPVerifications)> SendOtpAsync(
            string phoneNumber, string countrycode, string purpose)
        {
            try
            {
                // Normalize phone number
                phoneNumber = phoneNumber.Trim();

                //// Check for existing customer
                //var customer = await _context.Customers
                //    .Include(c => c.CustomerOtp)
                //    .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

                //// Create new customer if doesn't exist
                //if (customer == null)
                //{
                //    customer = new Customer
                //    {
                //        FullName = fullName,
                //        Email = email,
                //        PhoneNumber = phoneNumber,
                //        CreatedAt = DateTime.UtcNow,
                //        IsActive = true
                //    };

                //    _context.Customers.Add(customer);
                //    await _context.SaveChangesAsync();

                //    _logger.LogInformation("New customer created with ID: {CustomerId}", customer.CustomerID);
                //}
                //else
                //{
                //    // Update customer details if they've changed
                //    customer.FullName = fullName;
                //    customer.Email = email;
                //    _context.Customers.Update(customer);
                //}

                // Generate new OTP
                var otpCode = GenerateOtp();
                var expiresAt = DateTime.UtcNow.AddMinutes(5);
         
                var newOtp = new OTPVerifications 
                {
                   // OTPID = customer.Id,
                    OTPCode = otpCode,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    IsUsed = false,
                    Purpose = purpose,
                    PhoneNumber=phoneNumber
                  
                };

                _context.OTPVerifications.Add(newOtp);

                // Update or create OTP record
                //if (customer.CustomerOtp != null)
                //{
                //    // Update existing OTP
                //    customer.CustomerOtp.OtpCode = otpCode;
                //    customer.CustomerOtp.ExpiresAt = expiresAt;
                //    customer.CustomerOtp.CreatedAt = DateTime.UtcNow;
                //    customer.CustomerOtp.IsUsed = false;
                //    customer.CustomerOtp.AttemptCount = 0;

                //    _context.CustomerOtps.Update(customer.CustomerOtp);
                //}
                //else
                //{
                //    // Create new OTP record
                //    var newOtp = new CustomerOtp
                //    {
                //        CustomerId = customer.Id,
                //        OtpCode = otpCode,
                //        ExpiresAt = expiresAt,
                //        CreatedAt = DateTime.UtcNow,
                //        IsUsed = false,
                //        AttemptCount = 0
                //    };

                //    _context.CustomerOtps.Add(newOtp);
                //}

                await _context.SaveChangesAsync();

                // Send OTP via preferred method (SMS or Email)
                var sendMethod = _configuration["OtpSettings:PreferredMethod"] ?? "SMS";
                bool sendSuccess = false;

                if (sendMethod.Equals("SMS", StringComparison.OrdinalIgnoreCase))
                {
                    sendSuccess = await _smsService.SendOtpSmsAsync(phoneNumber, otpCode);
                }
                else
                {
                    var email = "kishorfarad@gmail.com";
                    var fullName = "Kishor Farad";
                    sendSuccess = await _emailService.SendOtpEmailAsync(email, fullName, otpCode);
                }

                if (!sendSuccess)
                {
                    _logger.LogWarning("Failed to send OTP to {PhoneNumber} via {Method}",
                        phoneNumber, sendMethod);
                    return (false, $"Failed to send OTP via {sendMethod}. Please try again.", null);
                }

                _logger.LogInformation("OTP sent successfully to {PhoneNumber} via {Method}",
                    phoneNumber, sendMethod);

                return (true, "OTP sent successfully. Valid for 5 minutes.", newOtp);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while sending OTP for {PhoneNumber}", phoneNumber);
                return (false, "A database error occurred. Please try again.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending OTP for {PhoneNumber}", phoneNumber);
                return (false, "An unexpected error occurred. Please try again.", null);
            }
        }

        public string GenerateOtp()
        {
            // Generate cryptographically secure random 6-digit OTP
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }
    }
}

//using Microsoft.EntityFrameworkCore;
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.Services
{
    public interface IOtpAuthService
    {
        //Task<(bool Success, string Message, OTPVerifications? OTPVerifications)> SendOtpAsync(
        //    string phoneNumber, string countrycode, string purpose);
        Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request);
        Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
        string GenerateOtp();
    }

    public class OtpAuthService : IOtpAuthService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<OtpAuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;
        private const int OTP_EXPIRY_MINUTES = 5;
        public OtpAuthService(
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

        //public async Task<(bool Success, string Message, OTPVerifications? OTPVerifications)> SendOtpAsync(
        //    string phoneNumber, string countrycode, string purpose)
        //{
        //    try
        //    {
        //        // Normalize phone number
        //        phoneNumber = phoneNumber.Trim();

        //        // Check for existing User
        //        var User = await _context.Users
        //            //.Include(c => c.oTPVerifications)
        //            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

        //        // Create new User if doesn't exist
        //        if (User == null)
        //        {
        //            User = new Users
        //            {
        //                FullName = string.Empty,
        //                Email = string.Empty,
        //                PhoneNumber = phoneNumber,
        //                CreatedAt = DateTime.UtcNow,
        //                IsActive = false,
        //                IsVerified = false,
        //                UserType="Customer"
        //            };

        //            _context.Users.Add(User);
        //            await _context.SaveChangesAsync();

        //            _logger.LogInformation("New User created with ID: {UserId}", User.UserID);
        //        }
        //        else
        //        {
        //            // Update User details if they've changed
        //            User.FullName = string.Empty;
        //            User.Email = string.Empty;
        //            _context.Users.Update(User);
        //        }

        //        // Generate new OTP
        //        var otpCode = GenerateOtp();
        //        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        //        // Invalidate previous unused OTPs for this phone number
        //        var previousOtps = await _context.OTPVerifications
        //            .Where(o => o.PhoneNumber == phoneNumber &&
        //                        !o.IsUsed &&
        //                        o.ExpiresAt > DateTime.UtcNow)
        //            .ToListAsync();

        //        foreach (var oldOtp in previousOtps)
        //        {
        //            oldOtp.IsUsed = true;
        //        }


        //        // Create new OTP record
        //        var otpVerification = new OTPVerifications 
        //        {
        //            PhoneNumber = phoneNumber,
        //            OTPCode = otpCode,
        //            Purpose = purpose,
        //            IsUsed = false,
        //            ExpiresAt = expiresAt,
        //            CreatedAt = DateTime.UtcNow,
        //            UserID = User.UserID
        //        };

        //        _context.OTPVerifications.Add(otpVerification);
        //        await _context.SaveChangesAsync();

        //        // Send OTP via preferred method (SMS or Email)
        //        var sendMethod = _configuration["OtpSettings:PreferredMethod"] ?? "SMS";
        //        bool sendSuccess = false;

        //        if (sendMethod.Equals("SMS", StringComparison.OrdinalIgnoreCase))
        //        {
        //            sendSuccess = await _smsService.SendOtpSmsAsync(phoneNumber, otpCode);
        //        }
        //        else
        //        {
        //            var email = "kishorfarad@gmail.com";
        //            var fullName = "Kishor Farad";
        //            sendSuccess = await _emailService.SendOtpEmailAsync(email, fullName, otpCode);
        //        }

        //        if (!sendSuccess)
        //        {
        //            _logger.LogWarning("Failed to send OTP to {PhoneNumber} via {Method}",
        //                phoneNumber, sendMethod);
        //            return (false, $"Failed to send OTP via {sendMethod}. Please try again.", null);
        //        }

        //        _logger.LogInformation("OTP sent successfully to {PhoneNumber} via {Method}",
        //            phoneNumber, sendMethod);

        //        return (true, "OTP sent successfully. Valid for 5 minutes.", otpVerification);
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        _logger.LogError(ex, "Database error while sending OTP for {PhoneNumber}", phoneNumber);
        //        return (false, "A database error occurred. Please try again.", null);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error while sending OTP for {PhoneNumber}", phoneNumber);
        //        return (false, "An unexpected error occurred. Please try again.", null);
        //    }
        //}

        public async Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request)
        {
            try
            {
                _logger.LogInformation("Sending OTP to phone: {PhoneNumber}, Purpose: {Purpose}",
                    request.PhoneNumber, request.Purpose);

                // Generate 6-digit OTP
                var otpCode = GenerateOtp();
                var expiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES);

                // Find or create user
                var user = await _context.Users
                    .Include(c=>c.CustomerProfile)
                    .Include(d=>d.ReferredCustomers)
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

                if (user == null)
                {
                    // Create new user with only phone number
                    user = new Users
                    {
                        PhoneNumber = request.PhoneNumber,
                        FullName = string.Empty,
                        Email=string.Empty,
                        UserType = "Customer",  // If it comes dynamically then need to pass dynamic value.
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        IsVerified = false,
                        PhoneVerified = false,
                        EmailVerified = false,
                       

                    };

                    user.CustomerProfile = new CustomerProfiles
                    {
                        ReferredBy = null,
                        CustomerCode = GenerateCustomerCode(), // UserID will be automatically set by EF Core
                        ReferralCode = GenerateReferralCode(user.PhoneNumber),
                        CreatedAt = DateTime.UtcNow, // ReferredBy is null (no referral)
                    };
                    _context.Users.Add(user);
                    //if (user.customerProfile == null)
                    //{
                    //    var customerProfile = new CustomerProfiles();
                    //    customerProfile.UserID = user.UserID;
                    //    customerProfile.CustomerCode = GenerateCustomerCode();
                    //    customerProfile.CreatedAt = DateTime.UtcNow;
                    //    _context.CustomerProfiles.Add(customerProfile);
                    //}
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("New user created with UserID: {UserID}", user.UserID);
                }
                else
                {
                    _logger.LogInformation("Existing user found with UserID: {UserID}", user.UserID);
                }

                // Invalidate previous unused OTPs for this phone number
                var previousOtps = await _context.OTPVerifications
                    .Where(o => o.PhoneNumber == request.PhoneNumber &&
                                !o.IsUsed &&
                                o.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();

                foreach (var oldOtp in previousOtps)
                {
                    oldOtp.IsUsed = true;
                }

                // Create new OTP record
                var otpVerification = new OTPVerifications
                {
                    PhoneNumber = request.PhoneNumber,
                    OTPCode = otpCode,
                    Purpose = request.Purpose,
                    IsUsed = false,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    UserID = user.UserID
                };

                _context.OTPVerifications.Add(otpVerification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("OTP generated successfully for UserID: {UserID}, OTPID: {OTPID}",
                    user.UserID, otpVerification.OTPID);

                // TODO: Send OTP via SMS service (Twilio, AWS SNS, etc.)
                // await _smsService.SendOtpAsync(request.PhoneNumber, otpCode);

                var response = new SendOtpResponseData
                {
                    ExpiresAt = expiresAt,
                    UserID = user.UserID,
                    ExpiresInSeconds = OTP_EXPIRY_MINUTES * 60
                };

                // Include OTP in response only in development mode
                var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
                if (environment == "Development")
                {
                    response.OTP = otpCode;
                }

                return new SendOtpResponse
                {
                    Success = true,
                    Message = "OTP sent successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to phone: {PhoneNumber}", request.PhoneNumber);
                return new SendOtpResponse
                {
                    Success = false,
                    Message = "Failed to send OTP. Please try again.",
                    Data = null
                };
            }
        }
        public async Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request)
        {
            try
            {
                _logger.LogInformation("Verifying OTP for phone: {PhoneNumber}", request.PhoneNumber);

                // Find the OTP
                var otpVerification = await _context.OTPVerifications
                    .Where(o => o.PhoneNumber == request.PhoneNumber &&
                                o.OTPCode == request.OTPCode &&
                                !o.IsUsed)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                // Validate OTP exists
                if (otpVerification == null)
                {
                    _logger.LogWarning("Invalid OTP attempt for phone: {PhoneNumber}", request.PhoneNumber);
                    return new VerifyOtpResponse
                    {
                        Success = false,
                        Message = "Invalid OTP code",
                        Data = null
                    };
                }

                // Validate OTP not expired
                if (DateTime.UtcNow > otpVerification.ExpiresAt)
                {
                    _logger.LogWarning("Expired OTP attempt for phone: {PhoneNumber}, OTPID: {OTPID}",
                        request.PhoneNumber, otpVerification.OTPID);
                    return new VerifyOtpResponse
                    {
                        Success = false,
                        Message = "OTP has expired. Please request a new one.",
                        Data = null
                    };
                }

                // Mark OTP as used
                otpVerification.IsUsed = true;

                // Find user and update verification status
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

                if (user == null)
                {
                    _logger.LogError("User not found for verified phone: {PhoneNumber}", request.PhoneNumber);
                    return new VerifyOtpResponse
                    {
                        Success = false,
                        Message = "User not found",
                        Data = null
                    };
                }
                
                


                    // Update user verification status
                user.PhoneVerified = true;
                user.IsVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
                user.LastLoginAt = DateTime.UtcNow;
                // Generate JWT token
                var token = _jwtService.GenerateTokenForUser(user);
                user.DeviceToken = token;
                await _context.SaveChangesAsync();

                _logger.LogInformation("OTP verified successfully for UserID: {UserID}", user.UserID);

               

                var userDto = new Users
                {
                    UserID = user.UserID,
                    UserType = user.UserType,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.FullName,
                    ProfileImage = user.ProfileImage,
                    IsActive = user.IsActive,
                    IsVerified = user.IsVerified,
                    EmailVerified = user.EmailVerified,
                    PhoneVerified = user.PhoneVerified,
                    DeviceToken= token,
                };

                return new VerifyOtpResponse
                {
                    Success = true,
                    Message = "OTP verified successfully",
                    Data = new AuthData
                    {
                        Token = token,
                        User = userDto
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for phone: {PhoneNumber}", request.PhoneNumber);
                return new VerifyOtpResponse
                {
                    Success = false,
                    Message = "Failed to verify OTP. Please try again.",
                    Data = null
                };
            }
        }


        public string GenerateOtp()
        {
            // Generate cryptographically secure random 6-digit OTP
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }
        public string GenerateCustomerCode()
        {
            // Generate unique customer code: CUST + 6-digit number
            // Example: CUST001234, CUST987654

            var random = new Random();
            var code = $"CUST{random.Next(100000, 999999)}";

            // Ensure uniqueness (retry if exists)
            while (_context.CustomerProfiles.Any(p => p.CustomerCode == code))
            {
                code = $"CUST{random.Next(100000, 999999)}";
            }

            return code;
        }

        public string GenerateReferralCode(string phoneNumber)
        {
            // Generate unique referral code: REF- + last 4 digits of phone + 4 random chars
            // Example: REF-5210-A8F3

            var lastDigits = phoneNumber.Length >= 4
                ? phoneNumber.Substring(phoneNumber.Length - 4)
                : phoneNumber.PadLeft(4, '0');

            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            var code = $"REF-{lastDigits}-{randomPart}";

            // Ensure uniqueness
            while (_context.CustomerProfiles.Any(p => p.ReferralCode == code))
            {
                randomPart = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
                code = $"REF-{lastDigits}-{randomPart}";
            }

            return code;
        }
    }
}

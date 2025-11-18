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
    public interface IOtpService
    {
        Task<(bool Success, string Message, OTPVerifications? OTPVerifications)> SendOtpAsync(
            string phoneNumber, string countrycode, string purpose);
        //Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request);
        Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
        string GenerateOtp();
    }

    public class OtpService : IOtpService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<OtpService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;

        public OtpService(
            AppDbContext context,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<OtpService> logger,
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

        public async Task<(bool Success, string Message, OTPVerifications? OTPVerifications)> SendOtpAsync(
            string phoneNumber, string countrycode, string purpose)
        {
            try
            {
                // Normalize phone number
                phoneNumber = phoneNumber.Trim();

                // Check for existing User
                var User = await _context.Users
                    //.Include(c => c.oTPVerifications)
                    .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

                // Create new User if doesn't exist
                if (User == null)
                {
                    User = new Users
                    {
                        FullName = string.Empty,
                        Email = string.Empty,
                        PhoneNumber = phoneNumber,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = false,
                        IsVerified = false,
                        UserType="Customer"
                    };

                    _context.Users.Add(User);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("New User created with ID: {UserId}", User.UserID);
                }
                else
                {
                    // Update User details if they've changed
                    User.FullName = string.Empty;
                    User.Email = string.Empty;
                    _context.Users.Update(User);
                }

                // Generate new OTP
                var otpCode = GenerateOtp();
                var expiresAt = DateTime.UtcNow.AddMinutes(5);

                // Invalidate previous unused OTPs for this phone number
                var previousOtps = await _context.OTPVerifications
                    .Where(o => o.PhoneNumber == phoneNumber &&
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
                    PhoneNumber = phoneNumber,
                    OTPCode = otpCode,
                    Purpose = purpose,
                    IsUsed = false,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    UserID = User.UserID
                };

                _context.OTPVerifications.Add(otpVerification);
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

                return (true, "OTP sent successfully. Valid for 5 minutes.", otpVerification);
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

                await _context.SaveChangesAsync();

                _logger.LogInformation("OTP verified successfully for UserID: {UserID}", user.UserID);

                // Generate JWT token
                var token = _jwtService.GenerateTokenForUser(user);

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
    }
}

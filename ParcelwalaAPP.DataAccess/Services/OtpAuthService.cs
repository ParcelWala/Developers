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
        Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<RefreshTokenResponse> RevokeRefreshTokenAsync(RefreshTokenRequest request);
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
       // private const int OTP_EXPIRY_MINUTES=5;
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

    

        public async Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request)
        {
            try
            {
                _logger.LogInformation("Sending OTP to phone: {phone_number}, countrycode: {country_code}, Purpose: {purpose}",
                    request.phone_number,request.country_code, request.purpose);
                int OTP_EXPIRY_MINUTES = Convert.ToInt32(_configuration["OtpSettings:OTP_EXPIRY_MINUTES"]);
                int RESEND_OTP_MINUTES = Convert.ToInt32(_configuration["OtpSettings:RESEND_OTP_MINUTES"]);
                   
                // Generate 6-digit OTP
                var otpCode = GenerateOtp();
                var expiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES);

                // Find or create user
                var user = await _context.Users
                    .Include(c=>c.CustomerProfile)
                    .Include(d=>d.ReferredCustomers)
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.phone_number);

                if (user == null)
                {
                    // Create new user with only phone number
                    user = new Users
                    {
                        PhoneNumber = request.phone_number,
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
                        WalletBalance=0,    
                        IsnewUser=true,
                        CustomerCode = GenerateCustomerCode(), // UserID will be automatically set by EF Core
                        ReferralCode =GenerateReferralCode(user.PhoneNumber),
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
                    .Where(o => o.PhoneNumber == request.phone_number &&
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
                    PhoneNumber = request.phone_number,
                    
                    OTPCode = otpCode,
                    Purpose = request.purpose,
                    CountryCode=request.country_code,
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
                    //ExpiresAt = expiresAt,
                    //UserID = user.UserID,
                    otp_id="otp_"+ otpVerification.OTPID,
                    expires_in = OTP_EXPIRY_MINUTES * 60,
                    can_resend_after= RESEND_OTP_MINUTES * 60,


                };

                // Include OTP in response only in development mode
                var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
                if (environment == "Development")
                {
                    response.otp = otpCode;
                }

                return new SendOtpResponse
                {
                    success = true,
                    message = "OTP sent successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to phone: {PhoneNumber}", request.phone_number);
                return new SendOtpResponse
                {
                    success = false,
                    message = "Failed to send OTP. Please try again.",
                    Data = null
                };
            }
        }
        public async Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request)
        {
            try
            {
                _logger.LogInformation("Verifying OTP for phone: {phone_number}", request.phone_number);

                // Find the OTP
                var otpVerification = await _context.OTPVerifications
                    .Where(o => o.PhoneNumber == request.phone_number &&
                                o.OTPCode == request.otp &&
                                !o.IsUsed)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                // Validate OTP exists
                if (otpVerification == null)
                {
                    _logger.LogWarning("Invalid OTP attempt for phone: {PhoneNumber}", request.phone_number);
                    return new VerifyOtpResponse
                    {
                        success = false,
                        message = "Invalid OTP code",
                        Data = null
                    };
                }

                // Validate OTP not expired
                if (DateTime.UtcNow > otpVerification.ExpiresAt)
                {
                    _logger.LogWarning("Expired OTP attempt for phone: {phone_number}, OTPID: {OTPID}",
                        request.phone_number, otpVerification.OTPID);
                    return new VerifyOtpResponse
                    {
                        success = false,
                        message = "OTP has expired. Please request a new one.",
                        Data = null
                    };
                }

                // Mark OTP as used
                otpVerification.IsUsed = true;

               

                // Find user and update verification status
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.phone_number);

                if (user == null)
                {
                    _logger.LogError("User not found for verified phone: {phone_number}", request.phone_number);
                    return new VerifyOtpResponse
                    {
                        success = false,
                        message = "User not found",
                        Data = null
                    };
                }

                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(user);

                //var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                //var userAgent = Request.Headers["User-Agent"].ToString();

                var refreshToken =  _jwtService.GenerateRefreshToken();
                // Save refresh token
                string ipAddress = null;// Microsoft.AspNetCore.Http.HttpContext.Connection.RemoteIpAddress?.ToString();
                await _jwtService.SaveRefreshTokenAsync(user.UserID, refreshToken, ipAddress);

                // Update user verification status
                user.PhoneVerified = true;
                user.IsVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
                user.LastLoginAt = DateTime.UtcNow;
              
                user.DeviceToken = request.device_token;
                user.DeviceType = request.device_type;
                await _context.SaveChangesAsync();

                _logger.LogInformation("OTP verified successfully for UserID: {UserID}", user.UserID);

                // Find Customer Profile and update verification status
                var customerprofile = await _context.CustomerProfiles
                    .FirstOrDefaultAsync(u => u.UserID == user.UserID);

               
                var userDto = new userDTO
                {
                    user_id  = user.UserID,
                    customer_id = customerprofile.CustomerID,
                    phone_number = user.PhoneNumber,
                    full_name = user.FullName,
                    email = user.Email,
                    profile_image = user.ProfileImage,
                    is_new_user = customerprofile.IsnewUser,
                    wallet_balance = customerprofile.WalletBalance,
                    referral_code = customerprofile.ReferralCode,
                   
                };
                ////Add Token details in UserTokens
                //var RefreshToken = new RefreshToken
                //{
                //    UserId = user.UserID,
                //    Token = refreshToken,

                //    ExpiresAt=DateTime.UtcNow.AddSeconds(86400),
                //    CreatedAt=DateTime.UtcNow,

                //};
                //_context.UserTokens.Add(UserToken); 
                //await _context.SaveChangesAsync();
                var RefreshTokenLifetimeMinutes = Convert.ToInt32(_configuration["Jwt:RefreshTokenLifetimeMinutes"]);
              
                var tokens = new tokensDTO
                {
                    access_token =accessToken ,
                    refresh_token = refreshToken,
                    expires_in= RefreshTokenLifetimeMinutes * 60,
                };

                return new VerifyOtpResponse
                {
                   success = true,

                    message = customerprofile.IsnewUser? "OTP verified. Please complete your profile": "Login successful",
                    Data = new AuthData
                    {
                        user = userDto,
                        tokens = tokens
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for phone: {PhoneNumber}", request.phone_number);
                return new VerifyOtpResponse
                {
                    success = false,
                    message = "Failed to verify OTP. Please try again.",
                    Data = null
                };
            }
        }
        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                // Get refresh token from database
                var refreshToken = await _jwtService.GetRefreshTokenAsync(request.refresh_token);
                var RefreshTokenLifetimeMinutes = Convert.ToInt32(_configuration["Jwt:RefreshTokenLifetimeMinutes"]);
                if (refreshToken == null || !refreshToken.IsActive)
                {
                    return new RefreshTokenResponse
                    {
                        success = false,
                        message = "Invalid or expired refresh token"
                    };
                }

                // Get user
                var user = refreshToken.User;

                if (!user.IsActive)
                {
                    return new RefreshTokenResponse
                    {
                        success = false,
                        message = "Account is inactive"
                    };
                }

                // Generate new tokens
                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // Save new refresh token
                string ipAddress = null;
                await _jwtService.SaveRefreshTokenAsync(user.UserID, newRefreshToken, ipAddress);

                // Revoke old refresh token
                await _jwtService.RevokeRefreshTokenAsync(request.refresh_token, ipAddress, newRefreshToken);

                var response = new RefreshTokenResponse
                {
                    success = true,
                    message = "Token refreshed successfully",
                    Data = new TokenData
                    {
                        access_token = newAccessToken,
                        refresh_token = newRefreshToken,
                        expires_in = RefreshTokenLifetimeMinutes * 60 // From app.settings
                    }
                };

                return response;

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error verifying OTP for phone: {PhoneNumber}", request.refresh_token);
                return new RefreshTokenResponse
                {
                    success = false,
                    message = "Failed to refreshed Token. Please try again.",
                    Data = null
                };
            }
        }

        public async Task<RefreshTokenResponse> RevokeRefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                string ipAddress = null;// HttpContext.Connection.RemoteIpAddress?.ToString();
                var revoked = await _jwtService.RevokeRefreshTokenAsync(request.refresh_token, ipAddress);

                if (!revoked)
                    return new RefreshTokenResponse
                    {
                        success = false,
                        message = "Invalid refresh token"
                    };

                var response = new RefreshTokenResponse
                {
                    success = true,
                    message = "Logout successfully",
                    
                };
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return new RefreshTokenResponse
                {
                    success = false,
                    message = "Failed to logout. Please try again.",
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
            var RefrealPrefix = _configuration["Referral:RefererName"] ?? throw new InvalidOperationException("ReferelName not configured");
            var lastDigits = phoneNumber.Length >= 4
                ? phoneNumber.Substring(phoneNumber.Length - 4)
                : phoneNumber.PadLeft(4, '0');

            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            var code = $"{RefrealPrefix.ToUpper()}{lastDigits}{randomPart}";

            // Ensure uniqueness
            while (_context.CustomerProfiles.Any(p => p.ReferralCode == code))
            {
                randomPart = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
                code = $"{RefrealPrefix}{lastDigits}{randomPart}";
            }

            return code;
        }
    }
}

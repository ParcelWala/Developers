using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcelwala.DataAccess.Data;
using ParcelwalaAPP.DataAccess.DTOs;
using ParcelwalaAPP.DataAccess.Services;
using ParcelwalaAPP.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ParcelwalaAPP.Controllers.APIs
{
    [Route("auth/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOtpAuthService _otpAuthService;
        private readonly IReferralService _referralService;
        private readonly IWalletService _walletService;

        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ILogger<CustomerController> logger,IOtpAuthService otpAuthService, AppDbContext context, IReferralService referralService, IWalletService walletService)
        {
            _logger = logger;
            _otpAuthService = otpAuthService;
            _context = context;
            _referralService = referralService;
            _walletService = walletService;
        }
       

        /// <summary>
        /// Send OTP to phone number
        /// </summary>
        /// <param name="request">Phone number and purpose</param>
        /// <returns>OTP details and user ID</returns>
        [HttpPost("send-otp")]
        [ProducesResponseType(typeof(SendOtpResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SendOtpResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SendOtpResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SendOtpResponse>> SendOtp(
            [FromBody] SendOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for SendOtp request");
                return BadRequest(new SendOtpResponse
                {
                    success = false,
                    message = "Invalid request data",
                    Data = null
                }); 
            }

            var response = await _otpAuthService.SendOtpAsync(request);

            if (response.success)
            {
                return Ok(response);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        /// <summary>
        /// Verify OTP code
        /// </summary>
        /// <param name="request">Phone number and OTP code</param>
        /// <returns>JWT token and user details</returns>
        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(VerifyOtpResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(VerifyOtpResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(VerifyOtpResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(VerifyOtpResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VerifyOtpResponse>> VerifyOtp(
            [FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for VerifyOtp request");
                return BadRequest(new VerifyOtpResponse
                {
                    success = false,
                    message = "Invalid request data",
                    Data = null
                });
            }

            var response = await _otpAuthService.VerifyOtpAsync(request);

            if (response.success)
            {
                return Ok(response);
            }

            // Return appropriate status code based on message
            if (response.message.Contains("not found"))
            {
                return NotFound(response);
            }

            if (response.message.Contains("Invalid") || response.message.Contains("expired"))
            {
                return BadRequest(response);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        /// <summary>
        /// Complete customer profile with optional referral code
        /// </summary>
        [Authorize]
        [HttpPut("complete-profile")]
        [ProducesResponseType(typeof(CompleteProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CompleteProfileResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CompleteProfileResponse>> CompleteProfile(
            [FromBody] CompleteProfileRequest request)
        {
            try
            {
                // Get User ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new CompleteProfileResponse
                    {
                        success = false,
                        message = "Invalid or missing authentication token"
                    });
                }

                // Validate request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    return BadRequest(new CompleteProfileResponse
                    {
                        success = false,
                        message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                // Get customer
                var customerUser = await _context.Users
                    .Include(c => c.CustomerProfile)
                    .FirstOrDefaultAsync(u =>u.UserID == userId);
                //var customer = await _context.CustomerProfiles
                //    .FirstOrDefaultAsync(c => c.CustomerID == customerId);

                if (customerUser == null)
                {
                    return BadRequest(new CompleteProfileResponse
                    {
                        success = false,
                        message = "Customer not found"
                    });
                }
                var customerProfile = customerUser.CustomerProfile;
                // Check if profile already completed
                bool wasNewUser = customerProfile.IsnewUser;

                // Update customer details
                customerUser.FullName = request.full_name;
                customerUser.Email = request.email;
                customerProfile.IsnewUser = false;

                // Generate referral code if not exists
                if (string.IsNullOrEmpty(customerProfile.ReferralCode))
                {
                    customerProfile.ReferralCode = _referralService.GenerateReferralCode(
                        request.full_name, customerProfile.CustomerID);
                }

                // Handle referral code if provided
                bool referralApplied = false;
                if (!string.IsNullOrWhiteSpace(request.referral_code) && customerProfile.ReferredBy == null)
                {
                    var (isValid, referrerId) = await _referralService.ValidateReferralCodeAsync(
                        request.referral_code, customerProfile.CustomerID);

                    if (isValid && referrerId.HasValue)
                    {
                        customerProfile.ReferredBy = referrerId.Value;
                        referralApplied = await _referralService.ApplyReferralBonusAsync(
                            customerProfile.CustomerID, referrerId.Value);

                        if (referralApplied)
                        {
                            // Reload customer to get updated wallet balance
                            await _context.Entry(customerProfile).ReloadAsync();
                            _logger.LogInformation(
                                "Referral bonus applied for Customer {CustomerId} using code {ReferralCode}",
                                customerProfile.CustomerID, request.referral_code);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Invalid referral code {ReferralCode} for Customer {CustomerId}",
                            request.referral_code, customerProfile.CustomerID);
                    }
                }

                await _context.SaveChangesAsync();

                var message = referralApplied
                    ? "Profile completed successfully. ₹50 referral bonus added to your wallet!"
                    : "Profile completed successfully";

                return Ok(new CompleteProfileResponse
                {
                    success = true,
                    message = message,
                    Data = new CompleteProfileData
                    {
                        user_id = customerUser.UserID,
                        customer_id = customerProfile.CustomerID,
                        phone_number = customerUser.PhoneNumber,
                        full_name = customerUser.FullName,
                        email = customerUser.Email,
                        profile_image = customerUser.ProfileImage,
                        is_new_user = wasNewUser,
                        wallet_balance = customerProfile.WalletBalance,
                        referral_code = customerProfile.ReferralCode ?? string.Empty
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing profile for customer");
                return StatusCode(500, new CompleteProfileResponse
                {
                    success = false,
                    message = "An error occurred while completing your profile"
                });
            }
        }


        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
           
                if (!ModelState.IsValid)
                    return BadRequest(new RefreshTokenResponse
                    {
                        success = false,
                        message = "Refresh token is required"
                    });
                var response = await _otpAuthService.RefreshTokenAsync(request);

                if (response.success)
                {
                    return Ok(response);
                }

                return StatusCode(StatusCodes.Status500InternalServerError, response);

                
           
        }

        /// <summary>
        /// Revoke refresh token (logout)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new RefreshTokenResponse
                {
                    success = false,
                    message = "token is required"
                });
            var response = await _otpAuthService.RevokeRefreshTokenAsync(request);

            if (response.success)
            {
                return Ok(response);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, response);

            
        }
    }

   
}

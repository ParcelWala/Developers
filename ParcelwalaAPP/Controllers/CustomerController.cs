using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParcelwalaAPP.DataAccess.DTOs;
using ParcelwalaAPP.DataAccess.Services;
using System.ComponentModel.DataAnnotations;

namespace ParcelwalaAPP.Controllers
{
    [Route("auth/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IOtpService _otpService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ILogger<CustomerController> logger,IOtpService otpService)
        {
            _logger = logger;
            _otpService = otpService;
        }
        [HttpPost("send-otp")]

        public async Task<ActionResult<SendOtpResponse>> SendOtp([FromBody] SendOtpRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new SendOtpResponse
                    {
                        Success = false,
                        Message = "Validation failed",
                        Data = new { Errors = errors }
                    });
                }

                var (success, message, OTPVerifications) = await _otpService.SendOtpAsync(
                    request.phone_number,
                    request.country_code,
                    request.purpose
                );

                if (!success)
                {
                    return BadRequest(new SendOtpResponse
                    {
                        Success = false,
                        Message = message
                    });
                }

                return Ok(new SendOtpResponse
                {
                    Success = true,
                    Message = message,
                    Data = new
                    {
                        otp_id = OTPVerifications?.OTPCode,
                        expires_in = OTPVerifications?.ExpiresAt,
                        can_resend_after = "5 minutes"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendOtp endpoint");

                return StatusCode(500, new SendOtpResponse
                {
                    Success = false,
                    Message = "An internal error occurred. Please try again later."
                });
            }
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
                    Success = false,
                    Message = "Invalid request data",
                    Data = null
                });
            }

            var response = await _otpService.VerifyOtpAsync(request);

            if (response.Success)
            {
                return Ok(response);
            }

            // Return appropriate status code based on message
            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }

            if (response.Message.Contains("Invalid") || response.Message.Contains("expired"))
            {
                return BadRequest(response);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }

   
}

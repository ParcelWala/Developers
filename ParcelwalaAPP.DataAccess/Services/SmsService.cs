using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.Services
{
    public interface ISmsService
    {
        Task<bool> SendOtpSmsAsync(string phoneNumber, string otpCode);
    }

    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly IConfiguration _configuration;

        public SmsService(ILogger<SmsService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendOtpSmsAsync(string phoneNumber, string otpCode)
        {
            try
            {
                // Mock SMS sending - In production, integrate with Twilio, AWS SNS, or other SMS provider
                _logger.LogInformation("Sending SMS to {PhoneNumber}: Your OTP is {OtpCode}",
                    phoneNumber, otpCode);

                // Simulate network delay
                await Task.Delay(500);

                // In production, use actual SMS gateway:
                /*
                // Example with Twilio:
                var twilioAccountSid = _configuration["Twilio:AccountSid"];
                var twilioAuthToken = _configuration["Twilio:AuthToken"];
                var twilioPhoneNumber = _configuration["Twilio:PhoneNumber"];
                
                TwilioClient.Init(twilioAccountSid, twilioAuthToken);
                
                var message = await MessageResource.CreateAsync(
                    body: $"Your OTP code is: {otpCode}. Valid for 5 minutes.",
                    from: new PhoneNumber(twilioPhoneNumber),
                    to: new PhoneNumber(phoneNumber)
                );
                
                return message.Status != MessageResource.StatusEnum.Failed;
                */

                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }
    }
}

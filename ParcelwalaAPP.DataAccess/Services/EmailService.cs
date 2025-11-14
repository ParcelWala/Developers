using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.Services
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string email, string fullName, string otpCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string email, string fullName, string otpCode)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "kishor.farad05@gmail.com";
                var senderPassword = _configuration["EmailSettings:SenderPassword"] ?? "puec grzf qtof vtgv";
                var senderName = _configuration["EmailSettings:SenderName"] ?? "Parcelwala API";

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Your OTP Code - Parcelwala API",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Hello {fullName},</h2>
                            <p>Your OTP code is:</p>
                            <h1 style='color: #4CAF50; font-size: 36px; letter-spacing: 5px;'>{otpCode}</h1>
                            <p>This code is valid for <strong>5 minutes</strong>.</p>
                            <p>If you didn't request this code, please ignore this email.</p>
                            <br>
                            <p>Best regards,<br>Parcelwala API Team</p>
                        </body>
                        </html>
                    ",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("OTP email sent successfully to {Email}", email);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending OTP email to {Email}", email);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", email);
                return false;
            }
        }
    }
}

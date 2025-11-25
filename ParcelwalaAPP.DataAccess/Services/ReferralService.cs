using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Parcelwala.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.Services
{
    public interface IReferralService
    {
        string GenerateReferralCode(string fullName, int customerId);
        Task<(bool isValid, int? referrerId)> ValidateReferralCodeAsync(string referralCode, int currentCustomerId);
        Task<bool> ApplyReferralBonusAsync(int newCustomerId, int referrerId);
    }

    public class ReferralService : IReferralService
    {
        private readonly AppDbContext _context;
        private readonly IWalletService _walletService;
        private readonly ILogger<ReferralService> _logger;
        private readonly IConfiguration _configuration;

        public ReferralService(
            AppDbContext context,
            IWalletService walletService,
            ILogger<ReferralService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _walletService = walletService;
            _logger = logger;
            _configuration = configuration;
        }

        public string GenerateReferralCode(string fullName, int customerId)
        {
            // Generate format: FIRSTNAME + 4 random chars + last 2 digits of ID
            var firstName = fullName.Split(' ')[0].ToUpper();
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            var idPart = customerId.ToString().PadLeft(2, '0').Substring(0, 2);

            return $"{firstName}{randomPart}{idPart}";
        }

        public async Task<(bool isValid, int? referrerId)> ValidateReferralCodeAsync(
            string referralCode, int currentCustomerId)
        {
            if (string.IsNullOrWhiteSpace(referralCode))
                return (false, null);

            var referrer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c=>c.ReferralCode==referralCode.ToUpper());
                //.FirstOrDefaultAsync(c => c.ReferralCode == referralCode.ToUpper());

            if (referrer == null)
            {
                _logger.LogWarning("Invalid referral code: {ReferralCode}", referralCode);
                return (false, null);
            }

            if (referrer.CustomerID == currentCustomerId)
            {
                _logger.LogWarning("Customer {CustomerId} tried to use own referral code", currentCustomerId);
                return (false, null);
            }

            return (true, referrer.CustomerID);
        }

        public async Task<bool> ApplyReferralBonusAsync(int newCustomerId, int referrerId)
        {
            try
            {
                var bonusAmount = decimal.Parse(_configuration["Referral:BonusAmount"] ?? "50");
                var referrerBonusAmount = decimal.Parse(_configuration["Referral:ReferrerBonusAmount"] ?? "50");

                // Add bonus to new customer
                await _walletService.AddCreditAsync(
                    newCustomerId,
                    bonusAmount,
                    "REFERRAL_BONUS",
                    $"Referral bonus for joining via referral code",
                    null
                );

                // Add bonus to referrer
                await _walletService.AddCreditAsync(
                    referrerId,
                    referrerBonusAmount,
                    "REFERRAL_BONUS",
                    $"Referral bonus for referring new customer",
                    null
                );

                _logger.LogInformation(
                    "Referral bonus applied: New Customer {NewCustomerId} and Referrer {ReferrerId}",
                    newCustomerId, referrerId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying referral bonus");
                return false;
            }
        }
    }
}

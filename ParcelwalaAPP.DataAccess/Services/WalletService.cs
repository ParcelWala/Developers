using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parcelwala.DataAccess.Data;
using ParcelwalaAPP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.Services
{
    public interface IWalletService
    {
        Task<decimal> GetBalanceAsync(int customerId);
        Task<bool> AddCreditAsync(int customerId, decimal amount, string category, string description, int? referenceId);
        Task<bool> DeductAmountAsync(int customerId, decimal amount, string category, string description, int? referenceId);
        Task<List<WalletTransactions>> GetTransactionHistoryAsync(int customerId, int pageNumber = 1, int pageSize = 20);
    }

    public class WalletService : IWalletService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WalletService> _logger;

        public WalletService(AppDbContext context, ILogger<WalletService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<decimal> GetBalanceAsync(int customerId)
        {
            var customer = await _context.CustomerProfiles.FindAsync(customerId);
            return customer?.WalletBalance ?? 0;
        }

        public async Task<bool> AddCreditAsync(
            int customerId, decimal amount, string category, string description, int? referenceId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var customer = await _context.CustomerProfiles.FindAsync(customerId);
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found: {CustomerId}", customerId);
                    return false;
                }

                var balanceBefore = customer.WalletBalance;
                customer.WalletBalance += amount;
                var balanceAfter = customer.WalletBalance;

                var walletTransaction = new WalletTransactions
                {
                    UserID = customer.UserID,
                    CustomerID= customerId,
                    Amount = amount,
                    TransactionType = "CREDIT",
                    //Category = category,
                    Description = description,
                    ReferenceID = referenceId,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WalletTransaction.Add(walletTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Credit added: Customer {CustomerId}, Amount {Amount}, New Balance {Balance}",
                    customerId, amount, balanceAfter);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error adding credit to wallet for Customer {CustomerId}", customerId);
                return false;
            }
        }

        public async Task<bool> DeductAmountAsync(
            int customerId, decimal amount, string category, string description, int? referenceId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var customer = await _context.CustomerProfiles.FindAsync(customerId);
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found: {CustomerId}", customerId);
                    return false;
                }

                if (customer.WalletBalance < amount)
                {
                    _logger.LogWarning("Insufficient balance: Customer {CustomerId}", customerId);
                    return false;
                }

                var balanceBefore = customer.WalletBalance;
                customer.WalletBalance -= amount;
                var balanceAfter = customer.WalletBalance;

                var walletTransaction = new WalletTransactions
                {
                    UserID = customer.UserID,
                    CustomerID = customerId,
                    Amount = amount,
                    TransactionType = "DEBIT",
                   // Category = category,
                    Description = description,
                    ReferenceID = referenceId,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WalletTransaction.Add(walletTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Amount deducted: Customer {CustomerId}, Amount {Amount}, New Balance {Balance}",
                    customerId, amount, balanceAfter);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deducting amount from wallet for Customer {CustomerId}", customerId);
                return false;
            }
        }

        public async Task<List<WalletTransactions>> GetTransactionHistoryAsync(
            int customerId, int pageNumber = 1, int pageSize = 20)
        {
            return await _context.WalletTransaction
                .Where(wt => wt.CustomerID == customerId)
                .OrderByDescending(wt => wt.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

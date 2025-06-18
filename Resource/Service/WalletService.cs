using Wallet.Models;
using RazorWithSQLiteApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElsSaleWallet.Services
{
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WalletService> _logger;

        public WalletService(ApplicationDbContext context, ILogger<WalletService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<WalletBalance> GetFullWalletDataAsync(int userId)
        {
            try
            {
                var wallet = await _context.Wallets
                    .Include(w => w.Transactions)
                    .Include(w => w.PaymentRequests)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    throw new KeyNotFoundException($"Wallet for user {userId} not found");
                }

                return wallet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting full wallet data for user {UserId}", userId);
                throw;
            }
        }

    }
}
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

        public async Task<WalletBalance> CreateWalletAsync(Wallet.DTOs.CreateWalletRequest request)
        {
            if (await _context.Wallets.AnyAsync(w => w.UserId == request.UserId))
            {
                throw new InvalidOperationException("Wallet already exists for user");
            }

            var wallet = new WalletBalance
            {
                UserId = request.UserId,
                Name = request.Name ?? "Unknown",
                Email = request.Email ?? "unknown@example.com",
                Balance = request.InitialBalance
            };

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();

            var fullWallet = await _context.Wallets
                .Include(w => w.Transactions)
                .Include(w => w.PaymentRequests)
                .FirstOrDefaultAsync(w => w.Id == wallet.Id);

            return fullWallet!;
        }



        public async Task<WalletBalance> AddMoneyAsync(int userId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be positive");
            }

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                throw new KeyNotFoundException($"No wallet found for user {userId}");
            }

            wallet.Balance += amount;

            var transaction = new Transaction
            {
                WalletId = wallet.Id,
                Amount = amount,
                Type = "Credit",
                Date = DateTime.UtcNow,
                Status = "Completed"
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return wallet;
        }

        public async Task<PaymentRequest> SendMoneyAsync(int senderWalletId, string recipientIdentifier, decimal amount, string description)
        {
            // 1. Validate Amount
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be positive.");
            }

            // 2. Find Sender's Wallet
            var senderWallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == senderWalletId); // Find by WalletId, not UserId

            if (senderWallet == null)
            {
                throw new KeyNotFoundException($"No wallet found for sender with ID: {senderWalletId}.");
            }

            // 3. Check Sender's Balance
            if (senderWallet.Balance < amount)
            {
                throw new InvalidOperationException("Insufficient balance in sender's wallet.");
            }

            // 4. Find Recipient's Wallet
            // Note: recipientIdentifier is used to find by email OR name.
            var recipientWallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Email == recipientIdentifier || w.Name == recipientIdentifier);

            if (recipientWallet == null)
            {
                throw new KeyNotFoundException($"No wallet found for recipient with identifier: '{recipientIdentifier}'.");
            }

            // 5. Create Payment Request
            var paymentRequest = new PaymentRequest
            {
                SenderWalletId = senderWallet.Id, // The ID of the wallet initiating the payment
                WalletId = senderWallet.Id,      // This might be redundant if SenderWalletId is used,
                                                 // but keeping it as per your PaymentRequest model's FK
                Recipientor = recipientIdentifier, // Store the identifier used to find the recipient
                Amount = amount,
                Description = description,       // Now correctly assigned from input
                Status = "Pending",              // Initial status
                CreatedAt = DateTime.UtcNow
            };

            // Optional: If you want to also store the recipient's actual WalletId in PaymentRequest
            // Make sure your PaymentRequest model has an `int RecipientId { get; set; }` property
            // If so, uncomment the line below:
            // paymentRequest.RecipientId = recipientWallet.Id;


            // 6. Deduct Amount from Sender (Process immediately)
            senderWallet.Balance -= amount;
            var debitTransaction = new Transaction
            {
                WalletId = senderWallet.Id,
                Amount = -amount, // Negative for debit
                Type = "Debit",
                Status = "Completed", // Assuming immediate processing
                Date = DateTime.UtcNow
            };

            // 7. Credit Amount to Recipient (Process immediately)
            recipientWallet.Balance += amount;
            var creditTransaction = new Transaction
            {
                WalletId = recipientWallet.Id,
                Amount = amount, // Positive for credit
                Type = "Credit",
                Status = "Completed", // Assuming immediate processing
                Date = DateTime.UtcNow
            };

            // 8. Add to Context and Save Changes
            _context.PaymentRequests.Add(paymentRequest);
            _context.Transactions.Add(debitTransaction);
            _context.Transactions.Add(creditTransaction);

            await _context.SaveChangesAsync();

            return paymentRequest;
        }

    }
}
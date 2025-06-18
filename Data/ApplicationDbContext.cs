namespace RazorWithSQLiteApp.Data;

using Microsoft.EntityFrameworkCore;
using Wallet.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WalletBalance> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<PaymentRequest> PaymentRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(t => t.WalletId);

        modelBuilder.Entity<PaymentRequest>()
            .HasOne(pr => pr.Wallet)
            .WithMany(w => w.PaymentRequests)
            .HasForeignKey(pr => pr.WalletId);

        modelBuilder.Entity<WalletBalance>()
                    .HasIndex(w => w.UserId)
                    .IsUnique(); // Ensure one wallet per user
        // Seed data
        modelBuilder.Entity<WalletBalance>().HasData(
            new WalletBalance
            {
                Id = 1,
                UserId = 1, // Kimsreng
                Balance = 1234.56m
            },
            new WalletBalance
            {
                Id = 2,
                UserId = 2, // User
                Balance = 500.00m
            }
        );

        modelBuilder.Entity<Transaction>().HasData(
            new Transaction
            {
                Id = 1,
                WalletId = 1,
                Type = "Payment Received",
                Date = new DateTime(2025, 6, 17, 14, 30, 0),
                Amount = 125.00m,
                Status = "Completed"
            },
            new Transaction
            {
                Id = 2,
                WalletId = 1,
                Type = "Product Purchase",
                Date = new DateTime(2025, 6, 16, 16, 15, 0),
                Amount = -89.99m,
                Status = "Completed"
            }
        );

        modelBuilder.Entity<PaymentRequest>().HasData(
            new PaymentRequest
            {
                Id = 1,
                WalletId = 1,
                RecipientId = "user123@example.com",
                Amount = 100.00m,
                Description = "Test Payment",
                Status = "Pending",
                CreatedAt = new DateTime(2025, 6, 17, 11, 0, 0)
            }
        );

        base.OnModelCreating(modelBuilder);
    }
}
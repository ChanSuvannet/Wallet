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
        base.OnModelCreating(modelBuilder);

        // Configure WalletBalance entity
        modelBuilder.Entity<WalletBalance>(entity =>
        {
            // Set primary key
            entity.HasKey(w => w.Id);

            // Configure one-to-many relationship with Transactions
            entity.HasMany(w => w.Transactions)
                  .WithOne(t => t.Wallet)
                  .HasForeignKey(t => t.WalletId)
                  .OnDelete(DeleteBehavior.Cascade); // Delete transactions when wallet is deleted

            // Configure one-to-many relationship with PaymentRequests
            entity.HasMany(w => w.PaymentRequests)
                  .WithOne(pr => pr.Wallet)
                  .HasForeignKey(pr => pr.WalletId)
                  .OnDelete(DeleteBehavior.Cascade); // Delete payment requests when wallet is deleted

            // Ensure each user has only one wallet
            entity.HasIndex(w => w.UserId)
                  .IsUnique();

            // Configure decimal precision for Balance
            entity.Property(w => w.Balance)
                  .HasColumnType("decimal(18,2)");
        });

        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            // Configure required fields
            entity.Property(t => t.Type)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(t => t.Status)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(t => t.Amount)
                  .HasColumnType("decimal(18,2)");
        });

        // Configure PaymentRequest entity
        modelBuilder.Entity<PaymentRequest>(entity =>
        {
            entity.HasKey(pr => pr.Id);

            // Configure required fields
            entity.Property(pr => pr.Recipientor)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(pr => pr.Status)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(pr => pr.Amount)
                  .HasColumnType("decimal(18,2)");

            entity.Property(pr => pr.Description)
                  .HasMaxLength(500);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed wallet data
        modelBuilder.Entity<WalletBalance>().HasData(
        new WalletBalance
        {
            Id = 1,
            UserId = 1, // Kimsreng
            Email = "kimsreng@gmail.com",
            Name = "kimsreng",
            Balance = 1234.56m
        },
        new WalletBalance
        {
            Id = 2,
            UserId = 2, // User
            Email = "admin2@gmail.com",
            Name = "admin",
            Balance = 500.00m
        }
    );

        // Seed transaction data
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

        // Seed payment request data
        modelBuilder.Entity<PaymentRequest>().HasData(
            new PaymentRequest
            {
                Id = 1,
                WalletId = 1,
                Recipientor = "net@gmail.com",
                Amount = 100.00m,
                Description = "Test Payment",
                Status = "Pending",
                CreatedAt = new DateTime(2025, 6, 17, 11, 0, 0)
            }
        );
    }
}
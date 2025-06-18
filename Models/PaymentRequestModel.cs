namespace Wallet.Models;

using System;

public class PaymentRequest
{
    public int Id { get; set; } // Primary key
    public int WalletId { get; set; } // Foreign key
    public WalletBalance Wallet { get; set; } // Navigation property
    public int SenderWalletId { get; set; }
    public string? Recipientor { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
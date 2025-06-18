namespace Wallet.Models;

using System;

public class Transaction
{
    public int Id { get; set; } // Primary key
    public int WalletId { get; set; } // Foreign key
    public WalletBalance Wallet { get; set; } // Navigation property
    public string Type { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
}
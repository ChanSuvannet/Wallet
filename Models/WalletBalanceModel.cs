namespace Wallet.Models;

using System.Collections.Generic;

public class WalletBalance
{
    public int Id { get; set; } // Primary key
    public int UserId { get; set; } // User ID
    public decimal Balance { get; set; }
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    public List<PaymentRequest> PaymentRequests { get; set; } = new List<PaymentRequest>();
}
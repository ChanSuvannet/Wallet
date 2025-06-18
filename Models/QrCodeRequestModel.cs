namespace Wallet.Models;

using System;

public class QrCodeRequest
{
    public string PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string MerchantId { get; set; }
    public DateTime Timestamp { get; set; }
}
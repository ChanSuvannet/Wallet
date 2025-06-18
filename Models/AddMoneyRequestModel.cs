namespace Wallet.Models;

public class AddMoneyRequest
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
}
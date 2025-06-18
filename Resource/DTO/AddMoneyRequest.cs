namespace Wallet.DTOs
{
     public class AddMoneyRequest
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
    }
    
}
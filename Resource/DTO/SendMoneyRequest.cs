namespace Wallet.DTOs
{
    public class SendMoneyRequest
    {
        public int UserId { get; set; }
        public string RecipientId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
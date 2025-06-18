namespace Wallet.DTOs
{
    public class SendMoneyRequest
    {
        public int SenderUserId { get; set; }
        public string Recipientor { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
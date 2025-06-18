namespace Wallet.DTOs
{
    public class CreateWalletRequest
    {
        public int UserId { get; set; }
        public decimal InitialBalance { get; set; } = 0;
    }

}
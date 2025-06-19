namespace Wallet.DTOs
{
    public class CreateWalletRequest
    {
        public int UserId { get; set; }
        public decimal InitialBalance { get; set; } = 0;
        public string Name { get; set; } = "Unknown";
        public string Email { get; set; } = "unknown@example.com";
    }
}

using Wallet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.OpenApi.Any;

namespace ElsSaleWallet.Services
{
    public interface IWalletService
    {
        Task<WalletBalance> GetFullWalletDataAsync(int userId);
        Task<WalletBalance> CreateWalletAsync(int userId);
    }
}
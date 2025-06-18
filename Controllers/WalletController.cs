using Wallet.Models;
using Microsoft.AspNetCore.Mvc;
using ElsSaleWallet.Services;
using System.Threading.Tasks;
using DTOs = Wallet.DTOs;
using Models = Wallet.Models;
namespace ElsSaleWallet.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        // GET: /wallet/page
        [HttpGet("page/{userId}")]
        public IActionResult Index(int userId)
        {
            ViewBag.UserId = userId;
            return View(new WalletBalance());
        }

        // GET: /wallet/wallet/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetWalletData(int userId)
        {
            try
            {
                var wallet = await _walletService.GetFullWalletDataAsync(userId);
                return Ok(wallet);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /api/walletapi/add-money
        [HttpPost("add-money")]
        public async Task<IActionResult> AddMoney([FromBody] DTOs.AddMoneyRequest request)
        {
            try
            {
                // Implement your add money logic here
                return Ok(new { Message = "Money added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.ErrorResponse // Now this works
                {
                    Message = "Failed to add money",
                    Details = ex.Message
                });
            }
        }

        // POST: /api/walletapi/send-money
        [HttpPost("send-money")]
        public async Task<IActionResult> SendMoney([FromBody] DTOs.SendMoneyRequest request)
        {
            try
            {
                // Implement your send money logic here
                return Ok(new { Message = "Payment request created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.ErrorResponse
                {
                    Message = "Failed to send money",
                    Details = ex.Message
                });
            }
        }
    }
}

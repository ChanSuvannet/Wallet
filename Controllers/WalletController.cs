using Wallet.Models;
using Microsoft.AspNetCore.Mvc;
using ElsSaleWallet.Services;
using System.Threading.Tasks;
using DTOs = Wallet.DTOs;
using Models = Wallet.Models;
using System.ComponentModel.DataAnnotations;
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

        // GET: /wallet/check/{userId}
        [HttpGet("check/{userId}")]
        public async Task<IActionResult> CheckWalletExists(int userId)
        {
            try
            {
                var wallet = await _walletService.GetFullWalletDataAsync(userId);
                return Ok(new { exists = true }); // Wallet exists
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { exists = false }); // Wallet not found
            }
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

        // POST: /wallet/create/{userId}
        [HttpPost("create/{userId}")]
        public async Task<IActionResult> CreateWallet([Required] int userId)
        {
            try
            {
                var wallet = await _walletService.CreateWalletAsync(userId);
                return Ok(wallet);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: /api/walletapi/add-money
        [HttpPost("add-money")]
        public async Task<IActionResult> AddMoney([FromBody] DTOs.AddMoneyRequest request)
        {
            try
            {
                // Validate UserId against token
                var tokenUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var wallet = await _walletService.AddMoneyAsync(request.UserId, request.Amount);
                return Ok(new { Message = "Money added successfully", Wallet = wallet });
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.ErrorResponse
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
                // Validate SenderUserId against token
                var tokenUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var paymentRequest = await _walletService.SendMoneyAsync(request.SenderUserId, request.Recipientor, request.Amount, request.Description);
                return Ok(new { Message = "Payment request created successfully", PaymentRequest = paymentRequest });
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

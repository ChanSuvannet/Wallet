using Wallet.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElsSaleWallet.Controllers
{
    public class WalletController : Controller
    {
        private static decimal _currentBalance = 1234.56m; // Simulated balance
        private static List<Transaction> _transactions = new List<Transaction>
        {
            new Transaction { Type = "Payment Received", Date = DateTime.Now, Amount = 125.00m, Status = "Completed" },
            new Transaction { Type = "Product Purchase", Date = DateTime.Now.AddDays(-1), Amount = -89.99m, Status = "Completed" },
            new Transaction { Type = "Wallet Top-up", Date = DateTime.Now.AddDays(-2), Amount = 500.00m, Status = "Completed" },
            new Transaction { Type = "Refund", Date = DateTime.Now.AddDays(-3), Amount = 45.50m, Status = "Completed" }
        };

        // MVC Action for Wallet View
        [HttpGet]
        public IActionResult Index()
        {
            var model = new WalletBalance { Balance = _currentBalance };
            return View(model);
        }

        // API: Get Wallet Balance
        [HttpGet]
        [Route("api/wallet/balance")]
        public IActionResult GetBalance()
        {
            return Ok(new { Balance = _currentBalance });
        }

        // API: Get Transactions
        [HttpGet]
        [Route("api/wallet/transactions")]
        public IActionResult GetTransactions()
        {
            return Ok(_transactions);
        }

        // API: Generate QR Code
        [HttpPost]
        [Route("api/wallet/generate-qr")]
        public IActionResult GenerateQrCode([FromBody] QrCodeRequest qrData)
        {
            // Simulate QR code generation (in real app, use a QR code library)
            return Ok(new
            {
                Success = true,
                QrCode = $"QR_{qrData.PaymentId}",
                PaymentId = qrData.PaymentId,
                Amount = qrData.Amount,
                Status = "Pending"
            });
        }

        // API: Check Payment Status
        [HttpGet]
        [Route("api/wallet/payment-status/{paymentId}")]
        public IActionResult CheckPaymentStatus(string paymentId)
        {
            // Simulate payment status check
            var random = new Random();
            var status = random.Next(0, 3) switch
            {
                0 => "Pending",
                1 => "Completed",
                _ => "Failed"
            };

            return Ok(new { Status = status, PaymentId = paymentId });
        }

        // API: Add Money
        [HttpPost]
        [Route("api/wallet/add-money")]
        public IActionResult AddMoney([FromBody] AddMoneyRequest request)
        {
            if (request.Amount <= 0 || string.IsNullOrEmpty(request.PaymentMethod))
            {
                return BadRequest(new { Success = false, Message = "Invalid amount or payment method" });
            }

            _currentBalance += request.Amount;
            _transactions.Add(new Transaction
            {
                Type = "Wallet Top-up",
                Date = DateTime.Now,
                Amount = request.Amount,
                Status = "Completed"
            });

            return Ok(new { Success = true, NewBalance = _currentBalance });
        }

        // API: Make Payment
        [HttpPost]
        [Route("api/wallet/make-payment")]
        public IActionResult MakePayment([FromBody] PaymentRequest request)
        {
            if (request.Amount <= 0 || string.IsNullOrEmpty(request.RecipientId))
            {
                return BadRequest(new { Success = false, Message = "Invalid recipient or amount" });
            }

            if (request.Amount > _currentBalance)
            {
                return BadRequest(new { Success = false, Message = "Insufficient balance" });
            }

            _currentBalance -= request.Amount;
            _transactions.Add(new Transaction
            {
                Type = "Payment Sent",
                Date = DateTime.Now,
                Amount = -request.Amount,
                Status = "Completed"
            });

            return Ok(new { Success = true, NewBalance = _currentBalance });
        }
    }
}
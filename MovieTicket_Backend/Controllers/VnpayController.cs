using Microsoft.AspNetCore.Mvc;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;
using VNPAY.NET;
using System.Security.Cryptography;
using System.Text;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using System.Transactions;
using MovieTicket_Backend.RepositoryInpl;

namespace MovieTicket_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnpayController : Controller
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly PaymentRepository _paymentRepository;

        public VnpayController(IVnpay vnPayservice, IConfiguration configuration)
        {
            _vnpay = vnPayservice;
            _configuration = configuration;

            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);
            _paymentRepository = new PaymentRepository(new DbConnectionFactory(configuration), configuration);
        }

        /// <summary>
        /// Tạo url thanh toán
        /// </summary>
        /// <param name="money">Số tiền phải thanh toán</param>
        /// <param name="description">Mô tả giao dịch</param>
        /// <returns></returns>
        [HttpGet("CreatePaymentUrl")]
        public ActionResult<string> CreatePaymentUrl(double money, string description)
        {
            try
            {
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext); // Lấy địa chỉ IP của thiết bị thực hiện giao dịch

                var request = new PaymentRequest
                {
                    PaymentId = DateTime.Now.Ticks,
                    Money = money,
                    Description = description,
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY, // Tùy chọn. Mặc định là tất cả phương thức giao dịch
                    CreatedDate = DateTime.Now, // Tùy chọn. Mặc định là thời điểm hiện tại
                    Currency = Currency.VND, // Tùy chọn. Mặc định là VND (Việt Nam đồng)
                    Language = DisplayLanguage.Vietnamese // Tùy chọn. Mặc định là tiếng Việt
                };

                var paymentUrl = _vnpay.GetPaymentUrl(request);

                return Created(paymentUrl, paymentUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Thực hiện hành động sau khi thanh toán. URL này cần được khai báo với VNPAY để API này hoạt đồng (ví dụ: http://localhost:1234/api/Vnpay/IpnAction)
        /// Dùng ngrok để tạo domain, đăng ký với VNPAY, đọc kết quả giao dịch
        /// Ví dụ kết quả callback sau khi thanh toán:
        ///     https://liked-absolutely-polliwog.ngrok-free.app/api/Vnpay/Callback?
        ///     vnp_Amount=2000000&vnp_BankCode=NCB&vnp_BankTranNo=VNP14908728&vnp_CardType=ATM&
        ///     vnp_OrderInfo=k&vnp_PayDate=20250415170038&vnp_ResponseCode=00&vnp_TmnCode=8PT36G81&
        ///     vnp_TransactionNo=14908728&vnp_TransactionStatus=00&vnp_TxnRef=638803331919845275&
        ///     vnp_SecureHash=8d574e26049e2e07e0e164cf8be456750a4dfdf70b5b2f9f743b8a5bbcfae544406f850ebeeec796d744349028d096d99858f1ac3daa1707ac907c89b63ac06b
        ///     Thay Callback bằng IpnAction là có thể lấy được kết quả giao dịch với tham số như trên
        ///     Dùng callback gọi bên app để cập nhật kết quả giao dịch
        ///     Dùng IpnAction để cập nhật DB
        /// </summary>
        /// <returns></returns>
        [HttpGet("IpnAction")]
        public async Task<IActionResult> IpnAction()
        {
            var vnpParams = Request.Query;

            // Create a sorted dictionary of all vnp_ parameters
            var vnpData = HttpContext.Request.Query
                .Where(kv => kv.Key.StartsWith("vnp_"))
                .OrderBy(kv => kv.Key)
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            // Remove the secure hash as it's not part of the data to verify
            var secureHash = vnpData["vnp_SecureHash"];
            vnpData.Remove("vnp_SecureHash");
            vnpData.Remove("vnp_SecureHashType");

            // Verify the hash
            if (!ValidateSignature(vnpData, secureHash))
            {
                return BadRequest(new { RspCode = "97", Message = "Invalid signature" });
            }

            // Get payment details
            string orderRef = vnpData["vnp_TxnRef"];
            string responseCode = vnpData["vnp_ResponseCode"];
            string transactionStatus = vnpData["vnp_TransactionStatus"];

            // Update your database based on payment status
            string paymentStatus = (responseCode == "00" && transactionStatus == "00")
                ? "successful"
                : "failed";

            // Return acknowledgment to VNPay
            return Ok(new { RspCode = "00", Message = paymentStatus });
        }

        private bool ValidateSignature(Dictionary<string, string> data, string secureHash)
        {
            // Build the hash data string
            var hashData = string.Join("&", data.Select(kv => $"{kv.Key}={kv.Value}"));

            // Calculate hash with your secret key
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_configuration["Vnpay:HashSecret"]));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(hashData));
            var calculatedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return calculatedHash == secureHash.ToLower();
        }

        /// <summary>
        /// Trả kết quả thanh toán về cho người dùng
        /// </summary>
        /// <returns></returns>
        [HttpGet("Callback")]
        public async Task<ActionResult<PaymentResult>> CallbackAsync()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var vnpParams = Request.Query;

                    // Create a sorted dictionary of all vnp_ parameters
                    var vnpData = HttpContext.Request.Query
                        .Where(kv => kv.Key.StartsWith("vnp_"))
                        .OrderBy(kv => kv.Key)
                        .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

                    // Remove the secure hash as it's not part of the data to verify
                    var secureHash = vnpData["vnp_SecureHash"];
                    vnpData.Remove("vnp_SecureHash");
                    vnpData.Remove("vnp_SecureHashType");

                    // Verify the hash
                    if (!ValidateSignature(vnpData, secureHash))
                    {
                        return BadRequest(new { RspCode = "97", Message = "Invalid signature" });
                    }

                    // Get payment details
                    string orderRef = vnpData["vnp_TxnRef"];
                    string responseCode = vnpData["vnp_ResponseCode"];
                    string transactionStatus = vnpData["vnp_TransactionStatus"];
                    var result = "";
                    if (responseCode == "00" && transactionStatus == "00")
                    {
                        result = await _paymentRepository.InsertPayment(
                            orderRef,
                            13,
                            vnpData["vnp_TransactionNo"],
                            vnpData["vnp_BankCode"],
                            transactionStatus,
                            responseCode,
                            double.Parse(vnpData["vnp_Amount"]),
                            vnpData["vnp_OrderInfo"]
                        );
                    }
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);

                    if (paymentResult.IsSuccess)
                    {
                        return Ok(new {result, paymentResult});
                    }

                    return BadRequest(paymentResult);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }
    }
}

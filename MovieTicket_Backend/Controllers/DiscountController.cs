using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.ModelDTOs;
using MovieTicket_Backend.RepositoryImpl;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/v1/discount")]
    [ApiController]
    public class DiscountController : Controller
    {
        private readonly ILogger<DiscountController> _logger;
        private readonly DiscountRepository _discountRepository;
        public DiscountController(ILogger<DiscountController> logger, DiscountRepository discountRepository)
        {
            _logger = logger;
            _discountRepository = discountRepository;
        }
        [HttpGet("")]
        public async Task<IActionResult> GetAllDiscounts(int page, int pageSize)
        {
            var discounts = await _discountRepository.GetAllDiscounts(page, pageSize);
            if (discounts == null)
            {
                return NotFound(new { status = "error", message = "Discounts not found" });
            }
            return Ok(new { status = "success", data = discounts });
        }
        [HttpGet("{discountId}")]
        public async Task<IActionResult> GetDiscountById(int discountId)
        {
            var discount = await _discountRepository.GetDiscountById(discountId);
            if (discount == null)
            {
                return NotFound(new { status = "error", message = "Discount not found" });
            }
            return Ok(new { status = "success", data = discount });
        }
        [HttpPost("")]
        public async Task<IActionResult> CreateDiscount([FromBody] DiscountDTO discount)
        {
            if (discount == null)
            {
                return BadRequest(new { status = "error", message = "Invalid discount data" });
            }
            var result = await _discountRepository.CreateDiscount(discount);
            if (result)
            {
                return Ok(new { status = "success", message = "Discount created successfully" });
            }
            return BadRequest(new { status = "error", message = "Failed to create discount" });
        }
        [HttpPut("{discountId}")]
        public async Task<IActionResult> UpdateDiscount(int discountId, [FromBody] DiscountDTO discount)
        {
            if (discount == null)
            {
                return BadRequest(new { status = "error", message = "Invalid discount data" });
            }
            var result = await _discountRepository.UpdateDiscount(discountId, discount);
            if (result)
            {
                return Ok(new { status = "success", message = "Discount updated successfully" });
            }
            return BadRequest(new { status = "error", message = "Failed to update discount" });
        }
        [HttpDelete("{discountId}")]
        public async Task<IActionResult> DeleteDiscount(int discountId)
        {
            var result = await _discountRepository.DeleteDiscount(discountId);
            if (result)
            {
                return Ok(new { status = "success", message = "Discount deleted successfully" });
            }
            return BadRequest(new { status = "error", message = "Failed to delete discount" });
        }
        [HttpGet("{discountId}/codes")]
        public async Task<IActionResult> GetCodesGenerated(int discountId)
        {
            var discountCodes = await _discountRepository.GetCodesGenerated(discountId);
            if (discountCodes == null)
            {
                return NotFound(new { status = "error", message = "Discount codes not found" });
            }
            return Ok(new { status = "success", data = discountCodes });
        }
        [HttpPost("{discountId}/codes")]
        public async Task<IActionResult> CreateBatchDiscountCode(int discountId, [FromBody] int quantity)
        {
            if (quantity <= 0)
            {
                return BadRequest(new { status = "error", message = "Invalid quantity" });
            }
            var result = await _discountRepository.CreateBatchDiscountCode(discountId, quantity);
            if (result > 0)
            {
                return Ok(new { status = "success", message = $"{result} discount codes created successfully" });
            }
            return BadRequest(new { status = "error", message = "Failed to create discount codes" });
        }

        // 0: thành công
        // 1: discount code không tồn tại
        // 2: discount code đã hết hạn
        // 3: discount code đã hết lượt sử dụng
        // 4: discount code đã được sử dụng
        [HttpPost("aplly")]
        public async Task<IActionResult> ApplyDiscount([FromBody] ApplyDiscountRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.DiscountCode))
            {
                return BadRequest(new { status = "error", message = "Mã giảm giá không hợp lệ" });
            }

            var result = await _discountRepository.ApplyDiscountCode(request.DiscountCode, request.UserId);

            return result switch
            {
                0 => Ok(new { status = "success", message = "Mã giảm giá đã được áp dụng thành công" }),
                1 => NotFound(new { status = "error", message = "Mã giảm giá không tồn tại" }),
                2 => StatusCode(410, new { status = "error", message = "Mã giảm giá đã hết hạn" }),
                3 => Conflict(new { status = "error", message = "Mã giảm giá đã hết lượt sử dụng" }),
                4 => Conflict(new { status = "error", message = "Mã giảm giá đã được sử dụng" }),
                _ => StatusCode(500, new { status = "error", message = "Lỗi không xác định" })
            };
        }

    }
}

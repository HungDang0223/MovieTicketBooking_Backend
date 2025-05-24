using Dapper;
using Microsoft.EntityFrameworkCore;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.ModelDTOs;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Repositories;

namespace MovieTicket_Backend.RepositoryImpl
{
    public class DiscountRepository
    {
        private readonly ApplicationDbContext _context;
        public DiscountRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Discount>> GetAllDiscounts(int page, int pageSize)
        {
            var discounts = await _context.Discounts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return discounts;
        }
        public async Task<Discount?> GetDiscountById(int discountId)
        {
            return await _context.Discounts.FindAsync(discountId);
        }

        // Get all code in discount_code table in table discount table has discount_code = null
        public async Task<List<DiscountCode>> GetCodesGenerated(int discountId)
        {
            var discountCodes = await _context.DiscountsCodes
                .Where(dc => dc.DiscountId == discountId)
                .ToListAsync();
            return discountCodes;
        }

        public async Task<bool> CreateDiscount(DiscountDTO discount)
        {
            var newDiscount = new Discount
            {
                DiscountCode = discount.DiscountCode,
                DiscountDescription = discount.DiscountDescription,
                DiscountType = discount.DiscountType,
                DiscountValue = discount.DiscountValue,
                MaxDiscount = discount.MaxDiscount,
                MinOrderValue = discount.MinOrderValue,
                Quantity = discount.Quantity,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate
            };
            var rowEffect = await _context.Discounts.AddAsync(newDiscount);
            if (discount.DiscountCode == null)
            {
                await CreateBatchDiscountCode(newDiscount.DiscountId, newDiscount.Quantity);
            }
            await _context.SaveChangesAsync();
            if (rowEffect != null)
            {
                return true;
            }
            return false;
        }

        public async Task<int> CreateBatchDiscountCode(int discountId, int quantity)
        {
            var discount = await _context.Discounts.FindAsync(discountId);
            if (discount == null)
            {
                return 0;
            }
            var discountCodes = new List<DiscountCode>();
            for (int i = 0; i < quantity; i++)
            {
                var code = Guid.NewGuid().ToString();
                discountCodes.Add(new DiscountCode
                {
                    Code = code,
                    DiscountId = discountId
                });
            }
            await _context.DiscountsCodes.AddRangeAsync(discountCodes);
            await _context.SaveChangesAsync();
            return quantity;
        }

        public async Task<bool> UpdateDiscount(int discountId, DiscountDTO discount)
        {
            var existingDiscount = await _context.Discounts.FindAsync(discountId);
            if (existingDiscount == null)
            {
                return false;
            }
            existingDiscount.DiscountCode = discount.DiscountCode;
            existingDiscount.DiscountDescription = discount.DiscountDescription;
            existingDiscount.DiscountType = discount.DiscountType;
            existingDiscount.DiscountValue = discount.DiscountValue;
            existingDiscount.MaxDiscount = discount.MaxDiscount;
            existingDiscount.MinOrderValue = discount.MinOrderValue;
            existingDiscount.Quantity = discount.Quantity;
            existingDiscount.StartDate = discount.StartDate;
            existingDiscount.EndDate = discount.EndDate;
            _context.Entry(existingDiscount).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }
        public Task<bool> DeleteDiscount(int discountId)
        {
            var discount = _context.Discounts.Find(discountId);
            if (discount == null)
            {
                return Task.FromResult(false);
            }
            _context.Discounts.Remove(discount);
            _context.SaveChanges();
            return Task.FromResult(true);
        }

        // user use discount code
        // code này sẽ được gọi khi user sử dụng discount code tùy code ở bảng discount hay discount_code
        // Trả về mã lỗi nếu không thành công
        // 0: thành công
        // 1: discount code không tồn tại
        // 2: discount code đã hết hạn
        // 3: discount code đã hết lượt sử dụng
        // 4: discount code đã được sử dụng
        public async Task<int> ApplyDiscountCode(string discountCode, string usedBy)
        {
            // code dùng discount_code trong bảng discount
            var discountCodeInDiscount = await _context.Discounts
                .FirstOrDefaultAsync(dc => dc.DiscountCode == discountCode);
            var discountCodeInDiscountCode = await _context.DiscountsCodes
                .FirstOrDefaultAsync(dc => dc.Code == discountCode);
            if (discountCodeInDiscount != null)
            {
                // code hết hạn
                if (discountCodeInDiscount.IsActive == false)
                {
                    return 2;
                }
                // code chưa được đưa vào ds -> không tồn tại
                if (discountCodeInDiscount.StartDate != null && discountCodeInDiscount.StartDate > DateTime.Now)
                {
                    return 1;
                }
                if (discountCodeInDiscount.EndDate != null && discountCodeInDiscount.EndDate < DateTime.Now)
                {
                    return 2;
                }
                // code đã hết lượt sd
                if (discountCodeInDiscount.Quantity == 0)
                {
                    return 3;
                }
                // cập nhật quantity trừ TH quantity = -100 ~ không giới hạn
                var a = await _context.Discounts
                    .Where(dc => dc.DiscountCode == discountCode)
                    .Where(dc => dc.Quantity != -100)
                    .ExecuteUpdateAsync(d => d.SetProperty(dc => dc.Quantity, dc => dc.Quantity - 1));
                return 0;
            }

            if (discountCodeInDiscountCode != null)
            {
                var discount = await _context.Discounts
                    .FirstOrDefaultAsync(dc => dc.DiscountId == discountCodeInDiscountCode.DiscountId);
                // code hết hạn
                if (discount.IsActive == false)
                {
                    return 2;
                }
                // code chưa được đưa vào sd -> không tồn tại
                if (discount.StartDate != null && discount.StartDate > DateTime.Now)
                {
                    return 1;
                }
                if (discount.EndDate != null && discount.EndDate < DateTime.Now)
                {
                    return 2;
                }
                // code đã được sd
                if (discountCodeInDiscountCode.IsUsed == true)
                {
                    return 4;
                }
                // cập nhật quantity trừ TH quantity = -100 ~ không giới hạn và người sd
                await _context.Discounts
                    .Where(dc => dc.DiscountCode == discountCode)
                    .ExecuteUpdateAsync(d => d.SetProperty(dc => dc.Quantity, dc => dc.Quantity - 1));
                await _context.DiscountsCodes
                    .Where(dc => dc.Code == discountCode)
                    .ExecuteUpdateAsync(d => d.SetProperty(dc => dc.IsUsed, dc => true)
                        .SetProperty(dc => dc.UsedBy, dc => usedBy)
                        .SetProperty(dc => dc.UsedAt, dc => DateTime.Now));
                return 0;
            }
            return 1;
        }
    }
}

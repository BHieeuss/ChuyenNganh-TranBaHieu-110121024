using HieuEMart.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HieuEMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Staff")]
    public class OrderController : Controller
	{
        private readonly DataContext _dataContext;
        public OrderController(DataContext context) {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
        }
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var detailsOrder = await _dataContext.OrderDetails
                .Include(o => o.Product)
                .Where(o => o.OrderCode == ordercode)
                .ToListAsync();

            var order = await _dataContext.Orders
                .FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            var billingAddress = await _dataContext.BillingAddresses
                .FirstOrDefaultAsync(b => b.OrderId == order.Id);

            ViewData["BillingAddress"] = billingAddress;

            return View(detailsOrder);
        }



        [HttpPost]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _dataContext.Orders.Update(order);
            await _dataContext.SaveChangesAsync();

            return Json(new { success = true, redirectUrl = Url.Action("Index") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrder(string ordercode)
        {
            // Tìm đơn hàng theo mã
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                TempData["error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index");
            }

            // Xóa chi tiết đơn hàng liên quan
            var orderDetails = await _dataContext.OrderDetails.Where(od => od.OrderCode == ordercode).ToListAsync();
            if (orderDetails.Any())
            {
                _dataContext.OrderDetails.RemoveRange(orderDetails);
            }

            // Xóa địa chỉ thanh toán liên quan (nếu có)
            var billingAddress = await _dataContext.BillingAddresses.FirstOrDefaultAsync(b => b.OrderId == order.Id);
            if (billingAddress != null)
            {
                _dataContext.BillingAddresses.Remove(billingAddress);
            }

            // Xóa đơn hàng
            _dataContext.Orders.Remove(order);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Đã xóa đơn hàng thành công.";
            return RedirectToAction("Index");
        }
    }
}

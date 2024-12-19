using HieuEMart.Repository;
using iText.Kernel.Pdf; 
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.IO.Font;
using iText.Kernel.Font;

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

        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var detailsOrder = await _dataContext.OrderDetails
                .Include(o => o.Product)
                .Where(o => o.OrderCode == ordercode)
                .ToListAsync();

            var order = await _dataContext.Orders
                .FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            var billingAddress = await _dataContext.BillingAddresses
                .FirstOrDefaultAsync(b => b.OrderId == order.Id);

            ViewData["BillingAddress"] = billingAddress;

            return View(detailsOrder);
        }


        [HttpGet]
        public async Task<IActionResult> ExportOrderToPdf(string ordercode)
        {
            if (string.IsNullOrWhiteSpace(ordercode))
            {
                Console.WriteLine("OrderCode is null or empty.");
                return BadRequest("Mã đơn hàng không hợp lệ.");
            }

            ordercode = ordercode.Trim();
            Console.WriteLine($"OrderCode received: {ordercode}");

            var order = await _dataContext.Orders
                .FirstOrDefaultAsync(o => o.OrderCode.Equals(ordercode, StringComparison.OrdinalIgnoreCase));

            if (order == null)
            {
                Console.WriteLine("Order not found.");
                return NotFound($"Không tìm thấy đơn hàng với mã: {ordercode}");
            }

            var orderDetails = await _dataContext.OrderDetails
                .Include(o => o.Product)
                .Where(o => o.OrderCode.Equals(ordercode, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            if (!orderDetails.Any())
            {
                Console.WriteLine("OrderDetails not found.");
                return NotFound($"Không tìm thấy thông tin chi tiết đơn hàng với mã: {ordercode}");
            }

            var billingAddress = await _dataContext.BillingAddresses
                .FirstOrDefaultAsync(b => b.OrderId == order.Id);

            if (billingAddress == null)
            {
                Console.WriteLine("BillingAddress not found.");
                return NotFound($"Không tìm thấy địa chỉ thanh toán cho đơn hàng với mã: {ordercode}");
            }

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph("Thông Tin Đơn Hàng").SetFontSize(20).SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph($"Mã Đơn Hàng: {ordercode}").SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("\n"));

                document.Add(new Paragraph("Thông Tin Khách Hàng:"));
                document.Add(new Paragraph($"- Họ và Tên: {billingAddress.FullName}"));
                document.Add(new Paragraph($"- Số Điện Thoại: {billingAddress.PhoneNumber}"));
                document.Add(new Paragraph($"- Địa Chỉ: {billingAddress.SpecificAddress}, {billingAddress.Ward}, {billingAddress.District}, {billingAddress.Province}"));

                var table = new Table(5).UseAllAvailableWidth();
                table.AddHeaderCell("Tên Sản Phẩm");
                table.AddHeaderCell("Giá Sản Phẩm");
                table.AddHeaderCell("Số Lượng");
                table.AddHeaderCell("Tổng");

                decimal total = 0;
                foreach (var item in orderDetails)
                {
                    decimal subtotal = item.Quantity * item.Price;
                    total += subtotal;

                    table.AddCell(item.Product.Name);
                    table.AddCell(item.Price.ToString("N0"));
                    table.AddCell(item.Quantity.ToString());
                    table.AddCell(subtotal.ToString("N0"));
                }

                table.AddFooterCell("Tổng Cộng:");
                table.AddFooterCell(total.ToString("N0"));

                document.Add(table);
                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", $"Order_{ordercode}.pdf");
            }
        }
    }
}

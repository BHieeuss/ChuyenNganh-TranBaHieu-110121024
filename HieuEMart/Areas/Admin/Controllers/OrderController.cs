using HieuEMart.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HieuEMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
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
            var detailsOrder = await _dataContext.OrderDetails.Include(o => o.Product).Where(o => o.OrderCode == ordercode).ToListAsync();
            return View(detailsOrder);
        }

    }
}

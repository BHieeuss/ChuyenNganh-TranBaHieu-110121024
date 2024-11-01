using HieuEMart.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HieuEMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    public class UserController : Controller
    {
        private readonly DataContext _dataContext;
        public UserController(DataContext context)
        {
            _dataContext = context;
        }

        [Route("Index")]
        public async Task <IActionResult> Index()
        {
            return View(await _dataContext.Users.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}

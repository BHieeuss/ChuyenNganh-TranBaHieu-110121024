using HieuEMart.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HieuEMart.Controllers
{
    public class ProductController : Controller
	{
		private readonly DataContext _dataContext;
		public ProductController(DataContext context)
		{
			_dataContext = context;

		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Details(int Id)
		{
			if (Id == null) return RedirectToAction("Index");

			var ProductById = _dataContext.Products.Where(p => p.Id == Id).FirstOrDefault();

			//Sản phẩm liên quan
			var relatedProducts = await _dataContext.Products
				.Where(p => p.CategoryId == ProductById.CategoryId && p.Id != ProductById.Id)
				.Take(4)
				.ToListAsync();
			ViewBag.RelatedProducts = relatedProducts;

			return View(ProductById);
		}

		public async Task<IActionResult> Search(string searchTerm)
		{
			var products = await _dataContext.Products
				.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
				.ToListAsync();
			ViewBag.Keyword = searchTerm;

			return View(products);
		}

	}
}

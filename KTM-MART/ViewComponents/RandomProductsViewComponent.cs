using KTM_MART.DataAccess;
using KTM_MART.Models;
using KTM_MART.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KTM_MART.ViewComponents
{
	public class RandomProductsViewComponent : ViewComponent
	{
		private readonly ApplicationDbContext _context;

		public RandomProductsViewComponent(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			List<Product> products = await _context.Products.FromSqlRaw("EXEC RandomProducts").ToListAsync();
			
			List<ProductVM> randomProducts = products.Select(p => new ProductVM
			{
				Product = p,
			}).ToList();

			return View(randomProducts);
		}


		//public async Task<IViewComponentResult> InvokeAsync()
		//{
		//	List<ProductVM> randomProducts = await _context.Products.FromSqlRaw("EXEC RandomProducts").ToListAsync();

		//	return View(randomProducts);
		//}
	}
}

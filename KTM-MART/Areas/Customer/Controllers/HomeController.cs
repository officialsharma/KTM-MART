using KTM_MART.DataAccess.Repository.IRepository;
using KTM_MART.Models;
using KTM_MART.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace KTM_MART.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");   

            return View(productList);
        }
	

		public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Details(int productId)
        {
            ShoppingCart CartObj = new()
            {
                Count = 1,
                ProductId = productId,
                Product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == productId, includeProperties: "Category"),

            };

            return View(CartObj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
		public IActionResult Details(ShoppingCart shoppingCart)
		{

                //shoppingCart.Id = 0; 
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
				shoppingCart.ApplicationUserId = claim.Value;

				ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
					u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);
            //var product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == shoppingCart.ProductId);


                if(cartFromDb == null)
				{
					_unitOfWork.ShoppingCart.Add(shoppingCart);
				    _unitOfWork.Save();

				//Setting Session for the shopping cart
				HttpContext.Session.SetInt32(SD.SessionCart,
                        _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);

				}
				else
				{
					_unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
				    _unitOfWork.Save();
  



			}
			return RedirectToAction(nameof(Index));
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
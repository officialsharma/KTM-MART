using KTM_MART.DataAccess.Repository.IRepository;
using KTM_MART.Models;
using KTM_MART.Models.ViewModels;
using KTM_MART.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace KTM_MART.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailSender _emailSender;
		[BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }
		public int OrderTotal { get; set; }
		public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
		{
			_unitOfWork = unitOfWork;
			_emailSender = emailSender;
		}
		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new ShoppingCartVM()
			{
				ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
				includeProperties: "Product"),
				OrderHeader = new()
		    };
			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
					cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.SubTotal += ( cart.Price * cart.Count);				
				ShoppingCartVM.VatTotal = (0.13 * ShoppingCartVM.SubTotal);
				ShoppingCartVM.OrderHeader.OrderTotal = (ShoppingCartVM.SubTotal + ShoppingCartVM.VatTotal + 100);
			}
			return View(ShoppingCartVM);
		}

		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			ShoppingCartVM = new ShoppingCartVM()
			{
				ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
				includeProperties: "Product"),
				OrderHeader = new()
			};
			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ViewBag.CashOnDelivery = "Cash On Delivery";
			ViewBag.Esewa = "Esewa";
			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
					cart.Product.Price50, cart.Product.Price100);				
				ShoppingCartVM.SubTotal += (cart.Price * cart.Count);
				ShoppingCartVM.VatTotal = (0.13 * ShoppingCartVM.SubTotal);
				ShoppingCartVM.OrderHeader.OrderTotal = (ShoppingCartVM.SubTotal + ShoppingCartVM.VatTotal + 100);
			}
			return View(ShoppingCartVM);
		}

		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
				includeProperties: "Product");
			ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
					cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.SubTotal += (cart.Price * cart.Count);
				ShoppingCartVM.VatTotal = (0.13 * ShoppingCartVM.SubTotal);
				ShoppingCartVM.OrderHeader.OrderTotal = (ShoppingCartVM.SubTotal + ShoppingCartVM.VatTotal + 100);
			}
			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
			else
			{
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}
			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();
			foreach (var cart in ShoppingCartVM.ListCart)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count,
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}
			string selectedPaymentMethod = Request.Form["payment_method"].ToString();

			if (applicationUser.CompanyId.GetValueOrDefault() == 0 && selectedPaymentMethod != "Cash on Delivery" && selectedPaymentMethod != "Esewa")
			{

				//Stripe Payement Settings
				var domain = "https://localhost:44339/";
				// Create the Payment Intent
				var paymentIntentService = new PaymentIntentService();
				var paymentIntentOptions = new PaymentIntentCreateOptions
				{
					Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal * 100),
					Currency = "usd",
					PaymentMethodTypes = new List<string> { "card" },
					Metadata = new Dictionary<string, string>
	                {
					   { "integration_check", "accept_a_payment" }
					}
				};
				var paymentIntent = paymentIntentService.Create(paymentIntentOptions);
				var options = new SessionCreateOptions
				{
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl = domain + $"customer/cart/index",
					
			};
				
				foreach (var item in ShoppingCartVM.ListCart)
				{
					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.ProductName
							},
						},
						Quantity = item.Count,
					};
					options.LineItems.Add(sessionLineItem);
				}
				var service = new SessionService();
				Session session = service.Create(options);
				PaymentIntent intent = paymentIntentService.Get(paymentIntent.Id);
				_unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, intent.Id);
				_unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			}
			else
			{
				return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
			}
		}


		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser");

			if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{
				var sessionService = new SessionService();
				Session session = sessionService.Get(orderHeader.SessionID);

				//Check the stripe status
				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}
			}

			_emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - KTM Mart", "<p>New Order From KTM MArt HAs been Created</p>");

			List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
			_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
			_unitOfWork.Save();

			OrderHeader confirmedOrder = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
			return View(confirmedOrder);
		}


		//public IActionResult OrderConfirmation(int id)
		//{
		//	OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);


		//	if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
		//	{
		//		var sessionService = new SessionService();
		//		Session session = sessionService.Get(orderHeader.SessionID);

		//		//Check the stripe status
		//		if (session.PaymentStatus.ToLower() == "paid")
		//		{
		//			_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
		//			_unitOfWork.Save();
		//		}
		//	}

		//	List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
		//	_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
		//	_unitOfWork.Save();

		//	OrderHeader confirmedOrder = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
		//	return View(confirmedOrder);
		//}
		public IActionResult Plus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
			_unitOfWork.ShoppingCart.IncrementCount(cart, 1);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Minus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
			if(cart.Count <= 1)
			{
				_unitOfWork.ShoppingCart.Remove(cart);
				var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count-1;
				HttpContext.Session.SetInt32(SD.SessionCart, count);
			}
			else
			{
				_unitOfWork.ShoppingCart.DecrementCount(cart, 1);
			}
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Remove(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
			_unitOfWork.ShoppingCart.Remove(cart);
			_unitOfWork.Save();
			var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
			HttpContext.Session.SetInt32(SD.SessionCart, count);
			return RedirectToAction(nameof(Index));
		}
		private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
		{
			if(quantity <= 50)
			{
				return price;
			}
			else
			{
				if(quantity <= 100)
				{
					return price50;
				}
				return price100;
			}
		}
	}
}

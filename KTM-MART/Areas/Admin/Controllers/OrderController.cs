using KTM_MART.DataAccess.Repository.IRepository;
using KTM_MART.Models;
using KTM_MART.Models.ViewModels;
using KTM_MART.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace KTM_MART.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public OrderVM OrderVM { get; set; }
		public OrderController(IUnitOfWork unitOfWork)
		{ 
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Details(int orderId)
		{
			OrderVM  = new OrderVM()
			{
				OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
				OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product"),
			};
			return View(OrderVM);
		}
		[ActionName("Details")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Details_PAY_NOW()
		{
			OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
			OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product");


			//Stripe Payement Settings
			var domain = "https://localhost:44339/";
			// Create the Payment Intent
			var paymentIntentService = new PaymentIntentService();
			var paymentIntentOptions = new PaymentIntentCreateOptions
			{
				Amount = Convert.ToInt32(OrderVM.OrderHeader.OrderTotal * 100),
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
				SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={OrderVM.OrderHeader.Id}",
				CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",

			};

			foreach (var item in OrderVM.OrderDetail)
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
			_unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, intent.Id);
			_unitOfWork.Save();
			Response.Headers.Add("Location", session.Url);
			TempData["Success"] = "Order Successfully Placed";
			return new StatusCodeResult(303);
		}

		public IActionResult PaymentConfirmation(int orderHeaderid)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);

			if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{
				var sessionService = new SessionService();
				Session session = sessionService.Get(orderHeader.SessionID);

				//Check the stripe status
				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}
			}

			//List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
			//_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
			//_unitOfWork.Save();

			return View(orderHeaderid);
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateOrderDetail(int orderId)
		{
			var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked:false);
			orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
			orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
			orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
			orderHeaderFromDb.City = OrderVM.OrderHeader.City;
			orderHeaderFromDb.State = OrderVM.OrderHeader.State;
			orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
			if(OrderVM.OrderHeader.Carrier != null)
			{
				orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
			}
			if (OrderVM.OrderHeader.TrackingNumber != null)
			{
				orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			}
			_unitOfWork.OrderHeader.Update(orderHeaderFromDb);
			_unitOfWork.Save();
			TempData["Success"] = "Order Successfully Updated";
			return RedirectToAction("Details", "Order", new {orderId = orderHeaderFromDb.Id});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StartProcessing()
		{
		    _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
			_unitOfWork.Save();
			TempData["Success"] = "Order Successfully Updated";
			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		}

	

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ShipOrder()
		{
			var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
			orderHeader.TrackingNumber =OrderVM.OrderHeader.TrackingNumber;
			orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
			orderHeader.OrderStatus = SD.StatusShipped;
			orderHeader.ShippingDate = DateTime.Now;
			if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{
				orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
			}
			_unitOfWork.OrderHeader.Update(orderHeader);
			_unitOfWork.Save();
			TempData["Warning"] = "Order Successfully Shipped";
			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		}

		//      [HttpPost]
		//[Authorize(Roles =  SD.Role_Admin + "," + SD.Role_Employee)]
		//      [ValidateAntiForgeryToken]
		//      public IActionResult CancelOrder()
		//      {
		//          var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
		//	// Retrieve the PaymentIntent
		//	var paymentIntentService = new PaymentIntentService();
		//	var paymentIntent = paymentIntentService.Get("paymentIntent.Id");

		//	// Retrieve the Charge object associated with the PaymentIntent
		//	var chargeService = new ChargeService();
		//	var charge = chargeService.Get(paymentIntent.Id);
		//	if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
		//	{
		//		var options = new RefundCreateOptions
		//		{
		//			Reason = RefundReasons.RequestedByCustomer,
		//			PaymentIntent = orderHeader.PaymentIntentId
		//		};
		//		var service = new RefundService();
		//		Refund refund = service.Create(options);
		//		_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
		//	}
		//	else
		//	{
		//              _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);

		//          }
		//          _unitOfWork.Save();
		//          TempData["error"] = "Order Cancelled.";
		//          return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		//      }

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult CancelOrder()
		{
			var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);

			if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
			{
				//var options = new RefundCreateOptions
				//{
				//	Reason = RefundReasons.RequestedByCustomer,
				//	PaymentIntent = orderHeader.PaymentIntentId,
				//};
				//var service = new RefundService();
				//Refund refund = service.Create(options);
				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
			}

			_unitOfWork.Save();
			TempData["error"] = "Order Cancelled.";
			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		}

		//[HttpPost]
		//[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		//[ValidateAntiForgeryToken]
		//public IActionResult CancelOrder()
		//{
		//	var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);

		//	if (orderHeader == null)
		//	{
		//		TempData["error"] = "Order not found.";
		//		return RedirectToAction("Index", "Order");
		//	}

		//	if (orderHeader.PaymentIntentId == null)
		//	{
		//		TempData["error"] = "No payment intent associated with order.";
		//		return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		//	}

		//	// Retrieve the PaymentIntent
		//	var paymentIntentService = new PaymentIntentService();
		//	var paymentIntent = paymentIntentService.Get(orderHeader.PaymentIntentId);

		//	if (paymentIntent == null)
		//	{
		//		TempData["error"] = "Payment intent not found.";
		//		return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		//	}

		//	// Retrieve the Charges associated with the PaymentIntent
		//	var chargeService = new ChargeService();
		//	var chargeListOptions = new ChargeListOptions { PaymentIntent = paymentIntent.Id };
		//	var chargeList = chargeService.List(chargeListOptions);
		//	var charges = chargeList.Data;

		//	if (charges.Count > 0)
		//	{
		//		var charge = charges[0];
		//		if (charge.Status == "succeeded")
		//		{
		//			// Cancel the PaymentIntent and Refund the Charge
		//			var options = new PaymentIntentCancelOptions();
		//			paymentIntent = paymentIntentService.Cancel(paymentIntent.Id, options);

		//			var refundService = new RefundService();
		//			var refund = refundService.Create(new RefundCreateOptions { Charge = charge.Id });

		//			_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
		//			_unitOfWork.Save();

		//			TempData["success"] = "Order Cancelled and Payment Refunded.";
		//			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		//		}
		//		else
		//		{
		//			TempData["error"] = "Cannot cancel order - charge has not succeeded.";
		//			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		//		}
		//	}
		//	else
		//	{
		//		TempData["error"] = "Cannot cancel order - no charges associated with payment intent.";
		//		return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		//	}
		//}

		//[HttpPost]
		//[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		//[ValidateAntiForgeryToken]
		//public IActionResult CancelOrder()
		//{
		//	var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);

		//	// Retrieve the PaymentIntent
		//	var paymentIntentService = new PaymentIntentService();
		//	var paymentIntent = paymentIntentService.Get(orderHeader.PaymentIntentId);

		//	// Retrieve the Charges associated with the PaymentIntent
		//	var chargeService = new ChargeService();
		//	var chargeListOptions = new ChargeListOptions { PaymentIntent = paymentIntent.Id };

		//	var chargeList = chargeService.List(chargeListOptions);
		//	var charges = chargeList.Data;

		//	if (charges.Count > 0)
		//	{
		//		var charge = charges[0];
		//		if (charge.Status == "succeeded")
		//		{
		//			if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
		//			{
		//				// Refund the PaymentIntent
		//				var options = new RefundCreateOptions
		//				{
		//					Reason = RefundReasons.RequestedByCustomer,
		//					PaymentIntent = orderHeader.PaymentIntentId
		//				};
		//				var refundService = new RefundService();
		//				var refund = refundService.Create(options);
		//				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
		//			}
		//			else
		//			{
		//				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
		//			}

		//			_unitOfWork.Save();
		//			TempData["error"] = "Order Cancelled.";
		//			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		//		}
		//		else
		//		{
		//			TempData["error"] = "Cannot refund payment - charge has not succeeded.";
		//			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		//		}
		//	}
		//	else
		//	{
		//		TempData["error"] = "Cannot refund payment - no charges associated with payment intent.";
		//		return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		//	}
		//}










		#region API CALLS

		[HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeaders;
			if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

            }
			else
			{
				var ClaimsIdentity = (ClaimsIdentity)User.Identity;
				var claim = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
				orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
			}

			

			switch (status)
			{
                case "pending":
					orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:                
                    break;
            }
			return Json(new { data = orderHeaders });
		}
		#endregion
	}
}

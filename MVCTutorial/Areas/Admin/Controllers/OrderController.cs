using System.Collections;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Models;
using MVCTutorial.Repository;
using MVCTutorial.Utility;
using Stripe;
using Stripe.Checkout;

namespace MVCTutorial.Areas.Admin.Controllers;

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
        OrderVM = new OrderVM()
        {
            OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(filter:u=>u.Id==orderId, includeProperties:"ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetail.GetAll(u=>u.OrderId==orderId, includeProperties:"Product")
        };
        return View(OrderVM);
    }
    
    [ActionName("Details")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DetailsPayNow()
    {
        OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(filter: u => u.Id == OrderVM.OrderHeader.Id,
            includeProperties: "ApplicationUser");
        OrderVM.OrderDetails =
            _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product");
        
        var domain = "https://localhost:8080/";
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
            CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
        };
        foreach (OrderDetail detail in OrderVM.OrderDetails)
        {
            var sessionLimeItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(detail.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = detail.Product.Title,
                    },

                },
                Quantity = detail.Count,
            };
            options.LineItems.Add(sessionLimeItem);
        }

        var service = new SessionService();
        Session session = service.Create(options);

        _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id,
            session.PaymentIntentId);
        _unitOfWork.Save();

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }
    
    public IActionResult PaymentConfirmation(int orderHeaderId)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == orderHeaderId);
        if (orderHeader.PaymentStatus == Util.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, Util.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }
        return View(orderHeaderId);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Util.ROLE_ADMIN + "," + Util.ROLE_EMPLOYEE)]
    public IActionResult UpdateOrderDetail()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked:false);
        orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
        orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
        orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
        orderHeaderFromDb.City = OrderVM.OrderHeader.City;
        orderHeaderFromDb.State = OrderVM.OrderHeader.State;
        orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
        if (OrderVM.OrderHeader.Carrier != null)
        {
            orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
        }
        if (OrderVM.OrderHeader.TrackingNumber != null)
        {
            orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
        }
        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        _unitOfWork.Save();
        
        TempData["Success"] = "Order Details Updated Successfully.";
        return RedirectToAction("Details", new { orderId = orderHeaderFromDb.Id });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Util.ROLE_ADMIN + "," + Util.ROLE_EMPLOYEE)]
    public IActionResult StartProcessing()
    {
        _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, Util.StatusInProcess);
        _unitOfWork.Save();
        
        TempData["Success"] = "Order Status Updated Successfully.";
        return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.Id });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Util.ROLE_ADMIN + "," + Util.ROLE_EMPLOYEE)]
    public IActionResult ShipOrder()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked:false);
        orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
        orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
        orderHeaderFromDb.OrderStatus = Util.StatusShipped;
        orderHeaderFromDb.ShippingDate = DateTime.Now;
        if (orderHeaderFromDb.PaymentStatus == Util.PaymentStatusDelayedPayment)
        {
            orderHeaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
        }
        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        _unitOfWork.Save();
        
        TempData["Success"] = "Order Shipped Successfully.";
        return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.Id });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Util.ROLE_ADMIN + "," + Util.ROLE_EMPLOYEE)]
    public IActionResult CancelOrder()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked:false);
        if (orderHeaderFromDb.PaymentStatus == Util.StatusApproved)
        {
            var options = new RefundCreateOptions()
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = orderHeaderFromDb.PaymentIntentId
            };
            
            var service = new RefundService();
            Refund refund = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, Util.StatusCancelled, Util.StatusRefunded);
        }
        else
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, Util.StatusCancelled, Util.StatusCancelled);
        }
        _unitOfWork.Save();
        
        TempData["Success"] = "Order Canceled Successfully.";
        return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.Id });
    }


    #region API CALLS

    [HttpGet]
    public IActionResult GetAll(string? status)
    {
        IEnumerable<OrderHeader> orderHeaders;
        if (User.IsInRole(Util.ROLE_ADMIN) || User.IsInRole(Util.ROLE_EMPLOYEE))
        {
            orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
        }
        else
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            orderHeaders = _unitOfWork.OrderHeader.GetAll(filter: u => u.ApplicationUserId == claim.Value,
                includeProperties: "ApplicationUser");
        }

        switch (status)
        {
            case "pending":
                orderHeaders = orderHeaders.Where(u => u.OrderStatus == Util.StatusPending);
                break;
            case "inprocess":
                orderHeaders = orderHeaders.Where(u => u.OrderStatus == Util.StatusInProcess);
                break;
            case "completed":
                orderHeaders = orderHeaders.Where(u => u.OrderStatus == Util.StatusShipped);
                break;
            case "approved":
                orderHeaders = orderHeaders.Where(u => u.OrderStatus == Util.StatusApproved);
                break;
        }

        return Json(new { data = orderHeaders });
    }

    #endregion
}
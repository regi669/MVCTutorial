using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Models;
using MVCTutorial.Models.ViewModels;
using MVCTutorial.Repository;
using MVCTutorial.Utility;
using Stripe.Checkout;

namespace MVCTutorial.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
    public ShoppingCartVM ShoppingCartVm { get; set; }

    public CartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        ShoppingCartVm = new ShoppingCartVM()
        {
            ListCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, "Product"),
            OrderHeader = new OrderHeader()
        };
        foreach (ShoppingCart cart in ShoppingCartVm.ListCarts)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                cart.Product.Price100);
            ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }
        return View(ShoppingCartVm);
    }

    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        ShoppingCartVm = new ShoppingCartVM()
        {
            ListCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, "Product"),
            OrderHeader = new OrderHeader()
        };

        ShoppingCartVm.OrderHeader.ApplicationUser = _unitOfWork.Users.GetFirstOrDefault(
            u => u.Id == claim.Value);
        ShoppingCartVm.OrderHeader.Name = ShoppingCartVm.OrderHeader.ApplicationUser.Name;
        ShoppingCartVm.OrderHeader.PhoneNumber = ShoppingCartVm.OrderHeader.ApplicationUser.PhoneNumber;
        ShoppingCartVm.OrderHeader.StreetAddress = ShoppingCartVm.OrderHeader.ApplicationUser.StreetAdress;
        ShoppingCartVm.OrderHeader.City = ShoppingCartVm.OrderHeader.ApplicationUser.City;
        ShoppingCartVm.OrderHeader.State = ShoppingCartVm.OrderHeader.ApplicationUser.State;
        ShoppingCartVm.OrderHeader.PostalCode = ShoppingCartVm.OrderHeader.ApplicationUser.PostalCode;
        
        foreach (ShoppingCart cart in ShoppingCartVm.ListCarts)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                cart.Product.Price100);
            ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }
        return View(ShoppingCartVm);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SummaryPOST()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        
        ShoppingCartVm.ListCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, "Product");
        ShoppingCartVm.OrderHeader.OrderDate = DateTime.Now;
        ShoppingCartVm.OrderHeader.ApplicationUserId = claim.Value;
        foreach (ShoppingCart cart in ShoppingCartVm.ListCarts)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                cart.Product.Price100);
            ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }
        ApplicationUser applicationUser = _unitOfWork.Users.GetFirstOrDefault(u => u.Id == claim.Value);
        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            ShoppingCartVm.OrderHeader.PaymentStatus = Util.PaymentStatusPending;
            ShoppingCartVm.OrderHeader.OrderStatus = Util.StatusPending;
        }
        else
        {
            ShoppingCartVm.OrderHeader.PaymentStatus = Util.PaymentStatusDelayedPayment;
            ShoppingCartVm.OrderHeader.OrderStatus = Util.StatusApproved;
        }
        _unitOfWork.OrderHeader.Add(ShoppingCartVm.OrderHeader);
        _unitOfWork.Save();

        foreach (ShoppingCart cart in ShoppingCartVm.ListCarts)
        {
            OrderDetail orderDetail = new OrderDetail()
            {
                ProductId = cart.ProductId,
                OrderId = ShoppingCartVm.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count
            };
            _unitOfWork.OrderDetail.Add(orderDetail);
            _unitOfWork.Save();
        }

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            var domain = "https://localhost:8080/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVm.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
            };
            foreach (ShoppingCart cart in ShoppingCartVm.ListCarts)
            {
                var sessionLimeItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(cart.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = cart.Product.Title,
                        },

                    },
                    Quantity = cart.Count,
                };
                options.LineItems.Add(sessionLimeItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVm.OrderHeader.Id, session.Id,
                session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        return RedirectToAction(nameof(OrderConfirmation), routeValues: new { id = ShoppingCartVm.OrderHeader.Id });
    }

    public IActionResult OrderConfirmation(int id)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == id);
        if (orderHeader.PaymentStatus != Util.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(id, Util.StatusApproved, Util.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }
        
        List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
            .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
        _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
        _unitOfWork.Save();
        return View(id);
    }

    public IActionResult Plus(int id)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == id);
        _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
        _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }
    
    public IActionResult Minus(int id)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == id);
        _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
        if (cart.Count < 1)
        {
            _unitOfWork.ShoppingCart.Remove(cart);
        }
        _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }
    
    public IActionResult Remove(int id)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == id);
        _unitOfWork.ShoppingCart.Remove(cart);
        _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }

    private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
    {
        if (quantity <= 50)
        {
            return price;
        }
        if (quantity > 50 && quantity <= 100)
        {
            return price50;
        }
        return price100;
    }
}
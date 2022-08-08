using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Models;
using MVCTutorial.Models.ViewModels;
using MVCTutorial.Repository;

namespace MVCTutorial.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        ShoppingCartVM cartVm = new ShoppingCartVM()
        {
            ListCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, "Product")
        };
        foreach (ShoppingCart cart in cartVm.ListCarts)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                cart.Product.Price100);
            cartVm.CartTotal += (cart.Price * cart.Count);
        }
        return View(cartVm);
    }

    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        ShoppingCartVM cartVm = new ShoppingCartVM()
        {
            ListCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, "Product")
        };
        foreach (ShoppingCart cart in cartVm.ListCarts)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                cart.Product.Price100);
            cartVm.CartTotal += (cart.Price * cart.Count);
        }
        return View(cartVm);
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
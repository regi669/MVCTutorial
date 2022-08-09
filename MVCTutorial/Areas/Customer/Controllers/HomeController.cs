using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Models;
using MVCTutorial.Models.ViewModels;
using MVCTutorial.Repository;
using MVCTutorial.Utility;

namespace MVCTutorial.Controllers;

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
        IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
        return View(products);
    }

    public IActionResult Details(int productId)
    {
        var cart = new ShoppingCart()
        {
            Product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == productId, "Category,CoverType"),
            Count = 1,
            ProductId = productId
        };
        return View(cart);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Details(ShoppingCart cart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        cart.ApplicationUserId = claim.Value;

        var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => 
            c.ApplicationUserId == claim.Value && c.ProductId == cart.ProductId);
        if (cartFromDb is null)
        {
            _unitOfWork.ShoppingCart.Add(cart);
            _unitOfWork.Save();
            var numberOfItems = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count();
            HttpContext.Session.SetInt32(Util.SessionCart, numberOfItems);
        }
        else
        {
            _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, cart.Count);
            _unitOfWork.Save();
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
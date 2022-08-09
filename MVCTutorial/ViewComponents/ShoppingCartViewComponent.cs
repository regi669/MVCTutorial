using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Repository;
using MVCTutorial.Utility;

namespace MVCTutorial.ViewComponents;

public class ShoppingCartViewComponent : ViewComponent
{
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        if (claim != null)
        {
            if (HttpContext.Session.GetInt32(Util.SessionCart) != null)
            {
                return View((int)HttpContext.Session.GetInt32(Util.SessionCart));
            }
            var numberOfItems = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count();
            HttpContext.Session.SetInt32(Util.SessionCart, numberOfItems);
            return View((int)HttpContext.Session.GetInt32(Util.SessionCart));
        }
        HttpContext.Session.Clear();
        return View(0);
    } 
}
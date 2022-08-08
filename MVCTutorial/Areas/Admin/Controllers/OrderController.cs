using System.Collections;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Models;
using MVCTutorial.Repository;
using MVCTutorial.Utility;

namespace MVCTutorial.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
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
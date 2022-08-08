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
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateOrderDetail()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
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
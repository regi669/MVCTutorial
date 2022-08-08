using System.Collections;
using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Models;
using MVCTutorial.Repository;
using MVCTutorial.Utility;

namespace MVCTutorial.Areas.Admin.Controllers;

[Area("Admin")]
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
        orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties:"ApplicationUser");
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
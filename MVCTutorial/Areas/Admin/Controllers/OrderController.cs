﻿using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Repository;

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
    public IActionResult GetAll()
    {
        var orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties:"ApplicationUser");
        return Json(new { data = orderHeaders });
    }
    #endregion
}
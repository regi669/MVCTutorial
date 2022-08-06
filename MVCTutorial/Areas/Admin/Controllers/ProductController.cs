using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCTutorial.Models;
using MVCTutorial.Repository;

namespace MVCTutorial.Controllers;

public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> products = _unitOfWork.Product.GetAll();
        return View(products);
    }

    public IActionResult Upsert(int? id)
    {
        IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(
            u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
        IEnumerable<SelectListItem> CoverTypeList = _unitOfWork.CoverType.GetAll().Select(
            u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });

        Product product = new Product();
        if (id is null || id == 0)
        {
            ViewBag.CategoryList = CategoryList;
            ViewBag.CoverTypeList = CoverTypeList;
            return View(product);
        }
        else
        {
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(Product product)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.Product.Update(product);
            _unitOfWork.Save();
            TempData["success"] = "Cover Type Updated Successfully";
            return RedirectToAction("Index");
        }

        return View(product);
    }

    public IActionResult Delete(int? id)
    {
        if (id is null || id == 0)
        {
            return NotFound("Cover Type Not Found");
        }

        var productFromDb = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
        if (productFromDb is null)
        {
            return NotFound();
        }

        return View(productFromDb);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var productFromDb = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
        if (productFromDb is null)
        {
            return NotFound();
        }

        _unitOfWork.Product.Remove(productFromDb);
        _unitOfWork.Save();
        TempData["success"] = "Cover Type Deleted Successfully";
        return RedirectToAction("Index");
    }
}
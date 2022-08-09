using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Data;
using MVCTutorial.Models;
using MVCTutorial.Repository;
using MVCTutorial.Utility;


namespace MVCTutorial.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Util.ROLE_ADMIN)]
public class CategoryController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public IActionResult Index()
    {
        IEnumerable<Category> categories = _unitOfWork.Category.GetAll();
        return View(categories);
    }
    
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category category)
    {
        if (category.Name == category.DisplayOrder.ToString())
        {
            ModelState.AddModelError("Name", "The DisplayOrder cannot exactly mach the Name");
        }
        if (ModelState.IsValid)
        {
            _unitOfWork.Category.Add(category);
            _unitOfWork.Save();
            TempData["success"] = "Category Created Successfully";
            return RedirectToAction("Index");
        }

        return View(category);
    }
    
    public IActionResult Edit(int? id)
    {
        if (id is null || id == 0)
        {
            return NotFound("Category Not Found");
        }

        var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
        if (categoryFromDb is null)
        {
            return NotFound();
        }
        return View(categoryFromDb);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category category)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.Category.Update(category);
            _unitOfWork.Save();
            TempData["success"] = "Category Updated Successfully";
            return RedirectToAction("Index");
        }

        return View(category);
    }
    
    public IActionResult Delete(int? id)
    {
        if (id is null || id == 0)
        {
            return NotFound("Category Not Found");
        }

        var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
        if (categoryFromDb is null)
        {
            return NotFound();
        }
        return View(categoryFromDb);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
        if (categoryFromDb is null)
        {
            return NotFound();
        }
        _unitOfWork.Category.Remove(categoryFromDb);
        _unitOfWork.Save();
        TempData["success"] = "Category Deleted Successfully";
        return RedirectToAction("Index");
    }
}
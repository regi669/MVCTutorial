using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Models;
using MVCTutorial.Repository;

namespace MVCTutorial.Controllers;

[Area("Admin")]
public class CoverTypeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CoverTypeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public IActionResult Index()
    {
        IEnumerable<CoverType> coverTypes = _unitOfWork.CoverType.GetAll();
        return View(coverTypes);
    }
    
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CoverType coverType)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.CoverType.Add(coverType);
            _unitOfWork.Save();
            TempData["success"] = "Cover Type Created Successfully";
            return RedirectToAction("Index");
        }

        return View(coverType);
    }
    
    public IActionResult Edit(int? id)
    {
        if (id is null || id == 0)
        {
            return NotFound("Cover Type Not Found");
        }

        var coverTypeFromDb = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
        if (coverTypeFromDb is null)
        {
            return NotFound();
        }
        return View(coverTypeFromDb);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(CoverType coverType)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.CoverType.Update(coverType);
            _unitOfWork.Save();
            TempData["success"] = "Cover Type Updated Successfully";
            return RedirectToAction("Index");
        }

        return View(coverType);
    }
    
    public IActionResult Delete(int? id)
    {
        if (id is null || id == 0)
        {
            return NotFound("Cover Type Not Found");
        }

        var coverTypeFromDb = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
        if (coverTypeFromDb is null)
        {
            return NotFound();
        }
        return View(coverTypeFromDb);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var coverTypeFromDb = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
        if (coverTypeFromDb is null)
        {
            return NotFound();
        }
        _unitOfWork.CoverType.Remove(coverTypeFromDb);
        _unitOfWork.Save();
        TempData["success"] = "Cover Type Deleted Successfully";
        return RedirectToAction("Index");
    }
}
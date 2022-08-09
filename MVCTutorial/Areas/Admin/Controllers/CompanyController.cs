using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Models;
using MVCTutorial.Repository;
using MVCTutorial.Utility;

namespace MVCTutorial.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Util.ROLE_ADMIN)]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CompanyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public IActionResult Index()
    {
        IEnumerable<Company> companies = _unitOfWork.Companies.GetAll();
        return View(companies);
    }
    
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Company company)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.Companies.Add(company);
            _unitOfWork.Save();
            TempData["success"] = "Company Created Successfully";
            return RedirectToAction("Index");
        }

        return View(company);
    }
    
    public IActionResult Edit(int? id)
    {
        if (id is null || id == 0)
        {
            return NotFound("Company Type Not Found");
        }

        var companyFromDb = _unitOfWork.Companies.GetFirstOrDefault(c => c.Id == id);
        if (companyFromDb is null)
        {
            return NotFound();
        }
        return View(companyFromDb);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Company company)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.Companies.Update(company);
            _unitOfWork.Save();
            TempData["success"] = "Company Updated Successfully";
            return RedirectToAction("Index");
        }

        return View(company);
    }
    
    public IActionResult Delete(int? id)
    {
        if (id is null || id == 0)
        {
            return NotFound("Company Not Found");
        }

        var companyFromDb = _unitOfWork.Companies.GetFirstOrDefault(c => c.Id == id);
        if (companyFromDb is null)
        {
            return NotFound();
        }
        return View(companyFromDb);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var companyFromDb = _unitOfWork.Companies.GetFirstOrDefault(c => c.Id == id);
        if (companyFromDb is null)
        {
            return NotFound();
        }
        _unitOfWork.Companies.Remove(companyFromDb);
        _unitOfWork.Save();
        TempData["success"] = "Company Deleted Successfully";
        return RedirectToAction("Index");
    }
}
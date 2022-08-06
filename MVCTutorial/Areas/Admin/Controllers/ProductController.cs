using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCTutorial.Models;
using MVCTutorial.Models.ViewModels;
using MVCTutorial.Repository;

namespace MVCTutorial.Controllers;

public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;

    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> products = _unitOfWork.Product.GetAll();
        return View(products);
    }

    public IActionResult Upsert(int? id)
    {
        ProductVM productVm = new ProductVM()
        {
            Product = new Product(),
            CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }),
            CoverTypeList = _unitOfWork.CoverType.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            })
        };
        if (id is null || id == 0)
        {
            return View(productVm);
        }
        else
        {
            
        }

        return View(productVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(ProductVM productVm, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            var wwwRootPath = _environment.WebRootPath;
            if (file is not null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploadPath = Path.Combine(wwwRootPath, @"images\products");
                var extension = Path.GetExtension(file.FileName);

                using (var fileStreams = new FileStream(Path.Combine(uploadPath, fileName+extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }

                productVm.Product.ImageUrl = @"\images\products\" + fileName + extension;
            }
            _unitOfWork.Product.Add(productVm.Product);
            _unitOfWork.Save();
            TempData["success"] = "Product created Successfully";
            return RedirectToAction("Index");
        }

        return View(productVm);
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
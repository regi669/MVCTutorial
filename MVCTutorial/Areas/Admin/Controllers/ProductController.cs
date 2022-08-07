using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCTutorial.Models;
using MVCTutorial.Models.ViewModels;
using MVCTutorial.Repository;

namespace MVCTutorial.Areas.Admin.Controllers;

[Area("Admin")]
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
        return View();
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
        productVm.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
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

                if (productVm.Product.ImageUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, productVm.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                using (var fileStreams = new FileStream(Path.Combine(uploadPath, fileName+extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }

                productVm.Product.ImageUrl = @"\images\products\" + fileName + extension;
            }

            if (productVm.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productVm.Product);
                TempData["success"] = "Product created Successfully";
            }
            else
            {
                _unitOfWork.Product.Update(productVm.Product);
                TempData["success"] = "Product updated Successfully";
            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        return View(productVm);
    }
    
    #region API CALLS
    [HttpGet]
    public IActionResult GetAll()
    {
        var products = _unitOfWork.Product.GetAll("Category");
        return Json(new { data = products });
    }
    
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var productFromDb = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
        if (productFromDb is null)
        {
            return Json(new { success = false, message = "Error while deleting!" });
        }
        if (productFromDb.ImageUrl != null)
        {
            var oldImagePath = Path.Combine(_environment.WebRootPath, productFromDb.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
        }
        _unitOfWork.Product.Remove(productFromDb);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Deleted Succesfully" });
    }
    #endregion
}
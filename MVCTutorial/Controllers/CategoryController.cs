using Microsoft.AspNetCore.Mvc;
using MVCTutorial.Database;
using MVCTutorial.Entities;

namespace MVCTutorial.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public IActionResult Index()
    {
        IEnumerable<Category> categories = _dbContext.Categories.ToList();
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
            _dbContext.Add(category);
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(category);
    }
}
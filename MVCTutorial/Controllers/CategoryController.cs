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
            _dbContext.Categories.Add(category);
            _dbContext.SaveChanges();
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

        var categoryFromDb = _dbContext.Categories.SingleOrDefault(c => c.Id == id);
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
            _dbContext.Categories.Update(category);
            _dbContext.SaveChanges();
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

        var categoryFromDb = _dbContext.Categories.SingleOrDefault(c => c.Id == id);
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
        var categoryFromDb = _dbContext.Categories.SingleOrDefault(c => c.Id == id);
        if (categoryFromDb is null)
        {
            return NotFound();
        }
        _dbContext.Categories.Remove(categoryFromDb);
        _dbContext.SaveChanges();
        return RedirectToAction("Index");
    }
}
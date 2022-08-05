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
}
using System.Linq.Expressions;
using MVCTutorial.Data;
using MVCTutorial.Models;

namespace MVCTutorial.Repository.Implementation;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    
    public void Update(Category category)
    {
        _dbContext.Categories.Update(category);
    }
}
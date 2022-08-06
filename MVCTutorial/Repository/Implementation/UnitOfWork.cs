using MVCTutorial.DataAccess.Data;

namespace MVCTutorial.Repository.Implementation;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    public ICategoryRepository Category { get; }

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        Category = new CategoryRepository(_dbContext);
    }
    
    public void Save()
    {
        _dbContext.SaveChanges();
    }
}
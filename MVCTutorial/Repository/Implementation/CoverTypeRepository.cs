using MVCTutorial.Data;
using MVCTutorial.Models;

namespace MVCTutorial.Repository.Implementation;

public class CoverTypeRepository : Repository<CoverType>,ICoverTypeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CoverTypeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Update(CoverType coverType)
    {
        _dbContext.CoverTypes.Update(coverType);
    }
}
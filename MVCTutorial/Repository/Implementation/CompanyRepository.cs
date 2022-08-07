using MVCTutorial.Data;
using MVCTutorial.Models;

namespace MVCTutorial.Repository.Implementation;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CompanyRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Update(Company company)
    {
        _dbContext.Companies.Update(company);
    }
}
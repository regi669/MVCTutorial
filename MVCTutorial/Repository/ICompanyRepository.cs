using MVCTutorial.Models;

namespace MVCTutorial.Repository;

public interface ICompanyRepository : IRepository<Company>
{
    void Update(Company company);
}